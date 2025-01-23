using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using EditorButton;
#endif

public class GameManager : SingletonDontDestroy<GameManager> {
	// STATE
	[HideInInspector] public State GameState = State.OVER;

	//	[Header ("Build type info (Auto detected)")]
	[HideInInspector] public Build BuildType = Build.DESKTOP;

	// BUILD & CONTROLS
	[Header ("Global Racing Parameters")]
	[Range (1f, 2f)] public float raceSpeed = 1f;
	[Range (0.01f, 0.05f)] public float brakePower = 0.03f;
	[Range (300f, 600f)] public float playerTurn = 400f;
	public bool useIndividualTurn;

	[Header ("Global Hover Controls")]
	public HoverType HoverMode = HoverType.PID_CONTROLLED_ADDFORCE;
	public float hoverHeight = 5f;
	[Range (0.05f, 0.9f)] public float hoverFactor = 0.1f;
	[Range (0.05f, 0.9f)] public float bendFactor = 0.2f;
	public float fallGravity = 200f;

	[Header ("PID Hover Parameters:")]
	public float PIDHoverForce = 300f;
	public float hoverGravity = 20f;

	[Header ("Allways ON for mobile build")]
	public bool useAutoThrust;

	// GAME CURRENCY & INIT QUANTITY
	[Header ("MONEY AT BEGINNING")]
	[SerializeField] private int _money = 0;
	public int Money => _money;

	[Header ("PLAYER OBJECTS")]
	public List<PlayerObject> players = new List<PlayerObject> ();

	[Header ("LEVEL OBJECTS")]
	public Level[] levels;
	[HideInInspector] public Level selectedLevel;


	/*	private void OnValidate () {
	#if UNITY_ANDROID || UNITY_IOS
			BuildType = Build.MOBILE;
			useAutoThrust = true;
	#else
			BuildType = Build.DESKTOP;
	#endif
		}*/

	void Start () {
#if UNITY_ANDROID || UNITY_IOS
		BuildType = Build.MOBILE;
		useAutoThrust = true;
#endif

		LoadGoods ();
		LoadLevels ();
	}


	public void LoadPlayerData () {
		foreach (PlayerObject ply in players) {
			ply.LoadData ();
		}
	}

	// Return a sum of collected stars in all levels also unlock levels
	public int GetAllStars () {
		int allStars = 0;
		// Count collected stars
		foreach (var levelItem in levels) allStars += levelItem.StarsCollected;
		// Unlock levels when we collected stars needed to unlock
		foreach (var levelItem in levels) if (!levelItem.unlocked && allStars >= levelItem.starsToUnlock) levelItem.UnlockLevel ();
		// Returning Value
		return allStars;
	}

	// PLAYER LIST & RE-ORDER BY UNLOCK STATE FUNCTIONS
	public int GetPlayerIndex (string selected) {
		for (int i = 0; i < players.Count; ++i) {
			if (players[i].GetInstanceID ().ToString () == selected) return i;
		}
		return 0;
	}

	public PlayerObject GetSelectedPlayer () {
		string selected = PlayerPrefs.GetString ("selected", "");
		if (selected != "")
			return players[GetPlayerIndex (selected)];		
		else
			return players[0];
	}

	public void SortPlayers () => players.Sort (UnlockedSort);

	private int UnlockedSort (PlayerObject item_1, PlayerObject item_2) {
		if (item_1 && item_2) {
			if (!item_1.unlocked && item_2.unlocked) {
				return 1;
			}
			if (item_1.unlocked && !item_2.unlocked) {
				return -1;
			} else {
				return 0;
			}
		} else {
			return 0;
		}
	}


	#region CURRENCY & ITEM QUANTITY HANDLING

	public void SetMoney (int quantity) {
		_money = quantity;
		SaveGoods ();
	}

	public void AddMoney (int quantity) {
		_money += quantity;
		SaveGoods ();
	}

	public void RemoveMoney (int quantity) {
		_money -= quantity;
		SaveGoods ();
	}

	#endregion


	#region SAVING AND LOADING
	
	private void SaveGoods () => PlayerPrefs.SetInt ("credit", _money);
	private void LoadGoods () => _money = PlayerPrefs.GetInt ("credit", _money);

	private void LoadLevels () {
		foreach (var levelItem in levels) levelItem.LoadData ();
	}

	#endregion


#if UNITY_EDITOR

	[ButtonAttribute ("DELETE MANAGER SAVEDATA", ButtonMode.EditorMode)]
	public void DeleteManagerPrefs () {
		PlayerPrefs.DeleteKey ("credit");
		PlayerPrefs.DeleteKey ("selected");
	}


	[ButtonAttribute ("DELETE LEVELS SAVEDATA", ButtonMode.EditorMode)]
	public void DeleteLevelsPrefs () {
		foreach (var levelItem in levels) levelItem.DeletePrefs ();
		levels[0].UnlockLevel ();
	}


	[ButtonAttribute ("DELETE PLAYERS SAVEDATA", ButtonMode.EditorMode)]
	public void DeletePlayersPrefs () {
		foreach (var playerItem in players) playerItem.DeletePrefs ();
	}


	[ButtonAttribute ("REFRESH MUSIC PLAYLIST", ButtonMode.EditorMode)]
	public void RefreshPlayList () {
		string[] playList = CollectMusicNames ();
		if (playList.Length == 0) return;
		foreach (var levelItem in levels) levelItem.SetPlayList (playList);
	}

	private string[] CollectMusicNames () {
		AudioClip[] clipFiles = Resources.LoadAll<AudioClip> ("music");
		string[] clipNames = new string[clipFiles.Length];
		for (int i = 0; i < clipFiles.Length; i++) clipNames[i] = clipFiles[i].name;
		return clipNames;
	}

#endif

}
