using UnityEngine;

public interface IShip {
	void Turbo_On ();
	void Turbo_Off ();
	void WallHit (Vector3 hitPosition, float reduceThrust);
	void CannonHit ();
	void MissileHit ();
}
