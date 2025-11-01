using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
	[SerializeField] private Transform cameraTarget;
	[SerializeField] private CinemachineCamera cinemachineCamera;
	[SerializeField] private float moveSpeed = 5;
	[SerializeField] private float zoomSpeed = 1;
	[SerializeField] private float rotateSpeed = 1;
	[SerializeField] private float minZoomDistance = 7.5f;

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
	}
	private void Update()
	{
		HandleRotating();
		HandlePanning();
		HandleZooming();
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
		moveAmount *= Time.deltaTime;
		cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
	}
}
