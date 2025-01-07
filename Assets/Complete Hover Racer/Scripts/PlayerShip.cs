using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof (Collider), typeof (Rigidbody))]
public class PlayerShip : MonoBehaviour, IShip {
	// Setup
	[Header ("Collider Setup:")]
	[SerializeField] private bool usePrimitiveCollider = true;

	[Header ("Ship Parameters:")]
	[HideInInspector] public float newInputTurn;
	[HideInInspector] public float newInputThrust;
	[HideInInspector] public float newInputBrake;

	[HideInInspector] public float turn;
	[HideInInspector] public float drift;
	[HideInInspector] public float acceleration;
	[HideInInspector] public float normalThrust;
	[HideInInspector] public float turboThrust;
	[HideInInspector] public float turboTime;
	private float brakePower;


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
	[Range (0.2f, 0.4f)] public float maxTilt = 0.2f;

	[Header ("Engine Sound Parameters:")]
	[SerializeField] private float minVolume = 0.1f;
	[SerializeField] private float maxVolume = 0.6f;
	[SerializeField] private float minPitch = 0.3f;
	[SerializeField] private float maxPitch = 0.8f;
	private float sfxLerp;
	private AudioSource engineAudio;

	[Header ("Engine Particle Parameters:")]
	[SerializeField] private float minParticleSpeed = 4f;
	[SerializeField] private float maxParticleSpeed = 16f;

	// HOVERING
	private float hover, hoverMax, hoverFactor, bendFactor, scanLength, fakeGrav;
	private LayerMask roadLayer;
	private Vector3 projection;
	private Quaternion rotateToRoad;

	[Header ("Ship Objects:")]
	public Transform shipBody;
	public Transform scanner;
	public ParticleSystem afterBurner;
	private ParticleSystem.MainModule module;
	private bool inRace;
	private bool mobile;
	private float turnInput;
	private float thrust;
	private float manualThrust;
	private float thrustForce;
	private float raceSpeed;
	private bool autoSpeed;
	private bool rebound;
	private float accel;
	private float turboAcceleration = 0.1f;
	Vector3 smoothVector;
	Vector3 vectorVelocity;
	private Rigidbody rb;
	private Quaternion tiltRot;

	private RacerCamera rCam;


	void Awake () {
		// Get RigidBody
		rb = GetComponent<Rigidbody> ();

#if UNITY_6000_0_OR_NEWER
		rb.automaticInertiaTensor = false;
		rb.freezeRotation = false;
		rb.inertiaTensor = new Vector3 (9999, 1, 9999);
#endif

		// Set Sound
		engineAudio = GetComponent<AudioSource> ();
		// Set Particle Module
		module = afterBurner.main;
		// Check in is mobile game
		if (GameManager.Instance.BuildType == Build.MOBILE) mobile = true;
	}


	void OnEnable () {
		RaceManager.OnRaceStart += StartRace;
		RaceManager.OnRaceOver += AutoPilot;
	}
	void OnDisable () {
		RaceManager.OnRaceStart -= StartRace;
		RaceManager.OnRaceOver -= AutoPilot;
	}

	private void StartRace () => inRace = true;


