using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInput : MonoBehaviour
{
	[SerializeField] private Rigidbody cameraTarget;
	[SerializeField] private CinemachineCamera cinemachineCamera;
	[SerializeField] private new Camera camera;
	[SerializeField] private LayerMask selectableUnitLayers;
	[SerializeField] private LayerMask floorLayers;
	[SerializeField] private RectTransform selectionBox;

	[SerializeField] private float moveSpeed = 5;
	[SerializeField] private float zoomSpeed = 1;
	[SerializeField] private float rotateSpeed = 1;
	[SerializeField] private float minZoomDistance = 7.5f;
	[SerializeField] private float mousePanSpeed = 5;
	[SerializeField] private float edgePanSize = 50;

	private Vector2 startMousePosition;
	private ISelectable selectedUnit;
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
	}
	
	private void Update()
	{
		HandleRotating();
		HandlePanning();
		HandleZooming();
		HandleLeftClick();
		HandleRightClick();
		HandleDragSelect();
	}

	private void OnDestroy()
	{
		Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
		Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
	}

	private void HandleUnitDeselected(UnitDeselectedEvent evt)
	{
		selectedUnit = null;
	}

	private void HandleUnitSelected(UnitSelectedEvent evt)
	{
		selectedUnit = evt.Unit;
	}

	private void HandleDragSelect()
	{
		if (selectionBox==null) return;
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			startMousePosition = Mouse.current.position.ReadValue();
			selectionBox.gameObject.SetActive(true);
		
		}

		else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();
			float length = mousePosition.x - startMousePosition.x;
			float width = mousePosition.y - startMousePosition.y;
			selectionBox.anchoredPosition = startMousePosition+ new Vector2(length/2,width/2);
			selectionBox.sizeDelta = new Vector2(Mathf.Abs(length), Mathf.Abs(width));
		}

		else if (Mouse.current.leftButton.wasReleasedThisFrame && !Mouse.current.leftButton.wasPressedThisFrame)
		{
			selectionBox.gameObject.SetActive(false);
			selectionBox.sizeDelta = Vector2.zero;
		}

	}

	private void HandleRightClick()
	{
		if (selectedUnit == null||selectedUnit is not IMoveable moveable) return;
		Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

		if (Mouse.current.rightButton.wasReleasedThisFrame
			&& Physics.Raycast(cameraRay,out RaycastHit hit,float.MaxValue, floorLayers))
		{
			moveable.MoveTo(hit.point);
		}

	}

	private void HandleLeftClick()
	{
		if (camera == null) return;
		Ray cameraRay=camera.ScreenPointToRay(Mouse.current.position.ReadValue());

		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			if (Physics.Raycast(cameraRay,out RaycastHit hit,float.MaxValue, selectableUnitLayers)
				&&hit.collider.TryGetComponent(out ISelectable selectable))
			{
				if (selectedUnit!=null)
				{
					selectedUnit.Deselect();
				}
				selectable.Select();
			}
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
