using UnityEngine;
using System.Collections;

public class WaypointProgressTracker : MonoBehaviour {

	public WaypointCircuit circuit;
	public Transform target;
	[Range (5f, 15f)] public float lookAheadForTargetOffset = 10f;
	[Range (0.1f, 0.3f)] public float lookAheadForTargetFactor = 0.2f;

	public WaypointCircuit.RoutePoint targetPoint { get; private set; }
	public WaypointCircuit.RoutePoint progressPoint { get; private set; }

	private float progressDistance; // The progress round the route, used in smooth mode.
	private Vector3 lastPosition; // Used to calculate current speed (since we may not have a rigidbody component)
	private float speed; // current speed of this object (calculated from delta since last frame)
	private bool inRace;


	private void Update () {
		if (!inRace) return;

		if (Time.deltaTime > 0)
			speed = Mathf.Lerp (speed, (lastPosition - transform.position).magnitude / Time.deltaTime, Time.deltaTime);
		lastPosition = transform.position;

		progressPoint = circuit.GetRoutePoint (progressDistance);
		Vector3 progressDelta = progressPoint.position - transform.position;
		if (Vector3.Dot (progressDelta, progressPoint.direction) < 0) {
			progressDistance += progressDelta.magnitude * 0.5f;
		}		
	}

	public void StartTrack () {
		if (!inRace) {
			inRace = true;
			StartCoroutine (SmoothTrack ());
		}
	}

	IEnumerator SmoothTrack () {
		WaitForEndOfFrame loop = new WaitForEndOfFrame ();
		float factor = 0;

		while (factor < 1f) {
			factor += 0.005f;
			target.position = Vector3.Lerp (target.position,
				circuit.GetRoutePoint (progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed).position,
				factor);
			yield return loop;
		}

		while (true) {
			target.position = circuit.GetRoutePoint (progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed).position;
			yield return loop;
		}
	}


	private void OnDrawGizmos () {
		if (circuit != null && Application.isPlaying) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, target.position);
			Gizmos.DrawWireSphere (circuit.GetRoutePosition (progressDistance), 1);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (target.position, target.position + target.forward);
		}
	}

}
