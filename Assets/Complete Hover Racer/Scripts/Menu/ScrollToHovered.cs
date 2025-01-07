using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollToHovered : MonoBehaviour {

	public ScrollRect scr;
	[HideInInspector] public float hoverPos;
	private float interPos;
	private float first;
	private float wide;


	private IEnumerator Start () {
		if (GameManager.Instance.BuildType == Build.MOBILE) Destroy (this);

		yield return new WaitForEndOfFrame ();

		first = transform.GetChild (0).localPosition.x;
		hoverPos = first;
		wide = transform.GetChild (transform.childCount - 1).localPosition.x - transform.GetChild (0).localPosition.x;
	}

	void Update () {
		interPos = (hoverPos - first) / wide;
		scr.horizontalNormalizedPosition = Mathf.SmoothStep (scr.horizontalNormalizedPosition, interPos, 15f * Time.deltaTime);
	}

}
