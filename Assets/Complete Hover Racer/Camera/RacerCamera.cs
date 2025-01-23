using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using Photon.Pun;

public class RacerCamera : MonoBehaviourPun {

	[Range (0, 1f)] public float CameraTilt = 0.4f;
	[Space(5)]
	public int normalFOV = 60;
	public int turboFOV = 66;

	[Space (5)]
	public Transform _mainCam;
	public List<Transform> cameraPositions = new List<Transform> ();
	private int camIndex;

	private float camFOV;
	private Camera cam;
	private Transform player;
	private Transform playerBody;
	private Quaternion tiltRotation;
	private const float cameraSmoothTime = 0.02f;
	private Vector3 cameraVelocity = new Vector3 (0f, 0f, 0f);
	private IEnumerator followLoop;


	private void Awake () {
		cam = GetComponentInChildren<Camera> ();
		camFOV = normalFOV;
	}

	public void SetNormalFOV () => camFOV = normalFOV;
	public void SetTurboFOV () => camFOV = turboFOV;


	public void InitCamera (Transform playerTr, Transform playerBodyTr) {
		player = playerTr;
		playerBody = playerBodyTr;
		if (photonView.IsMine)
		{
			if (followLoop == null)
			{
				followLoop = StartFollow();
				StartCoroutine(followLoop);
			}
			cam.gameObject.SetActive(true);
		}
		else
        {
			cam.gameObject.SetActive(false);
        }
	}



	public void CameraSwitch () {
		camIndex = (camIndex + 1) % cameraPositions.Count;
		_mainCam.SetLocalPositionAndRotation (
		cameraPositions[camIndex].localPosition,
		cameraPositions[camIndex].localRotation);

	}



	IEnumerator StartFollow () {
		WaitForFixedUpdate loop = new WaitForFixedUpdate ();
		while (true) {
			transform.position = Vector3.SmoothDamp (transform.position, playerBody.position, ref cameraVelocity, cameraSmoothTime);
			tiltRotation = Quaternion.Slerp (player.rotation, playerBody.rotation, CameraTilt);
			transform.rotation = Quaternion.Slerp (transform.rotation, tiltRotation, 0.12f);

			cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, camFOV, 0.03f);

			yield return loop;
		}
	}

}
