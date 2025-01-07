using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Hangar : MonoBehaviour {

	[SerializeField] private MenuManager infoManager;
	[SerializeField] private GameObject levelSelector;
	[SerializeField] private UpgradePanel upgrade;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private GameObject lockIcon;
	[SerializeField] private TextMeshProUGUI priceText;

	private bool showTurn;
	private float turnValue;
	private float driftValue;
	private float accelerationValue;
	private float normalThrustValue;
	private float turboValue;

	[Header ("PLAYER CONDITION SLIDERS")]
	[SerializeField] private Slider turnSpeedSliderMAX;
	[SerializeField] private Slider turnSpeedSlider;
	[Space (5)]
	[SerializeField] private Slider driftSpeedSliderMAX;
	[SerializeField] private Slider driftSpeedSlider;
	[Space (5)]
	[SerializeField] private Slider accelerationSliderMAX;
	[SerializeField] private Slider accelerationSlider;
	[Space (5)]
	[SerializeField] private Slider normalThrustSliderMAX;
	[SerializeField] private Slider normalThrustSlider;
	[Space (5)]
	[SerializeField] private Slider turboTimeSliderMAX;
	[SerializeField] private Slider turboTimeSlider;

	[Header ("MENU BUTTONS")]
	[SerializeField] private GameObject buyButton;
	[SerializeField] private GameObject updateFail;
	private Button buy;
	[SerializeField] private GameObject playButton;

	[HideInInspector] public int selectedPlayer;
	private float deltaT;


	void Start () {
		GameManager.Instance.LoadPlayerData ();
		GameManager.Instance.SortPlayers ();

		showTurn = GameManager.Instance.useIndividualTurn;
		if (!showTurn) turnSpeedSliderMAX.transform.parent.gameObject.SetActive (false);

		buy = buyButton.GetComponent<Button> ();

		string selected = PlayerPrefs.GetString ("selected", "");
		if (selected != "") selectedPlayer = GameManager.Instance.GetPlayerIndex (selected);

		ShowPlayer ();
	}


	private void Update () {
		// SMOOTH SET INFI SLIDERS TO PREFERED VALUE
		deltaT = Time.deltaTime;
		accelerationSlider.value = Mathf.Lerp (accelerationSlider.value, accelerationValue, 15 * deltaT);
		normalThrustSlider.value = Mathf.Lerp (normalThrustSlider.value, normalThrustValue, 15 * deltaT);
		turboTimeSlider.value = Mathf.Lerp (turboTimeSlider.value, turboValue, 15 * deltaT);
		driftSpeedSlider.value = Mathf.Lerp (driftSpeedSlider.value, driftValue, 15 * deltaT);
		if (showTurn) turnSpeedSlider.value = Mathf.Lerp (turnSpeedSlider.value, turnValue, 15 * deltaT);
	}


	public void OnSlideLeft (InputAction.CallbackContext value) { if (value.started) SlideLeft (); }
	public void SlideLeft () {
		if (levelSelector.activeSelf) return;

		if (selectedPlayer > 0) selectedPlayer--;
		else selectedPlayer = GameManager.Instance.players.Count - 1;

		foreach (Transform child in gameObject.transform) Destroy (child.gameObject);

		AudioManager.Instance.PlaySFX ("slide");

		ShowPlayer ();
	}


	public void OnSlideRight (InputAction.CallbackContext value) { if (value.started) SlideRight (); }
	public void SlideRight () {
		if (levelSelector.activeSelf) return;

		if (selectedPlayer < GameManager.Instance.players.Count - 1) selectedPlayer++;
		else selectedPlayer = 0;

		foreach (Transform child in gameObject.transform) Destroy (child.gameObject);

		AudioManager.Instance.PlaySFX ("slide");

		ShowPlayer ();
	}


	public void BuyPlayer () {
		if (GameManager.Instance.Money >= GameManager.Instance.players[selectedPlayer].price) {
			GameManager.Instance.RemoveMoney (GameManager.Instance.players[selectedPlayer].price);
			GameManager.Instance.players[selectedPlayer].UnlockPlayer ();
			PlayerPrefs.SetString ("selected", GameManager.Instance.players[selectedPlayer].GetInstanceID ().ToString ());
			buyButton.SetActive (false);
			lockIcon.SetActive (false);
			priceText.text = "";
			infoManager.UpdateMoneyInfo ();
			playButton.SetActive (true);
			upgrade.gameObject.SetActive (true);
			AudioManager.Instance.PlaySFX ("upgrade");
		}
	}


	private void ShowPlayer () {
		PlayerObject player = GameManager.Instance.players[selectedPlayer];

		// Instantiate Prefab from Scriptable object of selected player
		Instantiate (player.menuPrefab, transform.position, transform.rotation, transform);
		// Set selected player parameters
		nameText.text = player.playerName;

		if (showTurn) {
			turnValue = player.turn;
			turnSpeedSlider.minValue = player.turnSliderMin;
			turnSpeedSlider.maxValue = player.turnSliderMax;
			turnSpeedSliderMAX.minValue = player.turnSliderMin;
			turnSpeedSliderMAX.maxValue = player.turnSliderMax;
			turnSpeedSliderMAX.value = player.turnMax;
		}

		driftValue = player.drift;
		driftSpeedSlider.minValue = player.driftSliderMin;
		driftSpeedSlider.maxValue = player.driftSliderMax;
		driftSpeedSliderMAX.minValue = player.driftSliderMin;
		driftSpeedSliderMAX.maxValue = player.driftSliderMax;
		driftSpeedSliderMAX.value = player.driftMax;

		accelerationValue = player.acceleration;
		accelerationSlider.minValue = player.accelerationSliderMin;
		accelerationSlider.maxValue = player.accelerationSliderMax;
		accelerationSliderMAX.minValue = player.accelerationSliderMin;
		accelerationSliderMAX.maxValue = player.accelerationSliderMax;
		accelerationSliderMAX.value = player.accelerationMax;

		normalThrustValue = player.normalThrust;
		normalThrustSlider.minValue = player.thrustSliderMin;
		normalThrustSlider.maxValue = player.thrustSliderMax;
		normalThrustSliderMAX.minValue = player.thrustSliderMin;
		normalThrustSliderMAX.maxValue = player.thrustSliderMax;
		normalThrustSliderMAX.value = player.normalThrustMax;

		turboValue = player.turboTime;
		turboTimeSlider.minValue = player.turboSliderMin;
		turboTimeSlider.maxValue = player.turboSliderMax;
		turboTimeSliderMAX.minValue = player.turboSliderMin;
		turboTimeSliderMAX.maxValue = player.turboSliderMax;
		turboTimeSliderMAX.value = player.turboTimeMax;

		if (player.unlocked) {
			buyButton.SetActive (false);
			lockIcon.SetActive (false);
			priceText.text = "";
			PlayerPrefs.SetString ("selected", GameManager.Instance.players[selectedPlayer].GetInstanceID ().ToString ());
			playButton.SetActive (true);
			upgrade.gameObject.SetActive (true);
		} else {
			if (GameManager.Instance.Money >= player.price) {
				priceText.text = "buy for " + player.price.ToString ();
				buy.interactable = true;
			} else {
				priceText.text = "ship price " + player.price.ToString ();
				buy.interactable = false;
			}
			lockIcon.SetActive (true);
			buyButton.SetActive (true);
			playButton.SetActive (false);
			upgrade.gameObject.SetActive (false);
		}

		upgrade.ReadPrices ();
	}


	// UPGRADE Player
	private void UpgradeSuccess () {
		upgrade.ReadPrices ();
		AudioManager.Instance.PlaySFX ("upgrade");
	}

	public void UpgradeTurn () {
		int price = GameManager.Instance.players[selectedPlayer].turnPrice;
		if (GameManager.Instance.Money >= price) {
			GameManager.Instance.RemoveMoney (price);
			infoManager.UpdateMoneyInfo ();
			GameManager.Instance.players[selectedPlayer].UpgradePlayer (UpgradeType.TURN);
			turnValue = GameManager.Instance.players[selectedPlayer].turn;
			UpgradeSuccess ();
		} else {
			updateFail.SetActive (true);
		}
	}

	//public void UpgradeDrift () {
	//	int price = GameManager.Instance.players[selectedPlayer].driftPrice;
	//	if (GameManager.Instance.Money >= price) {
	//		GameManager.Instance.RemoveMoney (price);
	//		infoManager.UpdateMoneyInfo ();
	//		GameManager.Instance.players[selectedPlayer].UpgradePlayer (UpgradeType.DRIFT);
	//		driftValue = GameManager.Instance.players[selectedPlayer].drift;
	//		UpgradeSuccess ();
	//	} else {
	//		updateFail.SetActive (true);
	//	}
	//}

	public void UpgradeAcceleration () {
		int price = GameManager.Instance.players[selectedPlayer].accelerationPrice;
		if (GameManager.Instance.Money >= price) {
			GameManager.Instance.RemoveMoney (price);
			infoManager.UpdateMoneyInfo ();
			GameManager.Instance.players[selectedPlayer].UpgradePlayer (UpgradeType.ACCELERATION);
			accelerationValue = GameManager.Instance.players[selectedPlayer].acceleration;
			UpgradeSuccess ();
		} else {
			updateFail.SetActive (true);
		}
	}

	public void UpgradeThrust () {
		int price = GameManager.Instance.players[selectedPlayer].normalThrustPrice;
		if (GameManager.Instance.Money >= price) {
			GameManager.Instance.RemoveMoney (price);
			infoManager.UpdateMoneyInfo ();
			GameManager.Instance.players[selectedPlayer].UpgradePlayer (UpgradeType.THRUST);
			normalThrustValue = GameManager.Instance.players[selectedPlayer].normalThrust;
			UpgradeSuccess ();
		} else {
			updateFail.SetActive (true);
		}
	}

	public void UpgradeTurbo () {
		int price = GameManager.Instance.players[selectedPlayer].turboTimePrice;
		if (GameManager.Instance.Money >= price) {
			GameManager.Instance.RemoveMoney (price);
			infoManager.UpdateMoneyInfo ();
			GameManager.Instance.players[selectedPlayer].UpgradePlayer (UpgradeType.TURBO);
			turboValue = GameManager.Instance.players[selectedPlayer].turboTime;
			UpgradeSuccess ();
		} else {
			updateFail.SetActive (true);
		}
	}

}
