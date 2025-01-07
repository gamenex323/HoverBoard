using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour {

	public Slider Music_Slider;
	public Slider SFX_Slider;

    void Start() {
		Music_Slider.value = PlayerPrefs.GetFloat ("MusicVolume", AudioManager.Instance.defaultMusicVol);
		SFX_Slider.value = PlayerPrefs.GetFloat ("SFXVolume", AudioManager.Instance.defaultSFXVol);
	}
	
}
