using UnityEngine;

public class SideWallCollider : MonoBehaviour {

	[Range (0.9f, 0.96f)] public float reduceSpeedOnHit = 0.96f;

	void OnCollisionStay (Collision collisionInfo) {
		if (GameManager.Instance.GameState != State.PLAY) return;
		collisionInfo.collider.GetComponentInParent<IShip> ()?.WallHit (collisionInfo.GetContact (0).normal, reduceSpeedOnHit);
	}

}
