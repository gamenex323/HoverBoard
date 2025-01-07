using UnityEngine;
using UnityEditor;
using System.Linq;

public class GenerateWaypoints : MonoBehaviour {

	public Transform startPiece;
	private Vector3 pieceCenter;


	private void OnValidate () {
		if (startPiece == null)
			pieceCenter = Vector3.zero;
		else
			pieceCenter = startPiece.GetComponent<Renderer> ().bounds.center;
	}


#if UNITY_EDITOR
	public void CreateWay (bool reversed) {
		// Reorder Childs To make StartPiece First
		var trackParts = transform.Cast<Transform> ().ToList ();
		int s = startPiece.GetSiblingIndex ();
		for (int i = 0; i < s; i++) trackParts[i].SetAsLastSibling ();

		// Reverse if needed
		if (reversed) {
			trackParts.Reverse ();
			foreach (var child in trackParts) child.SetAsLastSibling ();
		}

		// Create WayPointCircuit as waypoints container
		GameObject WayPoints = new GameObject ();
		WayPoints.name = "WAYPOINTS";
		WayPoints.AddComponent<WaypointCircuit> ();
		WayPoints.transform.position = Vector3.zero;

		// Generate and positionate waypoints
		trackParts.Clear ();
		trackParts = transform.Cast<Transform> ().ToList ();
		foreach (var child in trackParts) {
			Transform wp = new GameObject ().transform;
			wp.position = child.GetComponent<Renderer> ().bounds.center;
			wp.parent = WayPoints.transform;
			Undo.RegisterCreatedObjectUndo (wp.gameObject, "WayPoint Added");
		}

		// Register newly created gameobject
		Undo.RegisterCreatedObjectUndo (WayPoints, "Created WayPoint System");

		WayPoints.GetComponent<WaypointCircuit> ().AssignWayPoints ();
	}
#endif


	private void OnDrawGizmosSelected () {
		if (pieceCenter == Vector3.zero) return;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (pieceCenter, 20);
	}

}


#if UNITY_EDITOR
[CustomEditor (typeof (GenerateWaypoints))]
public class GenerateWaypointsEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		GenerateWaypoints myScript = (GenerateWaypoints)target;

		GUILayout.Space (7);
		if (GUILayout.Button ("GENERATE WAYPOINTS", GUILayout.Height (30))) myScript.CreateWay (false);
		GUILayout.Space (5);
		if (GUILayout.Button ("GENERATE REVERSED WAYPOINTS", GUILayout.Height (30))) myScript.CreateWay (true);
		GUILayout.Space (5);
	}

}
#endif