	void Start () {
		SetColliders ();

		autoSpeed = GameManager.Instance.useAutoThrust;
		thrust = normalThrust;
		accel = acceleration;
		rebound = GameManager.Instance.selectedLevel.sideWallRebound;
		roadLayer = RaceManager.Instance.roadLayer;
		
		if (RaceManager.Instance.levelSO.useLocalSetup) {
			raceSpeed = RaceManager.Instance.levelSO.raceSpeed;
			hover = RaceManager.Instance.levelSO.hoverHeight;
			hoverFactor = RaceManager.Instance.levelSO.hoverFactor;
			bendFactor = RaceManager.Instance.levelSO.bendFactor;
			UsePIDHover = RaceManager.Instance.levelSO.HoverMode == HoverType.PID_CONTROLLED_ADDFORCE ? true : false;
			fallGravity = RaceManager.Instance.levelSO.fallGravity;
			if (UsePIDHover) {
				hoverForce = RaceManager.Instance.levelSO.PIDHoverForce * raceSpeed;
				hoverGravity = RaceManager.Instance.levelSO.hoverGravity;
			}
		} else {
			raceSpeed = GameManager.Instance.raceSpeed;
			hover = GameManager.Instance.hoverHeight;
			hoverFactor = GameManager.Instance.hoverFactor;
			bendFactor = GameManager.Instance.bendFactor;
			UsePIDHover = GameManager.Instance.HoverMode == HoverType.PID_CONTROLLED_ADDFORCE ? true : false;
			fallGravity = GameManager.Instance.fallGravity;
			if (UsePIDHover) {
				hoverForce = GameManager.Instance.PIDHoverForce * raceSpeed;
				hoverGravity = GameManager.Instance.hoverGravity;
			}
		}

		hoverMax = hover * 1.4f;		
		brakePower = GameManager.Instance.brakePower;
		scanLength = Mathf.Ceil (hoverMax / Mathf.Cos (Mathf.Deg2Rad * (90f - scanner.localEulerAngles.x))) + 2;

		if (UsePIDHover) {
			HoverPID = new PIDController ();
			HoverPID.pCoeff *= raceSpeed;
			HoverPID.iCoeff *= raceSpeed;
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
		if (Physics.Raycast (transform.position, -transform.up, out RaycastHit hit, hoverMax, roadLayer)) {
			BendToRoad (hit.normal);
			if (UsePIDHover) PIDHover (hit);
			else rb.MovePosition (Vector3.Lerp (rb.position, transform.up * hover + hit.point, hoverFactor));
		} else GravitySim ();

		// Continue Fixedupdate Only When Race Is On
		if (!inRace) return;
		// TURN & TILT PLAYER
		TurnAndTilt ();
		// THRUST
		Thrust ();
	} // END of FIXED UPDATE


	// PID HOVER
	private void PIDHover (RaycastHit hit) {
		forcePercent = HoverPID.Seek (hover + 1, hit.distance + 1);
		force = hit.normal * hoverForce * forcePercent;
		gravity = -hit.normal * hoverGravity * hit.distance;
		rb.AddForce (gravity + force, ForceMode.Acceleration);	
	}

	// Rotate Ship Via Road Normal
	private void BendToRoad (Vector3 roadNormal) {
		if (Physics.Raycast (scanner.position, scanner.forward, out RaycastHit fHit, scanLength, roadLayer)) roadNormal = fHit.normal;
		projection = Vector3.ProjectOnPlane (transform.forward, roadNormal);
		rotateToRoad = Quaternion.LookRotation (projection, roadNormal);
		rb.MoveRotation (Quaternion.Lerp (rb.rotation, rotateToRoad, bendFactor));
	}


	private void GravitySim () {
		fakeGrav = Vector3.Dot (transform.forward, Vector3.up);
		fakeGrav = fakeGrav > 0f ? fakeGrav : 0f;
		rb.AddForce (Vector3.down * ((fallGravity * fakeGrav) + fallGravity), ForceMode.Acceleration);
		rb.MoveRotation (Quaternion.Slerp (rb.rotation, Quaternion.LookRotation (rb.velocity.normalized), 0.1f));
	}


	private void TurnAndTilt () {
		if (mobile) {
#if UNITY_EDITOR
			turnInput = LeftTurn.instance.Volume + RightTurn.instance.Volume + newInputTurn;
#else
			turnInput = LeftTurn.instance.Volume + RightTurn.instance.Volume + newInputTurn;
#endif
		} else turnInput = newInputTurn;

		rb.AddTorque (transform.up * turn * turnInput, ForceMode.Acceleration);
		tiltRot.Set (0f, 0f, -maxTilt * turnInput, 1f);
		shipBody.localRotation = Quaternion.Lerp (shipBody.localRotation, tiltRot, 0.2f);
	}


	private void Thrust () {		
		if (autoSpeed) {

			if (mobile) {
#if UNITY_EDITOR
				if (Brake.instance.On || newInputBrake < 0)
					thrustForce = Mathf.Lerp (thrustForce, 0, brakePower);
				else
					thrustForce = Mathf.Lerp (thrustForce, thrust, accel);
#else
				if (Brake.instance.On  || newInputBrake < 0)
					thrustForce = Mathf.Lerp (thrustForce, 0, brakePower);
				else
					thrustForce = Mathf.Lerp (thrustForce, thrust, accel);
#endif
			} else if (newInputBrake < 0)
				thrustForce = Mathf.Lerp (thrustForce, 0, brakePower);
			else thrustForce = Mathf.Lerp (thrustForce, thrust, accel);

		} else {    // IF NOT AUTO ACCELERATION

			manualThrust = thrust * (newInputThrust - newInputBrake);
			if (manualThrust < 0) {
				thrustForce = Mathf.Lerp (thrustForce, 0, brakePower);
			} else if (manualThrust > 0) {
				thrustForce = Mathf.Lerp (thrustForce, manualThrust, accel);
			} else {
				thrustForce = Mathf.Lerp (thrustForce, 0, 0.005f);
			}

		}

		// Add ThrustForce
		smoothVector = Vector3.SmoothDamp (smoothVector, transform.forward, ref vectorVelocity, drift);
		rb.AddForce (raceSpeed * thrustForce * smoothVector, ForceMode.Acceleration);
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

		if (rCam != null) {
			rCam.SetTurboFOV ();
		} else {
			rCam = RaceManager.Instance.raceCam.GetComponent<RacerCamera> ();
			rCam.SetTurboFOV ();
		}

	}

	public void Turbo_Off () {
		thrust = normalThrust;
		accel = acceleration;
		rCam.SetNormalFOV ();
	}

	public void WallHit (Vector3 hitNormal, float reduceThrust) {
		thrustForce *= reduceThrust;

		if (!rebound) return;

		// Bounce from wall
		rb.AddForce (5 * -hitNormal, ForceMode.VelocityChange);
		// Turn away from wall
		if (Vector3.Dot (hitNormal, rb.velocity.normalized) > 0)
			rb.MoveRotation (Quaternion.Slerp (rb.rotation, Quaternion.LookRotation (-hitNormal, transform.up), 0.01f));
	}

	public void CannonHit () {
		if (thrustForce > normalThrust / 2f) thrustForce = normalThrust / 2f;
	}

	public void MissileHit () {
		thrustForce = 0;
		rb.velocity = Vector3.zero;
	}

	// Autopilot when race ends
	public void AutoPilot () {
		inRace = false;

		AiShip autoDrive = GetComponent<AiShip> ();
		autoDrive.enabled = true;
		autoDrive.normalThrust = normalThrust;
		autoDrive.acceleration = acceleration;
		autoDrive.inRace = true;

		WaypointProgressTracker tracker = GetComponent<WaypointProgressTracker> ();
		tracker.enabled = true;
		tracker.circuit = RaceManager.Instance.circuit;
		tracker.StartTrack ();

		enabled = false;
	}

}
