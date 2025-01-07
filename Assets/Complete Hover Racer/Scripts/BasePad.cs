using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
using EditorButton;
#endif


[RequireComponent (typeof (BoxCollider))]
public class BasePad : MonoBehaviour {

	public GameObject quadPrefab;

	[Space(10)]
	public bool autoSnap;
	public bool getRoadFromRacemanager = true;
	public LayerMask roadLayers;
	public float offSet = 6f;
	[Range (0.05f, 0.3f)] public float snapHeight = 0.15f;

	private BoxCollider col;


	private void OnValidate () {
		if (!gameObject.activeInHierarchy) return;

		if (getRoadFromRacemanager) {
			RaceManager rm = FindObjectOfType<RaceManager> ();
			if (rm != null)
				roadLayers = rm.roadLayer;
			else Debug.LogError ("Scene don't contain RaceManager");
		}

		col = GetComponent<BoxCollider> ();
		col.isTrigger = true;
	}



#if UNITY_EDITOR

	private Color gizmoColor;
	private float lastSnap;
	private Vector3 lastPos;
	private Quaternion lastRot;


	[ButtonAttribute ("SNAP TO ROAD", ButtonMode.EditorMode)]
	public void SnapToRoad () {
		RaycastHit hit;
		Vector3 origo = transform.position + transform.up;
		if (Physics.Raycast (transform.position, -transform.up, out hit, 300, roadLayers)) {
			transform.SetPositionAndRotation (hit.point, Quaternion.FromToRotation (transform.up, hit.normal) * transform.rotation);
			transform.position += transform.up * (offSet + snapHeight);
		}
	}


	private void OnDrawGizmos () {
		if (autoSnap && (snapHeight != lastSnap || transform.position != lastPos || transform.rotation != lastRot)) {

			if (Physics.Raycast (transform.position, -transform.up, out RaycastHit hit, 256f, roadLayers)) {
				transform.position = hit.point;
				transform.rotation = Quaternion.FromToRotation (transform.up, hit.normal) * transform.rotation;
				transform.position += transform.up * offSet;
				// Delete existing children
				var quadList = transform.Cast<Transform> ().ToList ();
				foreach (var child in quadList) DestroyImmediate (child.gameObject);
				// Instantiate new quad prefab
				GameObject clone = PrefabUtility.InstantiatePrefab (quadPrefab, transform) as GameObject;
				clone.transform.position = hit.point + (transform.up * snapHeight);
				clone.transform.rotation = Quaternion.FromToRotation (-transform.forward, hit.normal) * transform.rotation;
				if (col == null) col = GetComponent<BoxCollider> ();
				clone.transform.localScale = new Vector3 (col.size.x, col.size.z, 1f);
				// Change gizmo color
				gizmoColor = new Color (0.5f, 0f, 0.5f, 1f);
			} else {
				// Delete existing children
				var quadList = transform.Cast<Transform> ().ToList ();
				foreach (var child in quadList) DestroyImmediate (child.gameObject);
				// Change gizmo color
				gizmoColor = new Color (1f, 0.2f, 0f, 1f);
			}

			lastSnap = snapHeight;
			lastPos = transform.position;
			lastRot = transform.rotation;
		}

		Gizmos.color = gizmoColor;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube (Vector3.zero + col.center, new Vector3 (col.size.x, col.size.y, col.size.z));
	}

#endif

}
