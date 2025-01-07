using UnityEngine;
using UnityEditor;
using System.Linq;

public class PrepareColliderGroup : EditorWindow {

	[SerializeField] private GameObject RoadCollidersParent;
	[SerializeField] private GameObject WallCollidersParent;
	[SerializeField] private PhysicMaterial ColliderPhysicMaterial;

	[MenuItem ("Tools/CHR/Set Track Colliders")]
	static void RoadColliderTool () {
		EditorWindow.GetWindow<PrepareColliderGroup> ();
	}


	private void OnGUI () {
		EditorGUILayout.Space ();
		RoadCollidersParent = (GameObject)EditorGUILayout.ObjectField ("ROAD COLLIDERS PARENT", RoadCollidersParent, typeof (GameObject), true);
		WallCollidersParent = (GameObject)EditorGUILayout.ObjectField ("WALL COLLIDERS PARENT", WallCollidersParent, typeof (GameObject), true);
		EditorGUILayout.Space ();
		ColliderPhysicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField ("PHYSIC MATERIAL", ColliderPhysicMaterial, typeof (PhysicMaterial), false);
		EditorGUILayout.Space ();

		if (GUILayout.Button ("SET COLLIDERS", GUILayout.Height (30))) {
			if (PrepareRoadColliders (RoadCollidersParent, ColliderPhysicMaterial) 
				&& PrepareWallColliders (WallCollidersParent, ColliderPhysicMaterial))
				this.Close ();
		}
	}
	

	static bool PrepareRoadColliders (GameObject selection, PhysicMaterial phMat) {
		if (selection == null || phMat == null) return false;

		var colliderParts = selection.transform.Cast<Transform> ().ToList ();
		foreach (var child in colliderParts) {
			DestroyImmediate (child.GetComponent<MeshRenderer> ());
			DestroyImmediate (child.GetComponent<Mesh> ());
			child.GetComponent<Collider> ().material = phMat;
			child.gameObject.isStatic = true;
		}

		Undo.RegisterFullObjectHierarchyUndo (selection, "Preparing Road Colliders");

		return true;
	}

	static bool PrepareWallColliders (GameObject selection, PhysicMaterial phMat) {
		if (selection == null || phMat == null) return false;

		Rigidbody rb = selection.AddComponent<Rigidbody> ();
		rb.isKinematic = true;
		selection.AddComponent<SideWallCollider> ();
		selection.layer = 1;

		var colliderParts = selection.transform.Cast<Transform> ().ToList ();
		foreach (var child in colliderParts) {
			DestroyImmediate (child.GetComponent<MeshRenderer> ());
			DestroyImmediate (child.GetComponent<Mesh> ());
			child.GetComponent<Collider> ().material = phMat;
			child.gameObject.isStatic = true;
			child.gameObject.layer = 1;
		}

		Undo.RegisterFullObjectHierarchyUndo (selection, "Preparing Wall Colliders");

		return true;
	}

}
