using UnityEngine;

public class DistanceTracker : MonoBehaviour  {

	public int racePosition;    // POSITIONFIX
	public WaypointCircuit circuit;
	private WaypointCircuit.RoutePoint progressPoint;
	[HideInInspector] public float progressDistance;
	[HideInInspector] public Vector3 progressDelta;
	[HideInInspector] public Vector3 tWay;


	private void Update() {
		if (circuit == null)
			return;
		progressPoint = circuit.GetRoutePoint(progressDistance);
		progressDelta = progressPoint.position - transform.position;
		if (Vector3.Dot(progressDelta, progressPoint.direction) < 0) {
			progressDistance += progressDelta.magnitude * 0.5f;
		} else {
			progressDistance -= progressDelta.magnitude * 0.5f;
		}

		tWay = circuit.GetRoutePoint (progressDistance).direction;
	}

}