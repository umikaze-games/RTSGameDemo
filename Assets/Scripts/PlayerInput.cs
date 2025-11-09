using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
	[SerializeField] private Rigidbody cameraTarget;
	[SerializeField] private CinemachineCamera cinemachineCamera;
	[SerializeField] private new Camera camera;
	[SerializeField] private LayerMask selectableUnitsLayers;
	[SerializeField] private LayerMask floorLayers;
	[SerializeField] private RectTransform selectionBox;

	[SerializeField] private float moveSpeed = 5;
	[SerializeField] private float zoomSpeed = 1;
	[SerializeField] private float rotateSpeed = 1;
	[SerializeField] private float minZoomDistance = 7.5f;
	[SerializeField] private float mousePanSpeed = 5;
	[SerializeField] private float edgePanSize = 50;

	private Vector2 startMousePosition;
	private HashSet<AbstractUnit> aliveUnits=new (100);
	private HashSet<AbstractUnit> addedUnits = new(24);

	private List<ISelectable> selectedUnits=new (12);
	private bool enableEdgePan=true;
	private float zoomStartTime;
	private Vector3 startingFollowOffset;
	private CinemachineFollow cinemachineFollow;
	private float maxRotationAmount;
	private float rotateStartTime;

	private void Awake()
	{
		cinemachineFollow= cinemachineCamera.GetComponent<CinemachineFollow>();
		startingFollowOffset= cinemachineFollow.FollowOffset;
		maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
		Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
		Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
		Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
	}


	private void Update()
	{
		HandleRotating();
		HandlePanning();
		HandleZooming();
		HandleRightClick();
		HandleDragSelect();
	}

	private void OnDestroy()
	{
		Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
		Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
		Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
	}

	private void HandleUnitSpawn(UnitSpawnEvent evt)
	{
		aliveUnits.Add(evt.Unit);
	}
	private void HandleUnitDeselected(UnitDeselectedEvent evt)
	{
		selectedUnits.Remove(evt.Unit);
	}

	private void HandleUnitSelected(UnitSelectedEvent evt)
	{
		selectedUnits.Add(evt.Unit);
	}

	private void HandleDragSelect()
	{
		if (selectionBox==null) return;
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			HandleMouseDown();
		}

		else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
		{
			HandleMouseDrag();
		}

		else if (Mouse.current.leftButton.wasReleasedThisFrame && !Mouse.current.leftButton.wasPressedThisFrame)
		{
			HandleMouseUp();

		}

	}

	private void HandleMouseUp()
	{
		if (!Keyboard.current.shiftKey.isPressed)
		{
			DeselectAllUnits();
		}

		HandleLeftClick();
		foreach (AbstractUnit unit in addedUnits)
		{
			unit.Select();
		}
		selectionBox.gameObject.SetActive(false);
	}

	private void HandleMouseDrag()
	{
		Bounds selectionBoxBounds = ResizeSelectionBox();
		foreach (var unit in aliveUnits)
		{
			Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
			if (selectionBoxBounds.Contains(unitPosition))
			{
				addedUnits.Add(unit);
			}
		}
	}

	private void HandleMouseDown()
	{
		selectionBox.sizeDelta = Vector2.zero;
		startMousePosition = Mouse.current.position.ReadValue();
		selectionBox.gameObject.SetActive(true);
		addedUnits.Clear();
	}

	private void DeselectAllUnits()
	{
		ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
		foreach (ISelectable selectable in currentlySelectedUnits)
		{
			selectable.Deselect();
		}
	}

	private Bounds ResizeSelectionBox()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		float length = mousePosition.x - startMousePosition.x;
		float width = mousePosition.y - startMousePosition.y;
		selectionBox.anchoredPosition = startMousePosition + new Vector2(length / 2, width / 2);
		selectionBox.sizeDelta = new Vector2(Mathf.Abs(length), Mathf.Abs(width));
		return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
	}

	private void HandleRightClick()
	{
		if (selectedUnits.Count==0) return;
		Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

		if (Mouse.current.rightButton.wasReleasedThisFrame
			&& Physics.Raycast(cameraRay,out RaycastHit hit,float.MaxValue, floorLayers))
		{
			List<AbstractUnit> abstractUnits = new(selectedUnits.Count);
			foreach (ISelectable selectable in selectedUnits)
			{
					if (selectable is AbstractUnit unit)
					{
						abstractUnits.Add(unit);
					}
			}
			int layer = 0;
			int unitsOnLayer = 0;
			int maxUnitsOnLayer = 1;
			float circleRadius = 0;
			float radialOffset = 0;

			foreach (AbstractUnit unit in abstractUnits)
			{
				Vector3 targetPosition = new(
					hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
					hit.point.y,
					hit.point.z + circleRadius * Mathf.Sin(radialOffset * unitsOnLayer)
				);

				unit.MoveTo(targetPosition);
				unitsOnLayer++;

				if (unitsOnLayer >= maxUnitsOnLayer)
				{
					layer++;
					unitsOnLayer = 0;
					circleRadius += unit.AgentRadius * 3.5f;
					maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * 2));
					radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;
				}
			}

		}

	}

	private void HandleLeftClick()
	{
		if (camera == null) { return; }
		Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
		if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
				&& hit.collider.TryGetComponent(out ISelectable selectable))
		{
			selectable.Select();
		}

	}

	private void HandleRotating()
	{
		if (ShouldUseRotating())
		{
			rotateStartTime = Time.time;
		}
		float rotateTime= Mathf.Clamp01((Time.time - rotateStartTime) * rotateSpeed);
		Vector3 targetFollowOffset;
		if (Keyboard.current.pageDownKey.isPressed)
		{
			targetFollowOffset = new Vector3(
			maxRotationAmount,
			cinemachineFollow.FollowOffset.y,
			0);
		}
		else if (Keyboard.current.pageUpKey.isPressed)
		{
			targetFollowOffset = new Vector3(
			-maxRotationAmount,
			cinemachineFollow.FollowOffset.y,
			0);
		}
		else
		{
			targetFollowOffset = new Vector3(
				startingFollowOffset.x,
				cinemachineFollow.FollowOffset.y,
				startingFollowOffset.z
			);
		}

		cinemachineFollow.FollowOffset = Vector3.Slerp(
			cinemachineFollow.FollowOffset,
			targetFollowOffset,
			rotateTime
			);


	}
	private bool ShouldUseRotating()
	{
		return (
			Keyboard.current.pageUpKey.wasPressedThisFrame ||
			Keyboard.current.pageDownKey.wasReleasedThisFrame ||
			Keyboard.current.pageUpKey.wasPressedThisFrame ||
			Keyboard.current.pageDownKey.wasReleasedThisFrame
			);
	}
	private void HandleZooming()
	{
		if (ShouldUseZooming())
		{
			zoomStartTime = Time.time;
		}
		float zoomtime = Mathf.Clamp01((Time.time - zoomStartTime) * zoomSpeed);
		Vector3 targetFollowOffset;
		if (Keyboard.current.qKey.isPressed)
		{
			targetFollowOffset = new Vector3(
				cinemachineFollow.FollowOffset.x,
				minZoomDistance,
				cinemachineFollow.FollowOffset.z);
		}
		else {
			targetFollowOffset = new Vector3(
			    cinemachineFollow.FollowOffset.x,
				startingFollowOffset.y,
				cinemachineFollow.FollowOffset.z);
		}
			cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, zoomtime);
	}

	private bool ShouldUseZooming()
	{
		return (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.qKey.wasReleasedThisFrame);
	}
	private void HandlePanning()
	{
		Vector2 moveAmount = GetKeyboardMoveAmount();
		moveAmount += GetMouseMoveAmount();
		//moveAmount *= Time.deltaTime;
		//cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
		cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
	}

	private Vector2 GetMouseMoveAmount()
	{
		Vector2 moveAmount = Vector2.zero;
		if (!enableEdgePan) { return moveAmount; }
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		if (mousePosition.x<=edgePanSize)
		{
			moveAmount.x -= mousePanSpeed;
		}
		else if (mousePosition.x >= screenWidth - edgePanSize)
		{
			moveAmount.x += mousePanSpeed;
		}

		if (mousePosition.y >= screenHeight - edgePanSize)
		{
			moveAmount.y += mousePanSpeed;
		}
		else if (mousePosition.y <= edgePanSize)
		{
			moveAmount.y -= mousePanSpeed;
		}

		return moveAmount;
	}

	private Vector2 GetKeyboardMoveAmount()
	{
		Vector2 moveAmount = Vector2.zero;
		if (Keyboard.current.upArrowKey.isPressed)
		{
			moveAmount.y += moveSpeed;
		}
		if (Keyboard.current.downArrowKey.isPressed)
		{
			moveAmount.y -= moveSpeed;
		}
		if (Keyboard.current.leftArrowKey.isPressed)
		{
			moveAmount.x -= moveSpeed;
		}
		if (Keyboard.current.rightArrowKey.isPressed)
		{
			moveAmount.x += moveSpeed;
		}
		return moveAmount;
	
	}
}
