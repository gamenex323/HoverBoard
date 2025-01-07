using UnityEngine;

public class FinishLine : BasePad {

	void OnTriggerEnter (Collider other) {
		// Check if a collider is a ship via layer-index and check gamestate
		if (GameManager.Instance.GameState != State.PLAY) return;
		// If ship has a cannon or missile launcher, allow the firing - ship cant fire until first time pass the startline
		other.GetComponentInParent<ICannon> ()?.AllowFire ();
		other.GetComponentInParent<IMissileLauncher> ()?.AllowFire ();

		if (other.CompareTag ("Player"))
			RaceManager.Instance.LapCount ();	// Count laps for player
		else
			other.GetComponentInParent<WaypointProgressTracker> ()?.StartTrack ();  // Start AI waypoint tracking
	}

}
