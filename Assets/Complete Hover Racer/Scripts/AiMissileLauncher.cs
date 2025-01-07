using System.Collections;
using UnityEngine;

[RequireComponent (typeof (AiShip))]
public class AiMissileLauncher : MonoBehaviour, IMissileLauncher {

	[Range (1, 10)] public int aggressivity = 5;
	[Range (20, 50)] public int minFireRange = 30;
	[Range (150, 300)] public int maxFireRange = 260;
	public LayerMask detectLayers;
	public LayerMask targetLayers;
	private bool loaded;
	private bool canFire;
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
	}


	public void AllowFire () {
		canFire = true;
		if (loaded)  StartCoroutine (Aggressor ());		
	}

	public void Reload () {
		if (ship.hasLoadedWeapon) return;

		loaded = true;
		ship.hasLoadedWeapon = true;
		if (canFire) StartCoroutine (Aggressor ());
	}

	private void Shoot () {
		MissilePool.Instance.Get ().GetComponent<Missile> ().Fire (firePoint, rb.velocity.magnitude, selfID);
		sfx.PlayOneShot (clip);
		gunParticles.Stop ();
		gunParticles.Play ();
		loaded = false;
		ship.hasLoadedWeapon = false;
		StopAllCoroutines ();
	}


	IEnumerator Aggressor () {
		float loopTime = 11f - aggressivity;
		WaitForSeconds loop = new WaitForSeconds (loopTime);

		while (loaded) {
			if (Physics.SphereCast (firePoint.position, 20f, firePoint.forward, out RaycastHit hit, maxFireRange, detectLayers)) {
				if (targetLayers == (targetLayers | (1 << hit.transform.gameObject.layer))
					&& Vector3.Distance(firePoint.position, hit.point) >= minFireRange) Shoot ();
			}
			yield return loop;
		}
	}

}
