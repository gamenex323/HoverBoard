using UnityEngine;
using UnityEditor;

public class CreateWaypointsTool : EditorWindow {
	[SerializeField] private GameObject trackPiece;

	[MenuItem ("Tools/CHR/Create Waypoint Circuit")]
	static void WaypointsTool () {
		EditorWindow.GetWindow<CreateWaypointsTool> ();
	}

	private void OnGUI () {
		EditorGUILayout.Space ();

		trackPiece = (GameObject)EditorGUILayout.ObjectField ("ADD START PIECE", trackPiece, typeof (GameObject), true);

		EditorGUILayout.Space ();

		if (GUILayout.Button ("GENERATE", GUILayout.Height (30))) {
			//GameObject selection = Selection.activeGameObject;
			GenerateWaypoints generator = trackPiece.transform.parent.gameObject.AddComponent<GenerateWaypoints> ();
			generator.startPiece = trackPiece.transform;
			generator.CreateWay (false);
			DestroyImmediate (generator);
		}

		EditorGUILayout.Space ();

		if (GUILayout.Button ("REVERSE & GENERATE", GUILayout.Height (30))) {
			//GameObject selection = Selection.activeGameObject;
			GenerateWaypoints generator = trackPiece.transform.parent.gameObject.AddComponent<GenerateWaypoints> ();
			generator.startPiece = trackPiece.transform;
			generator.CreateWay (true);
			DestroyImmediate (generator);
		}
		
	}
}