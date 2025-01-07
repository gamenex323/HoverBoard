using UnityEngine;

[RequireComponent (typeof (Collider), typeof (Rigidbody))]
public class AiShip : MonoBehaviour, IShip {
	// Setup
	[Header ("Collider Setup:")]
	[SerializeField] private bool usePrimitiveCollider = true;

	[Header ("Ship Parameters:")]
	[Range (0.005f, 0.02f)] public float acceleration = 0.01f;
	[Range (1400f, 1900f)] public float normalThrust = 1500f;
	[Range (1f, 4f)] public float turboTime = 2.5f;

	// Gravity & PID Hover
	private float fallGravity;

	private PIDController HoverPID;
	private bool UsePIDHover;
	private float hoverForce;
	private float hoverGravity;

	private float forcePercent;
	private Vector3 force;
	private Vector3 gravity;


	[Header ("Ship Tilt On Turn:")]
	[Range (0.2f, 0.4f)] public float maxTilt = 0.3f;
	private Quaternion tiltRot;
	private float tilt;
	private float tiltVec;
	private float tiltAngle;

	[Header ("Engine Sound Parameters:")]
	[SerializeField] private float minVolume = 0.1f;
	[SerializeField] private float maxVolume = 0.6f;
	[SerializeField] private float minPitch = 0.3f;
	[SerializeField] private float maxPitch = 0.8f;
	private float sfxLerp;
	private AudioSource engineAudio;

	// HOVERING
	private float hover;
	private float hoverMax;
	private float hoverFactor;
	private float bendFactor;
	private float scanLength;
	private LayerMask roadLayer;
	private float fakeGrav;
	private Vector3 projection;
	private Quaternion rotateToRoad;
	private RaycastHit hit;

	[Header ("Engine Particle Parameters:")]
	[SerializeField] private float minParticleSpeed = 4f;
	[SerializeField] private float maxParticleSpeed = 16f;

	[Header ("Ship Objects:")]
	public Transform shipBody;
	public Transform scanner;
	public Transform wayPoint;
	public ParticleSystem afterBurner;
	private ParticleSystem.MainModule module;
	[HideInInspector] public bool inRace;
	private float thrust;
	private float turboThrust;
	private float thrustForce;
	private float accel;
	private float turboAcceleration = 0.1f;
	private float startCheat;
	private float raceSpeed;

	private Rigidbody rb;

	[HideInInspector] public bool hasLoadedWeapon;	// Ennsuring only one weapon can be loaded at a time
	

	void Awake () {
		// Get RigidBody
		rb = GetComponent<Rigidbody> ();
		// Set Sound
		engineAudio = GetComponent<AudioSource> ();
		// Set Particle Module
		module = afterBurner.main;
	}

	void OnEnable () => RaceManager.OnRaceStart += StartRace;
	void OnDisable () => RaceManager.OnRaceStart -= StartRace;
	private void StartRace () => inRace = true;


	void Start () {
		SetColliders ();

		thrust = normalThrust;
		turboThrust = normalThrust * 1.3f;
		accel = acceleration;

		if (inRace) thrustForce = thrust;   // Skip Acceleration when switching from Manual to AI drive

		// HOVERING
		roadLayer = RaceManager.Instance.roadLayer;
		hover = GameManager.Instance.hoverHeight;
		hoverMax = hover * 1.4f;

		raceSpeed = GameManager.Instance.raceSpeed;
		hoverFactor = Mathf.Lerp (0.3f, 0.6f, Mathf.InverseLerp (1f, 1.5f, raceSpeed));
		bendFactor = Mathf.Lerp (0.2f, 0.4f, Mathf.InverseLerp (1f, 1.5f, raceSpeed));
		scanLength = Mathf.Ceil (hoverMax / Mathf.Cos (Mathf.Deg2Rad * (90f - scanner.localEulerAngles.x))) + 2;

		// Gravity & PID Hover Setup
		fallGravity = GameManager.Instance.fallGravity;
		UsePIDHover = GameManager.Instance.HoverMode == HoverType.PID_CONTROLLED_ADDFORCE ? true : false;
		if (UsePIDHover) {
			HoverPID = new PIDController ();
			hoverForce = GameManager.Instance.PIDHoverForce * raceSpeed;
			hoverGravity = GameManager.Instance.hoverGravity;
		}
	}


	void Update () {
		// Engine Sound & VFX
		sfxLerp = thrustForce / turboThrust;
		engineAudio.volume = Mathf.Lerp (minVolume, maxVolume, sfxLerp);
		engineAudio.pitch = Mathf.Lerp (minPitch, maxPitch, sfxLerp);
		module.startSpeed = Mathf.Lerp (minParticleSpeed, maxParticleSpeed, sfxLerp);
	}


