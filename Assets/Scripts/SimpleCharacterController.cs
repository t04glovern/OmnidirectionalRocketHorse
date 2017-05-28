using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
	[Header("Controls")]
	public string XAxis = "Horizontal";
	public string YAxis = "Vertical";
	public string JumpButton = "Jump";
	public string FartButton = "Fire1";
    public string SneezeButton = "Fire2";

	[Header("Moving")]
	public float walkSpeed = 4;
	public float runSpeed = 10;
	public float gravity = 65;

	[Header("Jumping")]
	public float jumpSpeed = 25;
	public float jumpDuration = 0.5f;
	public float jumpInterruptFactor = 100;

	[Header("Graphics")]
	public Transform graphicsRoot;
	public SkeletonAnimation skeletonAnimation;

	[Header("Animation")]
	[SpineAnimation(dataField: "skeletonAnimation")]
	public string walkName = "walking";
	[SpineAnimation(dataField: "skeletonAnimation")]
	public string runName = "running";
	[SpineAnimation(dataField: "skeletonAnimation")]
	public string idleName = "standing";
	[SpineAnimation(dataField: "skeletonAnimation")]
	public string jumpName = "jumping";
	[SpineAnimation(dataField: "skeletonAnimation")]
    public string fartingName = "farting";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string sneezingName = "sneezing";

    [Header("Audio")]
    public AudioClip walkAudioClip;
    public AudioClip jumpAudioClip;
    public AudioClip[] fartAudioClips;
    public AudioClip[] sneezeAudioClips;
    public AudioSource playerAudioSource;

    [Header("Events")]
	[SpineEvent]
	public string walkingEventName = "walking-step";
	[SpineEvent]
	public string runningEventName = "running-step";
	[SpineEvent]
    public string jumpLandEventName = "jump-landing";
	[SpineEvent]
	public string jumpPeakEventName = "jump-peak";
    [SpineEvent]
    public string fartingTootEventName = "farting-toot";
    [SpineEvent]
    public string sneezeEventName = "sneeze";

    [Header("Key Bones")]
    [SpineBone]
    public Spine.Bone buttBone;
    [SpineBone]
    public Spine.Bone headBone;

	CharacterController controller;
    FireProjectile fireProjectile;
    public ParticleSystem particleController;
    public CameraShake cameraShake;

	Vector2 velocity = Vector2.zero;
	Vector2 lastVelocity = Vector2.zero;
	bool lastGrounded = false;
	float jumpEndTime = 0;
	bool jumpInterrupt = false;
    bool facingRight = true;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
        fireProjectile = GetComponent<FireProjectile>();
	}

	void Start()
	{
		// Register a callback for Spine Events (in this case, Footstep)
		skeletonAnimation.state.Event += HandleEvent;
	}

	void HandleEvent(Spine.TrackEntry trackEntry, Spine.Event e)
	{
		// Play some sound if walking-step event fired
		if (e.Data.Name == walkingEventName)
		{
            playerAudioSource.Stop();
            playerAudioSource.clip = walkAudioClip;
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();
		}

		// Play some sound if running-step event fired
		if (e.Data.Name == runningEventName)
		{
            playerAudioSource.Stop();
            playerAudioSource.clip = walkAudioClip;
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();
		}
	}

	void Update()
	{
        buttBone = skeletonAnimation.skeleton.FindBone("Tail");
        headBone = skeletonAnimation.skeleton.FindBone("Head");

        Vector3 buttBoneTransform = transform.TransformPoint(
            new Vector3(buttBone.WorldX, buttBone.WorldY, 0));
        Vector3 headBoneTransform = transform.TransformPoint(
            new Vector3(headBone.WorldX, headBone.WorldY, 0));

        bool farting = false;
        bool sneezing = false;

		//control inputs
		float x = Input.GetAxis(XAxis);
		float y = Input.GetAxis(YAxis);

        if(Input.GetButton(FartButton))
        {
            farting = true;
        }
        if(Input.GetButton(SneezeButton))
        {
            sneezing = true;
        }

		velocity.x = 0;

		if (Input.GetButtonDown(JumpButton) && controller.isGrounded)
		{
			//jump
            playerAudioSource.Stop();
            playerAudioSource.clip = jumpAudioClip;
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();
			velocity.y = jumpSpeed;
			jumpEndTime = Time.time + jumpDuration;
		}
		else if (Time.time < jumpEndTime && Input.GetButtonUp(JumpButton))
		{
			jumpInterrupt = true;
		}


		if (x != 0)
		{
			//walk or run
			velocity.x = Mathf.Abs(x) > 0.6f ? runSpeed : walkSpeed;
			velocity.x *= Mathf.Sign(x);
		}

		if (jumpInterrupt)
		{
			//interrupt jump and smoothly cut Y velocity
			if (velocity.y > 0)
			{
				velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 100);
			}
			else
			{
				jumpInterrupt = false;
			}
		}

		//apply gravity F = mA (Learn it, love it, live it)
		velocity.y -= gravity * Time.deltaTime;

		//move
		controller.Move(new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime);

		if(controller.isGrounded)
		{
			//cancel out Y velocity if on ground
			velocity.y = -gravity * Time.deltaTime;
			jumpInterrupt = false;
		}

		//graphics updates
        // farting
        if(farting) 
        {
            // fart
            playerAudioSource.Stop();
            playerAudioSource.clip = GetRandomFart();
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();

            cameraShake.ShakeCamera(2f, 0.25f);

            skeletonAnimation.loop = true;
            skeletonAnimation.AnimationName = fartingName;
            particleController.Play();
        }
        // sneezing
        else if(sneezing)
        {
            // sneeze
            playerAudioSource.Stop();
            playerAudioSource.clip = GetRandomSneeze();
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();

            cameraShake.ShakeCamera(1f, 0.5f);

            skeletonAnimation.loop = false;
            skeletonAnimation.AnimationName = sneezingName;

            fireProjectile.Fire(facingRight, headBoneTransform);

            particleController.Stop();
        }
        // idle
        else 
        {
            if(controller.isGrounded)
            {
                skeletonAnimation.loop = true;
                if (x == 0) //idle
                {
                    skeletonAnimation.AnimationName = idleName;
                }
                else //move
                {
                    skeletonAnimation.AnimationName = Mathf.Abs(x) > 0.6f ? runName : walkName;
                }
            }
        }

        if (!controller.isGrounded)
		{
			skeletonAnimation.loop = false;
			if (velocity.y > 0) //jump
			{
				skeletonAnimation.AnimationName = jumpName;
			}
			else //fall
			{
				skeletonAnimation.AnimationName = jumpName;
			}
		}

		//flip left or right
        if (x > 0) 
        {
            facingRight = true;
            skeletonAnimation.skeleton.FlipX = false;
        } 
        else if (x < 0) 
        {
            facingRight = false;
            skeletonAnimation.skeleton.FlipX = true;
        }

		//store previous state
		lastVelocity = velocity;
		lastGrounded = controller.isGrounded;
	}

	static float GetRandomPitch(float maxOffset)
	{
		return 1f + Random.Range(-maxOffset, maxOffset);
	}

    AudioClip GetRandomFart()
    {
        return fartAudioClips[Random.Range(0,(fartAudioClips.Length - 1))];
    }

    AudioClip GetRandomSneeze()
    {
        return sneezeAudioClips[Random.Range(0,(sneezeAudioClips.Length - 1))];
    }
}
