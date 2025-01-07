using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Collider), typeof (Rigidbody))]
public class Missile : MonoBehaviour {

	public LayerMask hitLayers;
	public Renderer missileMesh;

	[SerializeField] private int flySpeed = 250;    // 250 for AddForce & 5 for moving with rb.MovePosition
	[SerializeField] private int maxSpeed = 400;
	[SerializeField] private float flyTime = 4f;
	[SerializeField] private int radarRange = 60;

	public ParticleSystem impactFX;
	public TrailRenderer trailFX;
	public AudioSource impactSoundFX;

	public Transform radarPivot;

	private float hover;
	private float extraSpeed;
	private float FXduration;
	private float lifeTime = 0f;
	private float fDelta;
	private LayerMask roadLayer;
	private Vector3 projection;
	private Rigidbody rb;
	private int owner;	
	private Collider col;

	// Targeting
	private float radarRate = 0.1f;
	readonly Vector3 radarBox = new Vector3 (40f, 10f, 1f);
	private Transform target;


	void Awake () {
		roadLayer = RaceManager.Instance.roadLayer;
		hover = GameManager.Instance.hoverHeight;
		rb = GetComponent<Rigidbody> ();
		col = GetComponent<Collider> ();
		FXduration = Mathf.Max (impactFX.main.duration, impactSoundFX.clip.length);
		fDelta = Time.fixedDeltaTime;
	}


	public void Fire (Transform fPoint, float speed, int own) {
		transform.position = fPoint.position;
		transform.rotation = fPoint.rotation;
		rb.MoveRotation (Quaternion.LookRotation (transform.forward, transform.up));
		lifeTime = 0f;
		//extraSpeed = speed / 50f;
		owner = own;
		EnableMissile ();
		// Add starting speed
		extraSpeed = Mathf.Clamp (flySpeed + speed, flySpeed, maxSpeed);
		rb.AddForce (extraSpeed * transform.forward, ForceMode.VelocityChange);
	}


	private void EnableMissile () {
		col.enabled = true;
		missileMesh.enabled = true;
		trailFX.Clear ();
		trailFX.emitting = true;
		target = null;
		StartCoroutine ("TargetSearch");
	}

	private void DisableMissile () {
		col.enabled = false;
		missileMesh.enabled = false;
		trailFX.emitting = false;
		rb.velocity = Vector3.zero; // stop missile when moving by force
		rb.angularVelocity = Vector3.zero;
		StopCoroutine ("TargetSearch");
	}


	void FixedUpdate () {
		if (!col.enabled) return;

		// Missile try to follow racing track
		if (Physics.Raycast (transform.position, -transform.up, out RaycastHit hit, 16f, roadLayer)) {
			if (target == null)
				projection = Vector3.ProjectOnPlane (transform.forward, hit.normal);
			else {
				projection = Vector3.ProjectOnPlane ((target.position - transform.position).normalized, hit.normal);
				rb.velocity = (target.position - transform.position).normalized * rb.velocity.magnitude;
				//rb.velocity = Vector3.Lerp (rb.velocity, (target.position - transform.position).normalized * rb.velocity.magnitude, 0.5f);
			}

			rb.MoveRotation (Quaternion.LookRotation (projection, hit.normal));
			rb.MovePosition (transform.up * hover + hit.point);
		}

		// Moving forward
		//rb.AddForce (extraSpeed * transform.forward, ForceMode.VelocityChange);
		//rb.MovePosition (rb.position + transform.forward * (flySpeed + extraSpeed));

		// Check fly time
		lifeTime += fDelta;
		if (lifeTime >= flyTime) InstantReturn ();

	}   // FIXEDUPDATE END



	public IEnumerator TargetSearch () {
		WaitForSeconds rate = new WaitForSeconds (radarRate);
		while (true) {
			if (Physics.BoxCast (radarPivot.position, radarBox, transform.forward, out RaycastHit boxHit, Quaternion.LookRotation(radarPivot.forward, transform.up), radarRange, hitLayers)) {
				target = boxHit.transform;
				StopCoroutine ("TargetSearch");
			}
			
			yield return rate;
		}
	}




	private void OnTriggerEnter (Collider other) {
		if (other.transform.root.gameObject.GetInstanceID() == owner)   // Check cannon owner to avoid self hit
			return;
		else if (hitLayers == (hitLayers | (1 << other.gameObject.layer))) {
			if (other.TryGetComponent (out IShip ship)) {
				DisableMissile ();
				impactFX.Play ();
				impactSoundFX.Play ();
				StartCoroutine (ReturnPoolCort ());
				ship.MissileHit ();
			}
		}
	}


	private void OnCollisionEnter (Collision collision) {
		DisableMissile ();
		transform.position = collision.GetContact (0).point;
		transform.forward = -collision.GetContact (0).normal;
		impactFX.Play ();
		impactSoundFX.Play ();
		StartCoroutine (ReturnPoolCort ());
	}


	private IEnumerator ReturnPoolCort () {
		yield return new WaitForSeconds (FXduration);
		InstantReturn ();
	}

	private void InstantReturn () {
		DisableMissile ();
		StopAllCoroutines ();
		MissilePool.Instance.Return (gameObject);
	}

}
