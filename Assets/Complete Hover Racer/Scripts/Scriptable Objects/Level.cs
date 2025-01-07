using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CreateAssetMenu (menuName = "Level")]
public class Level : ScriptableObject {

	public Object scene;
	[HideInInspector] public string sceneNameForLoad;

	[HideInInspector] public string playMusic;
	[HideInInspector] public int selectedMusic;
	[HideInInspector] public string[] clipNames;

	public bool unlocked = true;
	public int starsToUnlock;

	public int StarsCollected {
		get { return PlayerPrefs.GetInt (GetInstanceID ().ToString () + "stars", 0); }
		set { PlayerPrefs.SetInt (GetInstanceID ().ToString () + "stars", value); }
	}

	public int LapRecord {
		get { return PlayerPrefs.GetInt (GetInstanceID ().ToString () + "lapRecord", 0); }
		set { PlayerPrefs.SetInt (GetInstanceID ().ToString () + "lapRecord", value); }
	}

	[Space (10)]
	public int laps = 3;

	[Space (10)]
	public bool sideWallRebound = true;

	[Space (5)]
	[Header ("OVERRIDE GLOBAL SETUP")]
	public bool useLocalSetup;
	[Range (1f, 2f)] public float raceSpeed = 1f;
	public HoverType HoverMode = HoverType.PID_CONTROLLED_ADDFORCE;
	public float hoverHeight = 5f;
	[Range (0.05f, 0.9f)] public float hoverFactor = 0.1f;
	[Range (0.05f, 0.9f)] public float bendFactor = 0.2f;
	public float fallGravity = 200f;
	public float PIDHoverForce = 300f;
	public float hoverGravity = 20f;

	[Space (5)]
	[Header ("LEVEL SELECT UI")]
	public string displayName;
	public Sprite displayImage;

	[Space (5)]
	[Header ("REWARDS")]
	[SerializeField] private int positionsReward = 100;
	public int Reward => positionsReward;
	[SerializeField] private int lapRecordReward = 100;
	public int LapReward => lapRecordReward;

	[Header ("AI CHEAT ON START")]
	[Range (0f, 0.5f)] public float cheatSpeed = 0;
	[Range (0, 50)] public int cheatDuration = 0;

	[Header ("AI PREFABS")]
	public List<GameObject> AIs = new List<GameObject> ();



#if UNITY_EDITOR
	private void OnValidate () {
		if (scene != null) sceneNameForLoad = scene.name;
	}


	public void SetPlayList (string[] playList) {
		clipNames = playList;
		if (selectedMusic >= clipNames.Length) {
			selectedMusic = clipNames.Length - 1;
			playMusic = clipNames[selectedMusic];
		}

		EditorUtility.SetDirty (this);
		AssetDatabase.SaveAssets ();
	}

	public void ChangeMusic (int selected) {
		selectedMusic = selected;
		playMusic = clipNames[selected];

		EditorUtility.SetDirty (this);
		AssetDatabase.SaveAssets ();
	}
#endif



	public void UnlockLevel () {
		unlocked = true;
		PlayerPrefs.SetInt (GetInstanceID ().ToString () + "unlocked", 1);
	}

	public void LoadData () {
		if (PlayerPrefs.GetInt (GetInstanceID ().ToString () + "unlocked", 0) == 1) unlocked = true;
	}

	public void DeletePrefs () {
		PlayerPrefs.DeleteKey (GetInstanceID ().ToString () + "unlocked");
		PlayerPrefs.DeleteKey (GetInstanceID ().ToString () + "stars");
		PlayerPrefs.DeleteKey (GetInstanceID ().ToString () + "lapRecord");
	}

}




#if UNITY_EDITOR
[CustomEditor (typeof (Level))]
public class LevelEditor : Editor {

	int selected;

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		Level myScript = (Level)target;

		if (myScript.clipNames == null) return;

		GUILayout.Space (10);
		// Show popup for select
		int oldSelected = selected;
		selected = EditorGUILayout.Popup ("PLAY MUSIC: ", myScript.selectedMusic, myScript.clipNames);
		// Update the selected choice in the underlying object
		if (oldSelected != selected) myScript.ChangeMusic (selected);
	}

}
#endif
