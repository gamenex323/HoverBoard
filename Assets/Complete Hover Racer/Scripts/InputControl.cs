using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent (typeof (PlayerShip))]
public class InputControl : MonoBehaviourPunCallbacks
{

	public PlayerShip pShip;
	public Cannon pCannon;
	public MissileLauncher pMissile;

	private const float keyAnalogTime = 0.33f;
	private IEnumerator SteerKeyCor, ThrustKeyCor, BrakeKeyCor;

	 private void Start()
    {
        // Disable controls if this is not the local player
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }
    }
	public void OnSteer (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return; // Only process input for local player
		pShip.newInputTurn = value.ReadValue<Vector2> ().x;
	}

	public void OnSteerKey (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return;
		if (value.started) {
			if (SteerKeyCor != null) StopCoroutine (SteerKeyCor);
			SteerKeyCor = SmoothSteerKey (value.ReadValue<Vector2> ().x);
			StartCoroutine (SteerKeyCor);
		} else if (value.canceled) {
			StopCoroutine (SteerKeyCor);
			SteerKeyCor = SmoothSteerKey (0);
			StartCoroutine (SteerKeyCor);
		}
	}
	private IEnumerator SmoothSteerKey (float inputValue) {
		float startTime = Time.time;
		float lastValue = pShip.newInputTurn;
		WaitForEndOfFrame loop = new WaitForEndOfFrame ();
		while (pShip.newInputTurn != inputValue) {
			float t = (Time.time - startTime) / keyAnalogTime;
			pShip.newInputTurn = Mathf.SmoothStep (lastValue, inputValue, t);
			yield return loop;
		}
	}



	public void OnThrust(InputAction.CallbackContext value)
	{
		if (photonView.IsMine)
			pShip.newInputThrust = value.ReadValue<float>();
	}

	public void OnThrustKey (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return;
		if (value.started) {
			if (ThrustKeyCor != null) StopCoroutine (ThrustKeyCor);
			ThrustKeyCor = SmoothThrustKey (1);
			StartCoroutine (ThrustKeyCor);
		} else if (value.canceled) {
			StopCoroutine (ThrustKeyCor);
			pShip.newInputThrust = 0;
		}
	}
	private IEnumerator SmoothThrustKey (float inputValue) {
		float startTime = Time.time;
		float lastValue = pShip.newInputThrust;
		WaitForEndOfFrame loop = new WaitForEndOfFrame ();
		while (pShip.newInputThrust != inputValue) {
			float t = (Time.time - startTime) / keyAnalogTime;
			pShip.newInputThrust = Mathf.SmoothStep (lastValue, inputValue, t);
			yield return loop;
		}
	}



	public void OnBrake(InputAction.CallbackContext value) 
	{
		if (photonView.IsMine)
			pShip.newInputBrake = value.ReadValue<float>(); 
	}

	public void OnBrakeKey (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return;
		if (value.started) {
			if (BrakeKeyCor != null) StopCoroutine (BrakeKeyCor);
			BrakeKeyCor = SmoothBrakeKey (1);
			StartCoroutine (BrakeKeyCor);
		} else if (value.canceled) {
			StopCoroutine (BrakeKeyCor);
			pShip.newInputBrake = 0;
		}
	}
	private IEnumerator SmoothBrakeKey (float inputValue) {
		float startTime = Time.time;
		float lastValue = pShip.newInputBrake;
		WaitForEndOfFrame loop = new WaitForEndOfFrame ();
		while (pShip.newInputBrake != inputValue) {
			float t = (Time.time - startTime) / keyAnalogTime;
			pShip.newInputBrake = Mathf.SmoothStep (lastValue, inputValue, t);
			yield return loop;
		}
	}




	public void OnFire (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return;
		if (value.started) {
			pCannon.newInputFire = true;
			pMissile.newInputFire = true;
		} else if (value.performed) {
			pCannon.newInputFire = true;
		} else if (value.canceled) {
			pCannon.newInputFire = false;
			pMissile.newInputFire = false;
		};
	}


	public void OnCameraSwitch (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return;
		if (value.started) RaceManager.Instance.raceCam.CameraSwitch ();
	}



	public void OnPause (InputAction.CallbackContext value) {
		if (!photonView.IsMine) return;
		if (value.started) RaceManager.Instance.OnPause ();
	}

}
