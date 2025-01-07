using UnityEngine;
using System.Collections;

public class FPSDisplay : SingletonDontDestroy<FPSDisplay> {
	
	private float deltaTime = 0.0f;
	private float fps;
	private string text;
	private int h;
	private GUIStyle style;
	private Rect rect;


	public override void Awake () {
		base.Awake ();

		h = Screen.height;
		style = new GUIStyle();
		rect = new Rect(40, 240f, 200f, h);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h / 18;
		style.normal.textColor = Color.white;
	}

	void Update() {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

		// QIUT ON ESCAPE
		//if (Input.GetKey (KeyCode.Escape))
		//	QuitGame ();
	}

	void OnGUI() {		
		fps = 1.0f / deltaTime;
//		text = " Fps: " + Mathf.Round(fps) + " Cpu: " + SystemInfo.processorCount + "x " + SystemInfo.processorFrequency + "Mhz";
		text = Mathf.Round(fps).ToString();
		GUI.Label(rect, text, style);
	}

	// QIUT ON ESCAPE
	//public void QuitGame () {
	//	#if UNITY_EDITOR
	//	UnityEditor.EditorApplication.isPlaying = false;
	//	#else
	//	Application.Quit();
	//	#endif
	//}

}