using UnityEngine;

[CreateAssetMenu (menuName = "Player")]
public class PlayerObject : ScriptableObject {

	public string playerName;
	public int price = 100;
	public bool unlocked = true;

	[Header ("Individual Turn")]
	[Range (2, 10)] public int turnUpgradeSteps = 4;
	[Range (300f, 500f)] public float turn = 400f;
	[Range (300f, 500f)] public float turnMax = 500f;
	[HideInInspector] public float turnMin = 300f;
	[HideInInspector] public float turnSliderMin = 300f;
	[HideInInspector] public float turnSliderMax = 500f;
	[Space (8)]

	[Header ("Setup")]
	[Range (0.1f, 0.5f)] public float drift = 0.3f;
	[Range (0.1f, 0.5f)] public float driftMax = 0.5f;
	[HideInInspector] public float driftMin = 0.1f;
	[HideInInspector] public float driftSliderMin = 0;
	[HideInInspector] public float driftSliderMax = 0.5f;
	[Space (8)]
	[Range (2, 10)] public int accelUpgradeSteps = 5;
	[Range (0.005f, 0.02f)] public float acceleration = 0.01f;
	[Range (0.005f, 0.02f)] public float accelerationMax = 0.02f;
	[HideInInspector] public float accelerationMin = 0.005f;
	[HideInInspector] public float accelerationSliderMin = 0.005f;
	[HideInInspector] public float accelerationSliderMax = 0.02f;
	[Space (8)]
	[Range (2, 10)] public int thrustUpgradeSteps = 5;
	[Range (1500f, 2000f)] public float normalThrust = 1500f;
	[Range (1500f, 2000f)] public float normalThrustMax = 2000f;
	[HideInInspector] public float normalThrustMin = 1500f;
	[HideInInspector] public float thrustSliderMin = 1500f;
	[HideInInspector] public float thrustSliderMax = 2000f;
	[Space (8)]
	[Range (2, 10)] public int turboUpgradeSteps = 5;
	[Range (1f, 4f)] public float turboTime = 2.5f;
	[Range (1f, 4f)] public float turboTimeMax = 4f;
	[HideInInspector] public float turboTimeMin = 1f;
	[HideInInspector] public float turboSliderMin = 0;
	[HideInInspector] public float turboSliderMax = 4f;


	[Header ("Upgrade")]
	public int turnPrice = 300;
	public int turnPriceRaise = 200;
	//[Space (10)]
	//public int driftPrice = 300;
	//public int driftPriceRaise = 200;
	[Space (10)]
	public int accelerationPrice = 50;
	public int accelerationPriceRaise = 10;
	[Space (10)]
	public int normalThrustPrice = 50;
	public int normalThrustPriceRaise = 10;
	[Space (10)]
	public int turboTimePrice = 100;
	public int turboTimePriceRaise = 50;


	[Header("Prefabs")]
	public GameObject menuPrefab;
	public GameObject gameplayPrefab;


