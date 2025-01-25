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
    public void AddAmmo(int extraAmmo)
    {
		GlobalData.Ammo += extraAmmo;
        AudioManager.Instance.PlaySFX("upgrade");
    }    

	public void BuyAmmo(int buyID)
	{
		switch (buyID)
		{
			case 0:
				if (PurchaseWithMoney(200))
				{
                    AddAmmo(1000);
                }
                break;
            case 1:
                if (PurchaseWithMoney(300))
                {
                    AddAmmo(2000);
                }
                break;
            case 2:
                if (PurchaseWithMoney(400))
                {
                    AddAmmo(3000);
                }
                break;
        }
        UpdateMoneyInfo();
    }
	public bool PurchaseWithMoney(int money)
    {
		if (GameManager.Instance.Money >= money)
		{
            GameManager.Instance.RemoveMoney (money);
			return true;
        }
		else
		{
			return false;
		}
    }


    public void QuitGame () {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

}
