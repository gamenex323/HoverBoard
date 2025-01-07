using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RaceManager : Singleton<RaceManager> {

	public LayerMask roadLayer;

	public float startDelay = 1f;
	public GameObject cameraPrefab;
	[HideInInspector] public RacerCamera raceCam;
	[Header ("Mobile Controls")]
	public GameObject joyPad;
	private GameObject mobileControls;

	[Header ("HUD ELEMENTS")]
	[SerializeField] public GameObject hudPanel;
	[SerializeField] private TextMeshProUGUI countdownText;
	[SerializeField] private TextMeshProUGUI lapTimeText;
	[SerializeField] private GameObject lapRecordLabel;
	[SerializeField] private TextMeshProUGUI lapRecordText;
	[SerializeField] private TextMeshProUGUI positionText;
	[SerializeField] private TextMeshProUGUI lapText;
	public TextMeshProUGUI ammoType;
	public TextMeshProUGUI ammoText;
	[SerializeField] private TextMeshProUGUI infoText;
	[SerializeField] public GameObject pausePanel;
	[SerializeField] public Button resumeButton;
	[SerializeField] public GameObject endPanel;
	[SerializeField] public Image[] stars;
	[SerializeField] private TextMeshProUGUI rewardText;
	[SerializeField] public GameObject lrPanel;
	[SerializeField] private TextMeshProUGUI lrRewardText;

	public static event Action OnRaceStart = delegate { };
	public static event Action OnRaceOver = delegate { };

	private GameObject player;
	private List<DistanceTracker> aiTrackers = new List<DistanceTracker> ();
	private int maxPos;
	private int racePosition;
	private int oldRacePos;
	private bool vwVisible;

	// LEVEL SET
	[HideInInspector] public Level levelSO;
	[HideInInspector] public WaypointCircuit circuit;
	private int _lapsTotal;
	private int _lap = 1;

	// RACE TIMER
	private float lapTime;
	private float lapRecordTime;
	private int record;
	private bool hasLapRecord;

	private DistanceTracker playerTracker;
	private Transform playerBody;
	private Transform finishLine;
	private List<RaceGridBox> gridPlaces = new List<RaceGridBox> ();


	public override void Awake () {
		base.Awake ();
		// Get Level object
		levelSO = GameManager.Instance.selectedLevel;
	}

	IEnumerator Start () {
		if (FindObjectOfType<EventSystem> () == null) {
			var es = new GameObject ("EventSystem", typeof (EventSystem));
			es.AddComponent<StandaloneInputModule> ();
		}

		// Audio Base Set
		AudioManager.Instance.UnpausedState ();

		// Start playing music for actual level
		AudioManager.Instance.PlayMusic (levelSO.playMusic);

		// SETUP LEVEL
		circuit = FindObjectOfType<WaypointCircuit> ();
		_lapsTotal = levelSO.laps;

		// SORTING RACE GRID AND DELETE INVALID
		finishLine = FindObjectOfType<FinishLine> ().transform;
		gridPlaces = FindObjectsOfType<RaceGridBox> ().ToList ();
		foreach (RaceGridBox gBox in gridPlaces.ToList ()) {
			if (gBox.transform.childCount == 0) {
				gridPlaces.Remove (gBox);
				Destroy (gBox.gameObject);
			}
		}
		gridPlaces.Sort (GridSort);

		// ADD PLAYER
		PlayerObject po = GameManager.Instance.GetSelectedPlayer ();
		Vector3 placePos = gridPlaces[0].transform.position + (gridPlaces[0].transform.up * GameManager.Instance.hoverHeight);
		player = Instantiate (po.gameplayPrefab, placePos, gridPlaces[0].transform.rotation);
		playerTracker = player.GetComponent<DistanceTracker> ();
		playerTracker.circuit = circuit;
		playerBody = player.GetComponent<PlayerShip> ().shipBody;

		aiTrackers.Add (playerTracker); // POSITIONFIX - we add player tracket to distance trackers list

		// ADD CAMERA & JOYPAD ON MOBILE BUILD
		raceCam = Instantiate (cameraPrefab, player.transform.position, player.transform.rotation).GetComponent<RacerCamera> ();
		raceCam.InitCamera (player.transform, playerBody);
		if (GameManager.Instance.BuildType == Build.MOBILE) mobileControls = Instantiate (joyPad);

		// ADD AI RACERS
		gridPlaces.RemoveAt (0);
		List<GameObject> AIs = new List<GameObject> (levelSO.AIs);
		AIs.Sort (RandomSort);
		int AIindex = 0;
		for (int i = 0; i < gridPlaces.Count; i++) {
			AIindex = (AIindex + 1) % AIs.Count;
			Vector3 AIPos = gridPlaces[i].transform.position + (gridPlaces[i].transform.up * GameManager.Instance.hoverHeight);
			GameObject AIply = Instantiate (AIs[AIindex], AIPos, gridPlaces[i].transform.rotation);
			AIply.GetComponent<WaypointProgressTracker> ().circuit = circuit;
			DistanceTracker disTr = AIply.GetComponent<DistanceTracker> ();
			disTr.circuit = circuit;
			aiTrackers.Add (disTr);
			// Set AI Cheat
			if (levelSO.cheatSpeed != 0 && levelSO.cheatDuration != 0) {
				float variation = levelSO.cheatSpeed / gridPlaces.Count * (i + 1);
				AIply.GetComponent<AiShip> ().Cheat_On (variation, levelSO.cheatDuration);
			}
		}

		// HUD INIT
		ammoType.text = "";
		ammoText.text = "";
		lapTimeText.text = "0:00:0";
		record = levelSO.LapRecord;
		if (record != 0)
			lapRecordText.text = FormatTime (record / 10f);
		else
			lapRecordLabel.SetActive (false);

		positionText.text = (aiTrackers.Count + 1).ToString ();
		ShowLap (_lap);


		// ALL SET, LOCK CURSOR AND READY TO RACE
		if (GameManager.Instance.BuildType != Build.MOBILE) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		GameManager.Instance.GameState = State.PLAY;

		// COUNTDOWN AND RACE START
		yield return new WaitForSeconds (startDelay);
		AudioManager.Instance.PlaySFX ("voice_3");
		countdownText.text = "3";
		yield return new WaitForSeconds (1f);
		AudioManager.Instance.PlaySFX ("voice_2");
		countdownText.text = "2";
		yield return new WaitForSeconds (1f);
		AudioManager.Instance.PlaySFX ("voice_1");
		countdownText.text = "1";
		yield return new WaitForSeconds (1f);
		AudioManager.Instance.PlaySFX ("voice_go");
		countdownText.text = "GO!";
		OnRaceStart ();

		yield return new WaitForSeconds (1f);
		Destroy (countdownText.gameObject);

		// In race coroutines
		if (playerTracker != null && aiTrackers.Count != 0) StartCoroutine (PositionCheck ());
		if (playerTracker != null) StartCoroutine (WrongWayCheck ());

	}



	public void OnPause () {
		if (!pausePanel.activeInHierarchy)
			PauseGame ();
		else if (resumeButton.gameObject.activeInHierarchy) resumeButton.onClick.Invoke ();
	}




	public void PauseGame () {
		if (GameManager.Instance.GameState == State.PAUSE) {
			Time.timeScale = 1f;
			pausePanel.SetActive (false);
			GameManager.Instance.GameState = State.PLAY;
			AudioManager.Instance.UnpausedState ();
			if (mobileControls != null) mobileControls.SetActive (true);
			else {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		} else if (GameManager.Instance.GameState == State.PLAY) {
			Time.timeScale = 0f;
			if (mobileControls != null) mobileControls.SetActive (false);
			pausePanel.SetActive (true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			GameManager.Instance.GameState = State.PAUSE;
			AudioManager.Instance.PausedState ();
		}
	}


	// Methods for buttons
	public void Restart () => LoadScene.Instance.ReloadScene ();
	public void QuitTo (string menuScene) => LoadScene.Instance.LoadByName (menuScene);


	public void RaceOver () {
		GameManager.Instance.GameState = State.OVER;
		StopAllCoroutines ();
		positionText.text = racePosition.ToString ();
		raceCam.CameraTilt = 0.7f;
		Destroy (playerTracker);

		// CALCULATE REWARD AND SHOW
		int rew = (aiTrackers.Count + 2) * levelSO.Reward - racePosition * levelSO.Reward;
		rewardText.text = $"{rew} x";
		if (hasLapRecord) {
			lrPanel.SetActive (true);
			lrRewardText.text = $"{levelSO.LapReward} x";
		}
		rew += levelSO.LapReward;
		GameManager.Instance.AddMoney (rew);

		// Calculate and add earned stars
		if (racePosition <= 3) {
			int str = 4 - racePosition;
			int sCol = levelSO.StarsCollected;
			if (str > sCol) levelSO.StarsCollected = str;
			for (int i = 0; i < str; i++) if (stars[i] != null) stars[i].color = Color.white;
		}
		// Delete DistanceTrackers, we dont need them after race end
		foreach (DistanceTracker item in aiTrackers) Destroy (item);

		//hudPanel.SetActive (false);
		if (mobileControls != null) mobileControls.SetActive (false);

		endPanel.SetActive (true);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		OnRaceOver ();
	}


	// HUD OPERATIONS
	public void ShowInfo (string info, float duration) {
		infoText.text = info;
		CancelInvoke ("HideInfo");
		Invoke ("HideInfo", duration);
	}
	public void HideInfo () => infoText.text = "";

	public void ShowAmmoType (string ammoT) => ammoType.text = ammoT;
	public void ShowAmmo (string ammo) {
		if (ammo == "" || ammo == "0") {
			ammoType.text = "";
			ammoText.text = "";
		} else ammoText.text = ammo;
	}

	public bool CanReload (string ammoName) {
		if (ammoType.text == "" || ammoType.text == ammoName)
			return true;
		else
			return false;
	}

	private string FormatTime (float timeSec) {
		int minutes = (int)timeSec / 60;
		int seconds = (int)timeSec % 60;
		int milliseconds = (int)(10 * (timeSec - minutes * 60 - seconds));
		return string.Format ("{0:0}:{1:00}:{2:0}", minutes, seconds, milliseconds);
	}


	//	 ANDROID PUSH BACK APP INTERACTIONS
#if UNITY_ANDROID
	void OnApplicationFocus (bool focusStatus) {
		if (!focusStatus && !pausePanel.activeSelf) PauseGame ();
	}
#endif


	// COROUTINES =================================================================
	// Count and Show Lap Time
	IEnumerator TimeCounter () {
		WaitForSeconds delay = new WaitForSeconds (0.1f);
		while (true) {
			yield return delay;
			lapTime += 0.1f;
			lapTimeText.text = FormatTime (lapTime);
		}
	}

	// Calculate and Show Racing Position
	IEnumerator PositionCheck () {  // POSITIONFIX
		WaitForSeconds delay = new WaitForSeconds (0.1f);
		while (true) {
			aiTrackers.Sort (SortByDistance);

			for (int i = 0; i < aiTrackers.Count; i++) aiTrackers[i].racePosition = i + 1;

			racePosition = playerTracker.racePosition;

			if (oldRacePos != racePosition) {
				oldRacePos = racePosition;
				positionText.text = racePosition.ToString ();
			}

			yield return delay;
		}
	}


	private int SortByDistance (DistanceTracker p1, DistanceTracker p2) => p2.progressDistance.CompareTo (p1.progressDistance);


	// Wrong Way Detection
	IEnumerator WrongWayCheck () {
		WaitForSeconds delay = new WaitForSeconds (0.25f);
		while (true) {
			if (Vector3.Angle (playerBody.forward, playerTracker.tWay) > 120f && !vwVisible) {
				infoText.text = "WRONG WAY!";
				vwVisible = true;
			} else if (vwVisible) {
				infoText.text = "";
				vwVisible = false;
			}

			yield return delay;
		}
	}


	// RACE Functions =====================================================

	public void LapCount () {
		if (_lap == 1 && lapTime == 0) {
			ShowLap (_lap);
			StartCoroutine (TimeCounter ());
			AudioManager.Instance.PlaySFX ("lap");
		}
		// Check if player completed all laps
		if (playerTracker.progressDistance >= _lap * (circuit.Length - 100)) {
			_lap++;
			if (_lap <= _lapsTotal) {
				CheckRecord ();
				ShowLap (_lap);
				lapTime = 0;
			}
			// RACE END
			if (_lap == _lapsTotal + 1) {
				StopCoroutine (TimeCounter ());
				CheckRecord ();
				RaceOver ();
			}
			AudioManager.Instance.PlaySFX ("lap");
		}

	}

	private void CheckRecord () {
		int checkTime = Mathf.FloorToInt (lapTime * 10);
		if (record == 0 || checkTime < record) {
			hasLapRecord = true;
			lapRecordLabel.SetActive (true);
			lapRecordText.text = FormatTime (lapTime);
			levelSO.LapRecord = checkTime;
			record = checkTime;
			// Communicate Lap Record to Player
			ShowInfo ("NEW LAP RECORD", 2);
			// SFX
		}
	}

	private void ShowLap (int lap) => lapText.text = $"{lap}/{_lapsTotal}";


	// Sorting AIs randomly to have different start positions
	private int RandomSort (GameObject item_1, GameObject item_2) {
		if (item_1 && item_2) {
			return UnityEngine.Random.value < 0.5f ? 1 : -1;
		} else {
			return 0;
		}
	}

	// Sorting Race Grid List by Grid Box distsnces from Start/Finish
	private int GridSort (RaceGridBox item_1, RaceGridBox item_2) {
		if (item_1 && item_2) {
			if (Vector3.Distance (finishLine.position, item_1.transform.position) <= Vector3.Distance (finishLine.position, item_2.transform.position)) {
				return 1;
			} else {
				return -1;
			}
		} else {
			return 0;
		}
	}

}
