using UnityEngine;
using UnityEngine.EventSystems;

public class Brake : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public static Brake instance = null;

	private bool _on = false;
	public bool On => _on;

	private float _volume = 0f;
	public float Volume => _volume;

	private float smooth = 3f;

	void Awake () {
		if (instance != null) Destroy (this.gameObject);
		else instance = this;
	}

	void Update () {
		if (_on && _volume < 1f)
			_volume += smooth * Time.deltaTime;
		else if (_volume > 0f)
			_volume -= smooth * Time.deltaTime;

		_volume = Mathf.Clamp01 (_volume);
	}

	public void OnPointerDown (PointerEventData data) => _on = true;
	public void OnPointerUp (PointerEventData data) => _on = false;

	void OnApplicationFocus (bool focusStatus) {
		if (!focusStatus) {
			_on = false;
			_volume = 0f;
		}
	}

}