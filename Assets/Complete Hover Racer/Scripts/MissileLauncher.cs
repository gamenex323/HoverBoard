using UnityEngine;

public class MissileLauncher : MonoBehaviour, IMissileLauncher {

	public string displayName = "MISSILE";
	[HideInInspector] public bool newInputFire;
	[SerializeField] private Transform firePoint;
	private bool canFire;
	private bool loaded;
	private int selfID;
	private Rigidbody rb;
	private AudioSource sfx;
	private AudioClip clip;
	private ParticleSystem gunParticles;
	private bool mobile;


	void OnEnable () => RaceManager.OnRaceOver += EndRace;
	void OnDisable () => RaceManager.OnRaceOver -= EndRace;

	void EndRace () => Destroy (this);


	private void Start () {
		rb = GetComponent<Rigidbody> ();
		sfx = firePoint.GetComponent<AudioSource> ();
		clip = sfx.clip;
		gunParticles = firePoint.GetComponent<ParticleSystem> ();
		selfID = gameObject.GetInstanceID ();
		mobile = GameManager.Instance.BuildType == Build.MOBILE ? true : false;
	}


	public void AllowFire () => canFire = true;

	public void Reload () {
		if (loaded || !RaceManager.Instance.CanReload (displayName)) return;

		loaded = true;
		RaceManager.Instance.ShowAmmo ("LOADED");
		RaceManager.Instance.ShowAmmoType (displayName);
		if (mobile) Fire.instance.Show ();
		AudioManager.Instance.PlaySFX ("reload");
	}

	private void Shoot () {
		MissilePool.Instance.Get ().GetComponent<Missile> ().Fire (firePoint, rb.velocity.magnitude, selfID);
		sfx.PlayOneShot (clip);
		gunParticles.Stop ();
		gunParticles.Play ();
		loaded = false;
		RaceManager.Instance.ShowAmmo ("");
	}


	private void Update () {
		if (!canFire || !loaded) return;

		if (mobile) {
#if UNITY_EDITOR
			if (newInputFire) Shoot ();
#else
			if (Fire.instance.On || newInputFire) Shoot ();
#endif
		} else if (newInputFire) Shoot ();
	}

}
