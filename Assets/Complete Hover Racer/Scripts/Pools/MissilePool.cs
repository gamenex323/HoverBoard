using System.Collections.Generic;
using UnityEngine;

public class MissilePool : MonoBehaviour {

	public static MissilePool Instance;
	[SerializeField] private GameObject prefab;
	private Queue<GameObject> objectAvailable = new Queue<GameObject> ();


	private void Awake () {
		Instance = this;
	}

	public GameObject Get () {
		if (objectAvailable.Count == 0) {
			GameObject newObject = Instantiate (prefab);
			newObject.SetActive (true);
			return newObject;
		}

		GameObject poolObject = objectAvailable.Dequeue ();
		poolObject.SetActive (true);
		return poolObject;
	}

	public void Return (GameObject poolObject) {
		poolObject.SetActive (false);
		objectAvailable.Enqueue (poolObject);
	}
}
