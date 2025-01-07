using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

	public TextMeshProUGUI infoText;
	public TextMeshProUGUI MoneyText;

	public GameObject ExitPanel;

	public Button[] BackButtons;

	[Header ("MUSIC NAME IN RESOURCES")]
	public string playMusic;

	private string tempHeadline;


	void Start () {
		MoneyText.text = GameManager.Instance.Money.ToString ();

		// MUSIC PLAY
		AudioManager.Instance.UnpausedState ();
		if (playMusic != "") AudioManager.Instance.PlayMusic (playMusic);
	}


	public void Escape () {
		bool close = false;
		foreach (Button bt in BackButtons)
			if (bt.gameObject.activeInHierarchy) {
				bt.onClick.Invoke ();
				close = true;
			}
		if (!close && !ExitPanel.activeSelf) {
			ExitPanel.SetActive (true);
		}
	}


	public void UpdateMoneyInfo () {
		MoneyText.text = GameManager.Instance.Money.ToString ();
		// SFX
	}

	public void AddMoney (int extraMoney) {
		GameManager.Instance.AddMoney (extraMoney);
		UpdateMoneyInfo ();
		AudioManager.Instance.PlaySFX ("upgrade");
	}


	public void QuitGame () {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

}
