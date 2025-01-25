using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour, ICannon {

	public string displayName = "CANNON";
	[SerializeField] bool loaded;
	[SerializeField] int maxAmmo = 50;
	[Range (5, 15)] public int fireRate = 10;
	[HideInInspector] public bool newInputFire;
	[SerializeField] private Transform firePoint;
	private bool canFire;
	private int ammo;
	private int selfID;
	private Rigidbody rb;
	private AudioSource sfx;
	private AudioClip clip;
	private ParticleSystem gunParticles;


	void OnEnable () => RaceManager.OnRaceOver += EndRace;
	void OnDisable () => RaceManager.OnRaceOver -= EndRace;

	void EndRace () {
		StopAllCoroutines ();
		Destroy (this);
	}


	private void Start () {
		maxAmmo = 10000000;
        GetComponentInParent<ICannon>()?.Reload(GlobalData.Ammo);
        rb = GetComponent<Rigidbody> ();
		sfx = firePoint.GetComponent<AudioSource> ();
		clip = sfx.clip;
		gunParticles = firePoint.GetComponent<ParticleSystem> ();
		if (loaded) {
			ammo = maxAmmo;
			RaceManager.Instance.ShowAmmo (ammo.ToString ());
			RaceManager.Instance.ShowAmmoType ("CANNON");
		}
		selfID = gameObject.GetInstanceID ();
	}


	public void AllowFire () {
		if (!canFire) {
			canFire = true;
			if (ammo != 0) StartCoroutine (FireCannon ());
		}
	}

	public void Reload (int munition) {
		if (!RaceManager.Instance.CanReload (displayName)) return;

		StopAllCoroutines ();
		ammo = Mathf.Clamp (ammo += munition, 0, maxAmmo);
		RaceManager.Instance.ShowAmmo (ammo.ToString ());
		RaceManager.Instance.ShowAmmoType (displayName);
		StartCoroutine (FireCannon ());
		AudioManager.Instance.PlaySFX ("reload");
	}

	private void Shoot () {
		BulletPool.Instance.Get ().GetComponent<Bullet> ().Fire (firePoint, rb.velocity.magnitude, selfID);
		sfx.PlayOneShot (clip);
		gunParticles.Stop ();
		gunParticles.Play ();
		ammo--;
		RaceManager.Instance.ShowAmmo (ammo.ToString ());
	}

	IEnumerator FireCannon () {
		bool mobile = GameManager.Instance.BuildType == Build.MOBILE ? true : false;
		if (mobile) Fire.instance.Show ();

		WaitForSeconds loop = new WaitForSeconds (1f / fireRate);
		while (true) {
			if (ammo > 0) {
				if (mobile) {
#if UNITY_EDITOR
					if (Fire.instance.On || newInputFire) Shoot ();
#else
					if (Fire.instance.On || newInputFire) Shoot ();
#endif
				} else if (newInputFire) Shoot ();
			} else {
				if (mobile) Fire.instance.Hide ();
				StopAllCoroutines ();
			}

			yield return loop;
		}
	}

}
