using UnityEngine;
using TMPro;

public class UpgradePanel : MonoBehaviour {

	public string onMax = "FULL TUNING";

	[Range (0.2f, 0.9f)] public float disabledAlpha = 0.6f;

	[Space (10)]
	public Hangar hangar;

	[Space (15)]
	[SerializeField] private CanvasGroup turnUpButton;
	[SerializeField] private TextMeshProUGUI turnUpPriceText;
	[Space (5)]
	//[SerializeField] private CanvasGroup driftUpButton;
	//[SerializeField] private TextMeshProUGUI driftUpPriceText;
	//[Space(5)]
	[SerializeField] private CanvasGroup accelerationUpButton;
	[SerializeField] private TextMeshProUGUI accelerationUpPriceText;
	[Space (5)]
	[SerializeField] private CanvasGroup thrustUpButton;
	[SerializeField] private TextMeshProUGUI thrustUpPriceText;
	[Space (5)]
	[SerializeField] private CanvasGroup turboUpButton;
	[SerializeField] private TextMeshProUGUI turboUpPriceText;


	private void OnEnable () => ReadPrices ();

	void Start () {
		if (!GameManager.Instance.useIndividualTurn) turnUpButton.gameObject.SetActive (false);
	}


	private void Show (CanvasGroup canv) {
		canv.alpha = 1f;
		canv.interactable = true;
		canv.blocksRaycasts = true;
	}

	private void Hide (CanvasGroup canv) {
		canv.alpha = disabledAlpha;
		canv.interactable = false;
		canv.blocksRaycasts = false;
	}


	public void ReadPrices () {
		// READ ACTUAL PRICES & CHECK IF UPGRADE REACHED MAXIMUM
		if (turnUpButton != null && GameManager.Instance.useIndividualTurn)
			if (GameManager.Instance.players[hangar.selectedPlayer].MaxTurn ()) {
				Hide (turnUpButton);
				turnUpPriceText.text = onMax;
			} else {
				Show (turnUpButton);
				turnUpPriceText.text = GameManager.Instance.players[hangar.selectedPlayer].turnPrice.ToString ();
			}

		//if (driftUpButton != null)
		//if (GameManager.Instance.players[hangar.selectedPlayer].MaxDrift ()) {
		//	Hide (driftUpButton);
		//	driftUpPriceText.text = onMax;
		//} else {
		//	Show (driftUpButton);
		//	driftUpPriceText.text = GameManager.Instance.players[hangar.selectedPlayer].driftPrice.ToString ();
		//}

		if (accelerationUpButton != null)
			if (GameManager.Instance.players[hangar.selectedPlayer].MaxAcceleration ()) {
				Hide (accelerationUpButton);
				accelerationUpPriceText.text = onMax;
			} else {
				Show (accelerationUpButton);
				accelerationUpPriceText.text = GameManager.Instance.players[hangar.selectedPlayer].accelerationPrice.ToString ();
			}

		if (thrustUpButton != null)
			if (GameManager.Instance.players[hangar.selectedPlayer].MaxThrust ()) {
				Hide (thrustUpButton);
				thrustUpPriceText.text = onMax;
			} else {
				Show (thrustUpButton);
				thrustUpPriceText.text = GameManager.Instance.players[hangar.selectedPlayer].normalThrustPrice.ToString ();
			}

		if (turboUpButton != null)
			if (GameManager.Instance.players[hangar.selectedPlayer].MaxTurbo ()) {
				Hide (turboUpButton);
				turboUpPriceText.text = onMax;
			} else {
				Show (turboUpButton);
				turboUpPriceText.text = GameManager.Instance.players[hangar.selectedPlayer].turboTimePrice.ToString ();
			}

	}

}
