using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class ActivationEvents : MonoBehaviour {

	public GameObject makeSelectedOnEnable;
	[Space(10)]
	public UnityEvent OnEnabled, OnDisabled;


	private void OnEnable () {
		OnEnabled.Invoke ();
		if (makeSelectedOnEnable != null) StartCoroutine (Select (makeSelectedOnEnable));
	}

	private void OnDisable () => OnDisabled.Invoke ();

	private IEnumerator Select (GameObject selectedGameobject) {
		yield return new WaitForEndOfFrame ();
		EventSystem.current.SetSelectedGameObject (null);
		EventSystem.current.SetSelectedGameObject (selectedGameobject);
	}

}