	public void OnValidate () {
		if (playerName.Length == 0) playerName = this.name;

		turnPrice = Mathf.RoundToInt (turnPrice / 5) * 5;
		turnPriceRaise = Mathf.RoundToInt (turnPriceRaise / 5) * 5;
		//driftPrice = Mathf.RoundToInt (driftPrice / 5) * 5;
		//driftPriceRaise = Mathf.RoundToInt (driftPriceRaise / 5) * 5;
		accelerationPrice = Mathf.RoundToInt (accelerationPrice / 5) * 5;
		accelerationPriceRaise = Mathf.RoundToInt (accelerationPriceRaise / 5) * 5;
		normalThrustPrice = Mathf.RoundToInt (normalThrustPrice / 5) * 5;
		normalThrustPriceRaise = Mathf.RoundToInt (normalThrustPriceRaise / 5) * 5;
		turboTimePrice = Mathf.RoundToInt (turboTimePrice / 5) * 5;
		turboTimePriceRaise = Mathf.RoundToInt (turboTimePriceRaise / 5) * 5;

		// CLAMP PLAYER SETTINGS TO ITS MAX BY UPGRADE STEPS
		float roundBy = 1;

		roundBy = (turnSliderMax - turnMin) / turnUpgradeSteps;
		turn = turnMin + Mathf.RoundToInt ((turn - turnMin) / roundBy) * roundBy;
		turnMax = turnMin + Mathf.RoundToInt ((turnMax - turnMin) / roundBy) * roundBy;
		turnMax = Mathf.Clamp (turnMax, turnMin + roundBy, thrustSliderMax);
		turn = Mathf.Clamp (turn, turnMin, turnMax);
		turnSliderMin = Mathf.RoundToInt (turnMin - roundBy);

		roundBy = (driftSliderMax - driftMin) / 5;
		drift = driftMin + Mathf.RoundToInt ((drift - driftMin) / roundBy) * roundBy;
		driftMax = driftMin + Mathf.RoundToInt ((driftMax - driftMin) / roundBy) * roundBy;
		driftMax = Mathf.Clamp (driftMax, driftMin + roundBy, thrustSliderMax);
		drift = Mathf.Clamp (drift, driftMin, driftMax);
		driftSliderMin = Mathf.RoundToInt (driftMin - roundBy);

		roundBy = (accelerationSliderMax - accelerationMin) / accelUpgradeSteps;
		acceleration = accelerationMin + Mathf.RoundToInt ((acceleration - accelerationMin) / roundBy) * roundBy;
		accelerationMax = accelerationMin + Mathf.RoundToInt ((accelerationMax - accelerationMin) / roundBy) * roundBy;
		accelerationMax = Mathf.Clamp (accelerationMax, accelerationMin + roundBy, thrustSliderMax);
		acceleration = Mathf.Clamp (acceleration, accelerationMin, accelerationMax);
		accelerationSliderMin = Mathf.RoundToInt (accelerationMin - roundBy);

		roundBy = (thrustSliderMax - normalThrustMin) / thrustUpgradeSteps;
		normalThrust = normalThrustMin + Mathf.RoundToInt ((normalThrust - normalThrustMin) / roundBy) * roundBy;
		normalThrustMax = normalThrustMin + Mathf.RoundToInt ((normalThrustMax - normalThrustMin) / roundBy) * roundBy;
		normalThrustMax = Mathf.Clamp (normalThrustMax, normalThrustMin + roundBy, thrustSliderMax);
		normalThrust = Mathf.Clamp (normalThrust, normalThrustMin, normalThrustMax);
		thrustSliderMin = Mathf.RoundToInt (normalThrustMin - roundBy);

		roundBy = (turboSliderMax - turboTimeMin) / turboUpgradeSteps;
		turboTime = turboTimeMin + Mathf.RoundToInt ((turboTime - turboTimeMin) / roundBy) * roundBy;
		turboTimeMax = turboTimeMin + Mathf.RoundToInt ((turboTimeMax - turboTimeMin) / roundBy) * roundBy;
		turboTimeMax = Mathf.Clamp (turboTimeMax, turboTimeMin + roundBy, turboSliderMax);
		turboTime = Mathf.Clamp (turboTime, turboTimeMin, turboTimeMax);
	}


	public void UpgradePlayer (UpgradeType upgrade) {
		float upStep = 0;

		switch (upgrade) {
			case UpgradeType.TURN:
				upStep = (turnSliderMax - turnMin) / turnUpgradeSteps;
				turn = Mathf.Clamp (turn + upStep, turn, turnMax);
				turnPrice += turnPriceRaise;
				break;

			//case UpgradeType.DRIFT:
			//	upStep = (driftSliderMax - driftMin) / driftUpgradeSteps;
			//	drift = Mathf.Clamp (drift + upStep, drift, driftMax);
			//	driftPrice += driftPriceRaise;
			//	break;

			case UpgradeType.ACCELERATION:
				upStep = (accelerationSliderMax - accelerationMin) / accelUpgradeSteps;
				acceleration = Mathf.Clamp (acceleration + upStep, acceleration, accelerationMax);
				accelerationPrice += accelerationPriceRaise;
				break;

			case UpgradeType.THRUST:
				upStep = (thrustSliderMax - normalThrustMin) / thrustUpgradeSteps;
				normalThrust = Mathf.Clamp (normalThrust + upStep, normalThrust, normalThrustMax);
				normalThrustPrice += normalThrustPriceRaise;
				break;

			case UpgradeType.TURBO:
				upStep = (turboSliderMax - turboTimeMin) / turboUpgradeSteps;
				turboTime = Mathf.Clamp (turboTime + upStep, turboTime, turboTimeMax);
				turboTimePrice += turboTimePriceRaise;
				break;
		}

		SaveData ();

	}


