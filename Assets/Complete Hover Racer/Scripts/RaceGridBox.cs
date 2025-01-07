using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class RaceGridBox : MonoBehaviour {

	public GameObject GridBoxPrefab;
	[Range (0.1f, 0.3f)] public float snapHeight = 0.15f;

	private LayerMask roadLayers;
	private Color gizmoColor;
	private Vector3 lastPos;
	private Quaternion lastRot;


	private void OnValidate () {
		if (!gameObject.activeInHierarchy) return;
		roadLayers = FindObjectOfType<RaceManager> ().roadLayer;
	}


#if UNITY_EDITOR

	private void OnDrawGizmos () {
		if (transform.position != lastPos || transform.rotation != lastRot) {

			RaycastHit hit;
			if (Physics.Raycast (transform.position, -transform.up, out hit, 256f, roadLayers)) {
				transform.position = hit.point + (transform.up * snapHeight);
				transform.rotation = Quaternion.FromToRotation (transform.up, hit.normal) * transform.rotation;
				// Delete existing children
				var quadList = transform.Cast<Transform> ().ToList ();
				foreach (var child in quadList) DestroyImmediate (child.gameObject);
				// Instantiate new quad prefab
				GameObject clone = PrefabUtility.InstantiatePrefab (GridBoxPrefab, transform) as GameObject;
				clone.transform.position = transform.position;
				clone.transform.rotation = Quaternion.FromToRotation (-transform.forward, hit.normal) * transform.rotation;
				// Change gizmo color
				gizmoColor = new Color (0.2f, 1f, 0f, 0.64f);
			} else {
				// Delete existing children
				var quadList = transform.Cast<Transform> ().ToList ();
				foreach (var child in quadList) DestroyImmediate (child.gameObject);
				// Change gizmo color
				gizmoColor = new Color (1f, 0.2f, 0f, 0.64f);
			}

			lastPos = transform.position;
			lastRot = transform.rotation;
		}

		Gizmos.color = gizmoColor;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube (transform.InverseTransformPoint(transform.position + (transform.up * 5)), new Vector3 (5f, 3f, 8f));
	}

#endif

}
