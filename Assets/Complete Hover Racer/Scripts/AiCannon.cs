using System.Collections;
using UnityEngine;

[RequireComponent (typeof (AiShip))]
public class AiCannon : MonoBehaviour, ICannon {

	[Range (1, 10)] public int aggressivity = 5;
	[Range (3, 10)] public int burstFire = 5;
	[Range (5, 15)] public int fireRate = 10;
	[Range (5, 30)] public int minFireRange = 20;
	[Range (100, 300)] public int maxFireRange = 200;
	[SerializeField] bool loaded;
	[SerializeField] int maxAmmo = 50;
	public LayerMask detectLayers;
	public LayerMask targetLayers;
	private bool canFire;
	private int ammo;
	private int shots;
	private int selfID;
	[SerializeField] private Transform firePoint;
	private Rigidbody rb;
	private AudioSource sfx;
	private AudioClip clip;
	private ParticleSystem gunParticles;

	private AiShip ship;


	void OnEnable () => RaceManager.OnRaceOver += EndRace;
	void OnDisable () => RaceManager.OnRaceOver -= EndRace;

	void EndRace () {
		StopAllCoroutines ();
		Destroy (this);
	}


	void Start () {
		ship = GetComponent<AiShip> ();
		rb = GetComponent<Rigidbody> ();
		sfx = firePoint.GetComponent<AudioSource> ();
		clip = sfx.clip;
		gunParticles = firePoint.GetComponent<ParticleSystem> ();
		selfID = gameObject.GetInstanceID ();
		if (loaded) ammo = maxAmmo;
	}


	public void AllowFire () {
		if (!canFire) {
			canFire = true;
			StartCoroutine (Aggressor ());
		}
	}

	public void Reload (int munition) {
		if (ship.hasLoadedWeapon) return;

		ammo = Mathf.Clamp (ammo += munition, 0, maxAmmo);
		loaded = true;
		ship.hasLoadedWeapon = true;
	}

	private void Shoot () {
		BulletPool.Instance.Get ().GetComponent<Bullet> ().Fire (firePoint, rb.velocity.magnitude, selfID);
		sfx.PlayOneShot (clip);
		gunParticles.Stop ();
		gunParticles.Play ();
		ammo--;
		if (loaded && ammo == 0) {
			loaded = false;
			ship.hasLoadedWeapon = false;
		}
	}


	IEnumerator Aggressor () {
		float loopTime = 11f - aggressivity;
		WaitForSeconds loop = new WaitForSeconds (loopTime);
		WaitForSeconds fireLoop = new WaitForSeconds (1f / fireRate * burstFire);

		while (true) {
			if (loaded && shots == 0 && Physics.SphereCast (firePoint.position, 10f, firePoint.forward, out RaycastHit hit, maxFireRange, detectLayers)) {
				if (targetLayers == (targetLayers | (1 << hit.transform.gameObject.layer))
					&& Vector3.Distance(firePoint.position, hit.point) >= minFireRange) {
					StopCoroutine ("FireCannon");
					shots = burstFire;
					StartCoroutine ("FireCannon");
					yield return fireLoop;
				}
			}
			yield return loop;
		}
	}

	IEnumerator FireCannon () {
		WaitForSeconds loop = new WaitForSeconds (1f / fireRate);
		while (shots > 0) {
			Shoot ();
			shots--;
			yield return loop;
		}		
	}

}
