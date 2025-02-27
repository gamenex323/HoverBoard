﻿using UnityEngine;

public class AmmoPad : BasePad {

	public int Ammo = 100;

	void OnTriggerEnter (Collider other) {
		if (GameManager.Instance.GameState != State.PLAY) return;
		Debug.Log("Player: " , other.gameObject);
		other.GetComponentInParent<ICannon> ()?.Reload (Ammo);
	}

}
