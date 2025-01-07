using UnityEngine;

public class MissilePad : BasePad {

	void OnTriggerEnter (Collider other) {
		if (GameManager.Instance.GameState != State.PLAY) return;
		other.GetComponentInParent<IMissileLauncher> ()?.Reload ();
	}

}
