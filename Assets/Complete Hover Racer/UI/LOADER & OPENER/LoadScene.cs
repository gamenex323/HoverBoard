using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : Singleton<LoadScene> {

	public GameObject LoaderBack;
	private AsyncOperation async;

	public void LoadByName (string missionName) {
		LoaderBack.SetActive (true);
		async = SceneManager.LoadSceneAsync (missionName);
	}

	public void ReloadScene () {
		LoaderBack.SetActive (true);
		async = SceneManager.LoadSceneAsync (SceneManager.GetActiveScene ().name);
	}

}
