using UnityEngine;

public class TurboPad : BasePad {

	void OnTriggerEnter (Collider other) {
		if (GameManager.Instance.GameState != State.PLAY) return;
		other.GetComponentInParent<IShip> ()?.Turbo_On ();
	}

}
