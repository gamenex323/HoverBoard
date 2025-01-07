using UnityEngine;
using UnityEngine.UI;

public class OpenScene : MonoBehaviour {

	public Image OpeningImage;
	private Color imgCol;

	private void Awake () => Time.timeScale = 1f;

	private void Start () => imgCol = OpeningImage.color;

	private void Update () {
		if (imgCol.a > 0) {
			imgCol.a = Mathf.Clamp01 (imgCol.a -= 2f * Time.deltaTime);
			OpeningImage.color = imgCol; 
		} else Destroy (gameObject);
	}

}