	void FixedUpdate () {
		// HOVERING
		if (UsePIDHover) PIDHover ();
		else RbMoveHover ();

		if (!inRace) return;

		// THRUST
		thrustForce = Mathf.Lerp (thrustForce, thrust + startCheat, accel);
		rb.AddForce (raceSpeed * thrustForce * shipBody.forward, ForceMode.Acceleration);

		// TILT
		tiltAngle = tiltVec - transform.eulerAngles.y;
		tiltRot = shipBody.localRotation;
		if (Mathf.Abs(tiltAngle) >= 0.3f) {
			if (tiltAngle > 0)
				tilt = Mathf.Lerp (tilt, maxTilt, 0.1f);
			else if (tiltAngle < 0)
				tilt = Mathf.Lerp (tilt, -maxTilt, 0.1f);
		} else tilt = Mathf.Lerp (tilt, 0, 0.2f);

		tiltRot.z = tilt;

		shipBody.localRotation = Quaternion.Slerp (shipBody.localRotation, tiltRot, 0.03f);

		tiltVec = transform.eulerAngles.y;

	} // END of FIXED UPDATE


	// PID HOVER
	private void PIDHover () {
		if (Physics.Raycast (transform.position, -transform.up, out RaycastHit hit, hoverMax, roadLayer)) {
			// TURN WITH PRE-SCAN
			if (Physics.Raycast (scanner.position, scanner.forward, out RaycastHit fHit, scanLength, roadLayer)) {
				projection = Vector3.ProjectOnPlane ((wayPoint.position - transform.position).normalized, fHit.normal);
				rotateToRoad = Quaternion.LookRotation (projection, fHit.normal);
			} else {
				projection = Vector3.ProjectOnPlane ((wayPoint.position - transform.position).normalized, hit.normal);
				rotateToRoad = Quaternion.LookRotation (projection, hit.normal);				
			}

			rb.MoveRotation (Quaternion.Lerp (rb.rotation, rotateToRoad, bendFactor));

			// Hover with force
			forcePercent = HoverPID.Seek (hover, hit.distance);

			force = hit.normal * hoverForce * forcePercent;
			gravity = -hit.normal * hoverGravity * hit.distance;

			rb.AddForce (force, ForceMode.Acceleration);
			rb.AddForce (gravity, ForceMode.Acceleration);

		} else GravitySim ();
	}

	// RIGIDBODY MOVEPOSITION HOVER
	private void RbMoveHover () {
		if (Physics.Raycast (transform.position, -transform.up, out RaycastHit hit, hoverMax, roadLayer)) {
			//Forward scan
			if (Physics.Raycast (scanner.position, scanner.forward, out RaycastHit fHit, scanLength, roadLayer)) {
				projection = Vector3.ProjectOnPlane ((wayPoint.position - transform.position).normalized, fHit.normal);
				rotateToRoad = Quaternion.LookRotation (projection, fHit.normal);				
			} else {
				projection = Vector3.ProjectOnPlane ((wayPoint.position - transform.position).normalized, hit.normal);
				rotateToRoad = Quaternion.LookRotation (projection, hit.normal);
			}

			rb.MoveRotation (Quaternion.Lerp (rb.rotation, rotateToRoad, bendFactor));
			rb.MovePosition (Vector3.Lerp (rb.position, transform.up * hover + hit.point, hoverFactor));

		} else GravitySim ();
	}

	private void GravitySim () {
		fakeGrav = Vector3.Dot (transform.forward, Vector3.up);
		fakeGrav = fakeGrav > 0f ? fakeGrav : 0f;
		rb.AddForce (Vector3.down * ((fallGravity * fakeGrav) + fallGravity), ForceMode.Acceleration);
		rb.MoveRotation (Quaternion.Slerp (rb.rotation, Quaternion.LookRotation (rb.velocity.normalized), 0.1f));
	}


	// SET COLLIDERS
	private void SetColliders () {
		MeshCollider[] mc = GetComponentsInChildren<MeshCollider> ();

		if (usePrimitiveCollider) {
			GetComponent<Collider> ().enabled = true;
			foreach (MeshCollider item in mc) Destroy (item);
		} else {
			if (mc != null) {
				Destroy (GetComponent<Collider> ());
				foreach (MeshCollider item in mc) item.enabled = true;
			} else {
				GetComponent<Collider> ().enabled = true;
				foreach (MeshCollider item in mc) Destroy (item);
			}
		}

	}


	// TURBO Operations
	public void Turbo_On () {
		thrust = turboThrust;
		accel = turboAcceleration;
		CancelInvoke ("Turbo_Off");
		Invoke ("Turbo_Off", turboTime);
	}

	public void Turbo_Off () {
		thrust = normalThrust;
		accel = acceleration;
	}

	public void WallHit (Vector3 hitPosition, float reduceThrust) => thrustForce *= reduceThrust;

	public void CannonHit () {
		if (thrustForce > normalThrust / 2f) thrustForce = normalThrust / 2f;
	}

	public void MissileHit () {
		accel = 0;
		thrustForce = 0;
		rb.velocity = Vector3.zero;
		// Reset status with help of Turbo_Off as wee need same setup
		CancelInvoke ("Turbo_Off");
		Invoke ("Turbo_Off", 1f);
	}

	// CHEAT Operations
	public void Cheat_On (float cheatSpeed, int cheatDuration) {
		startCheat = normalThrust * cheatSpeed;
		CancelInvoke ("Cheat_Off");
		Invoke ("Cheat_Off", cheatDuration);
	}
	public void Cheat_Off () => startCheat = 0;

}