	public bool MaxTurn () {
		return turn >= turnMax ? true : false;
	}

	public bool MaxDrift () {
		return drift >= driftMax ? true : false;
	}

	public bool MaxAcceleration () {
		return acceleration >= accelerationMax ? true : false;
	}

	public bool MaxThrust () {
		return normalThrust >= normalThrustMax ? true : false;
	}

	public bool MaxTurbo () {
		return turboTime >= turboTimeMax ? true : false;
	}


	public void UnlockPlayer () {
		unlocked = true;
		PlayerPrefs.SetInt (this.GetInstanceID ().ToString () + "unlocked", 1);
	}


	private void AssignDataToPrefab () {
		PlayerShip ship = gameplayPrefab.GetComponent<PlayerShip> ();

		if (GameManager.Instance.useIndividualTurn)
			ship.turn = turn;
		else
			ship.turn = GameManager.Instance.playerTurn;

		ship.drift = drift;
		ship.acceleration = acceleration;
		ship.normalThrust = normalThrust;
		ship.turboThrust = normalThrust * 1.3f;
		ship.turboTime = turboTime;
	}


	#region SAVING AND LOADING

	public void LoadData () {
		string ID = this.GetInstanceID ().ToString ();

		if (PlayerPrefs.GetInt (ID + "unlocked", 0) == 1) unlocked = true;

		turn = PlayerPrefs.GetFloat (ID + "turn", turn);
		turnPrice = PlayerPrefs.GetInt (ID + "turnPrice", turnPrice);

		//drift = PlayerPrefs.GetFloat (ID + "drift", drift);
		//driftPrice = PlayerPrefs.GetInt (ID + "driftPrice", driftPrice);

		acceleration = PlayerPrefs.GetFloat (ID + "acceleration", acceleration);
		accelerationPrice = PlayerPrefs.GetInt (ID + "accelerationPrice", accelerationPrice);

		normalThrust = PlayerPrefs.GetFloat (ID + "normalThrust", normalThrust);
		normalThrustPrice = PlayerPrefs.GetInt (ID + "normalThrustPrice", normalThrustPrice);

		turboTime = PlayerPrefs.GetFloat (ID + "turboTime", turboTime);
		turboTimePrice = PlayerPrefs.GetInt (ID + "turboTimePrice", turboTimePrice);

		// ASSIGN VALUES TO SHIP PREFAB
		AssignDataToPrefab ();
	}


	public void SaveData () {
		string ID = this.GetInstanceID ().ToString ();

		if (unlocked) PlayerPrefs.SetInt (ID + "unlocked", 1);

		PlayerPrefs.SetFloat (ID + "turn", turn);
		PlayerPrefs.SetInt (ID + "turnPrice", turnPrice);

		//PlayerPrefs.SetFloat (ID + "drift", drift);
		//PlayerPrefs.SetInt (ID + "driftPrice", driftPrice);

		PlayerPrefs.SetFloat (ID + "acceleration", acceleration);
		PlayerPrefs.SetInt (ID + "accelerationPrice", accelerationPrice);

		PlayerPrefs.SetFloat (ID + "normalThrust", normalThrust);
		PlayerPrefs.SetInt (ID + "normalThrustPrice", normalThrustPrice);

		PlayerPrefs.SetFloat (ID + "turboTime", turboTime);
		PlayerPrefs.SetInt (ID + "turboTimePrice", turboTimePrice);

		// ASSIGN VALUES TO SHIP PREFAB
		AssignDataToPrefab ();
	}


	public void DeletePrefs () {
		string ID = this.GetInstanceID ().ToString ();
		PlayerPrefs.DeleteKey (ID + "unlocked");
		PlayerPrefs.DeleteKey (ID + "turn");
		PlayerPrefs.DeleteKey (ID + "turnPrice");
		PlayerPrefs.DeleteKey (ID + "drift");
		PlayerPrefs.DeleteKey (ID + "driftPrice");
		PlayerPrefs.DeleteKey (ID + "acceleration");
		PlayerPrefs.DeleteKey (ID + "accelerationPrice");
		PlayerPrefs.DeleteKey (ID + "normalThrust");
		PlayerPrefs.DeleteKey (ID + "normalThrustPrice");
		PlayerPrefs.DeleteKey (ID + "turboTime");
		PlayerPrefs.DeleteKey (ID + "turboTimePrice");
	}

	#endregion

}
