using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour {

	public TextMeshProUGUI starsText;

	[Header ("Prefabs")]
	public GameObject panelPrefab;
	[Space (10)]
	public Transform slideContent;

	public Button backToMenuButton;

	private Button prevButton;


	private void OnEnable () {
		if (slideContent.childCount > 0) {
			EventSystem.current.SetSelectedGameObject (null);
			EventSystem.current.SetSelectedGameObject (slideContent.GetChild (0).gameObject);
		}
	}



	// First we count and show All Collected stars before adding level panels 
	private void Awake () => starsText.text = GameManager.Instance.GetAllStars ().ToString ();

	private IEnumerator Start () {
		// Adding level panels

		foreach (Level lev in GameManager.Instance.levels) {

			GameObject levelPanel = Instantiate (panelPrefab, slideContent);
			levelPanel.GetComponent<LevelPanel> ().SetLevel (lev);			
			Button levButton = levelPanel.GetComponent<Button> ();

			Navigation nav = levButton.navigation;
			nav.mode = Navigation.Mode.Explicit;

			if (prevButton != null) {
				nav.selectOnLeft = prevButton;
				nav.selectOnDown = backToMenuButton;

				if (levButton.IsInteractable()) {
					Navigation navPrev = prevButton.navigation;
					navPrev.mode = Navigation.Mode.Explicit;
					navPrev.selectOnRight = levButton;
					prevButton.navigation = navPrev;
				}

			} else {
				nav.selectOnDown = backToMenuButton;
				Navigation backNav = backToMenuButton.navigation;
				backNav.selectOnUp = levButton;
				backToMenuButton.navigation = backNav;
			}

			levButton.navigation = nav;

			prevButton = levButton;

		}


		yield return new WaitForEndOfFrame ();

		if (slideContent.childCount > 0) {			
			yield return new WaitForEndOfFrame ();
			EventSystem.current.SetSelectedGameObject (null);
			EventSystem.current.SetSelectedGameObject (slideContent.GetChild (0).gameObject);
		}
	}

}
