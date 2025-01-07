using UnityEngine;

public class BoothScene : MonoBehaviour {

	public Object loadScene;
	[HideInInspector] public string sceneNameForLoad;
	private AsyncOperation async;


#if UNITY_EDITOR
	private void OnValidate () {
		if (loadScene != null) sceneNameForLoad = loadScene.name;
	}
#endif


	void Start () {
		// Initial Setup
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Time.fixedDeltaTime = 1f / 60f;

		// Start music to be played while loading menu
		AudioManager.Instance.PlayMusic ("menu");

		// Load Menu scene
		LoadScene.Instance.LoadByName (sceneNameForLoad);
	}

}
