using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonDontDestroy<AudioManager> {

	[Range (0.1f, 1f)] public float defaultMusicVol = 0.2f;
	[Range (0.1f, 1f)] public float defaultSFXVol = 1f;
	[Space (5)]
	public AudioMixer mixer;
	public AudioMixerSnapshot unpaused;
	public AudioMixerSnapshot paused;
	[SerializeField] private AudioSource musicPlayer;
	[SerializeField] private AudioSource sfxPlayer;
	private string isPlaying;

	void Start () {
		MusicVol (PlayerPrefs.GetFloat ("MusicVolume", defaultMusicVol));
		SFXVol (PlayerPrefs.GetFloat ("SFXVolume", defaultSFXVol));
	}

	public void PausedState () => paused.TransitionTo (1f);
	public void UnpausedState () => unpaused.TransitionTo (0.3f);

	public void MusicVol (float vol) {
		mixer.SetFloat ("MUSIC_VOL", Mathf.Log10 (Mathf.Clamp01 (vol)) * 20);
		PlayerPrefs.SetFloat ("MusicVolume", vol);
	}

	public void SFXVol (float vol) {
		mixer.SetFloat ("SFX_VOL", Mathf.Log10 (Mathf.Clamp01 (vol)) * 20);
		PlayerPrefs.SetFloat ("SFXVolume", vol);
	}

	public void PlayMusic (string musicName) {
		if (musicName == isPlaying) return;
		musicPlayer.clip = Resources.Load<AudioClip> ("music/" + musicName);
		musicPlayer.Play ();
		isPlaying = musicName;
	}

	public void StopMusic () {
		musicPlayer.Stop ();
		isPlaying = "";
	}

	public void PlaySFX (string sfxName) => sfxPlayer.PlayOneShot (Resources.Load<AudioClip> ("sfx/" + sfxName));

}
