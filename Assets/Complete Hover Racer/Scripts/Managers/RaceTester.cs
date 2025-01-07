using UnityEngine;

public class RaceTester : MonoBehaviour {

	public Level _level;
	public GameManager manager;

	private void OnValidate () {
		manager.selectedLevel = _level;
	}

}
