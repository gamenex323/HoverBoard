using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

	public static T Instance;

	public virtual void Awake() {
		if (Instance == null) {
			Instance = this as T;
		} else {
			Destroy (gameObject);
		}
	}

	protected virtual void OnDestroy() {
		if (Instance == this) Instance = null;
	}

}
