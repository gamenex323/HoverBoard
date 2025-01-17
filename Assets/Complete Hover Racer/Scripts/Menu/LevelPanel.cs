using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public TextMeshProUGUI levelName;
	public Image displayImage;
	[Space (10)]
	public Image[] stars;
	[Space(10)]
	public GameObject lockedPanel;
	public TextMeshProUGUI lockedValue;

	private Level levelSO;
	private ScrollToHovered scrollControl;
	private bool desktopScroll;


	private void Awake () {		
		if (GameManager.Instance.BuildType != Build.MOBILE) {
			desktopScroll = true;
			scrollControl = transform.parent.GetComponent<ScrollToHovered> ();
		}
	}


	public void SetLevel (Level lev) {
		levelSO = lev;
		// Show Level Icon
		displayImage.sprite = levelSO.displayImage;
		// Show Level Name
		levelName.text = levelSO.displayName;
		
		if (levelSO.unlocked) { // WHEN LEVEL IS UNLOCKED
			// Set button interactable so you can select this level
			GetComponent<Button> ().interactable = true;
			// Showing star icons
			foreach (Image item in stars) item.enabled = true;
			// Set full color for collected stars
			int starsLen = levelSO.StarsCollected;
			for (int i = 0; i < starsLen; i++) if (stars[i] != null) stars[i].color = Color.white;
		} else {    // WHEN LEVEL IS LOCKED
			lockedPanel.SetActive (true);
			lockedValue.text = levelSO.starsToUnlock.ToString (); // Show how many stars needed to unlock
		}

	}

	public void GoTolevel () {
		GameManager.Instance.selectedLevel = levelSO;
		PhotonAuth.Instance.ScenceName = levelSO.sceneNameForLoad;
		//LoadScene.Instance.LoadByName (levelSO.sceneNameForLoad);
		PhotonAuth.Instance.JoinOrCreateRoom();
		if (UIManager.Instance)
			UIManager.Instance.CreateRoomPanel.SetActive(true);
	}


	// OnPOinter Interfaces
	public void OnPointerEnter (PointerEventData pointerEventData) {
		if (desktopScroll) scrollControl.hoverPos = transform.localPosition.x;
	}

	public void OnPointerExit (PointerEventData pointerEventData) {
	}

}
