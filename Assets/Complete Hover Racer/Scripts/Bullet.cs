using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Collider), typeof (Rigidbody))]
public class Bullet : MonoBehaviour {

	public LayerMask hitLayers;
	public Renderer bulletMesh;

	[SerializeField] private int flySpeed = 250;    // 250 for AddForce & 5 for moving with rb.MovePosition
	[SerializeField] private int maxSpeed = 400;
	[SerializeField] private float flyTime = 2f;

	public ParticleSystem impactFX;
	public AudioSource impactSoundFX;

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
		lifeTime = 0f;
		//extraSpeed = speed / 50f;
		owner = own;
		EnableBullet ();
		// Add starting speed
		extraSpeed = Mathf.Clamp (flySpeed + speed, flySpeed, maxSpeed);
		rb.AddForce (extraSpeed * transform.forward, ForceMode.VelocityChange);
	}


	//public void WallHit (ContactPoint contactP) {
	//	transform.position = contactP.point;
	//	transform.forward = contactP.normal;
	//	DisableBullet ();
	//	impactFX.Play ();
	//	impactSoundFX.Play ();
	//	StartCoroutine (ReturnPoolCort ());
	//}


	private void EnableBullet () {
		col.enabled = true;
		bulletMesh.enabled = true;
	}

	private void DisableBullet () {
		col.enabled = false;
		bulletMesh.enabled = false;
		rb.velocity = Vector3.zero; // stop bullet when moving by force
		rb.angularVelocity = Vector3.zero;
	}


	void FixedUpdate () {
		if (!col.enabled) return;

		// Bullet try to follow racing track
		if (Physics.Raycast (transform.position, -transform.up, out RaycastHit hit, 16f, roadLayer)) {
			projection = Vector3.ProjectOnPlane (transform.forward, hit.normal);
			rb.MoveRotation (Quaternion.LookRotation (projection, hit.normal));
			rb.MovePosition (transform.up * hover + hit.point);
		}

		// Moving forward with rigidbody
		//rb.MovePosition (rb.position + transform.forward * (flySpeed + extraSpeed));

		// Check fly time
		lifeTime += fDelta;
		if (lifeTime >= flyTime) InstantReturn ();

	}   // FIXEDUPDATE END


	private void OnTriggerEnter (Collider other) {
		if (other.transform.root.gameObject.GetInstanceID() == owner)   // Check cannon owner to avoid self hit
			return;
		else if (hitLayers == (hitLayers | (1 << other.gameObject.layer))) {
			if (other.TryGetComponent (out IShip ship)) {
				DisableBullet ();
				impactFX.Play ();
				impactSoundFX.Play ();
				StartCoroutine (ReturnPoolCort ());
				ship.CannonHit ();
			}
		}
	}


	private void OnCollisionEnter (Collision collision) {
		DisableBullet ();
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
		DisableBullet ();
		StopAllCoroutines ();
		BulletPool.Instance.Return (gameObject);
	}

}
