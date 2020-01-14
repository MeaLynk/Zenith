using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyController : MonoBehaviour
{
    [Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 8.0f;   // Speed when walking forward
        public float BackwardSpeed = 4.0f;  // Speed when walking backwards
        public float StrafeSpeed = 4.0f;    // Speed when walking sideways
        public float RunMultiplier = 2.0f;   // Speed when sprinting
        public KeyCode RunKey = KeyCode.LeftShift;
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector] public float CurrentTargetSpeed = 8f;

        private bool Run;

        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
            if (input == Vector2.zero) return;
            if (input.x > 0 || input.x < 0)
            {
                //strafe
                CurrentTargetSpeed = StrafeSpeed;
            }
            if (input.y < 0)
            {
                //backwards
                CurrentTargetSpeed = BackwardSpeed;
            }
            if (input.y > 0)
            {
                //forwards
                //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                CurrentTargetSpeed = ForwardSpeed;
            }
            if (Input.GetKey(RunKey))
            {
                CurrentTargetSpeed *= RunMultiplier;
                Run = true;
            }
            else
            {
                Run = false;
            }
        }
        public bool Running
        {
            get { return Run; }
        }
    }

    [Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
        public bool airControl; // can the user control the direction that is being moved in the air
        [Tooltip("set it to 0.1 or more if you get stuck in wall")]
        public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
    }

    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLookScript mouseLook = new MouseLookScript();
    public AdvancedSettings advancedSettings = new AdvancedSettings();


    private Rigidbody CharBody;
    private CapsuleCollider CharCapsule;
    private Vector3 GroundContactNormal;
    private bool Jump, PreviouslyGrounded, Jumping, IsGrounded;

    public Vector3 Velocity
    {
        get { return CharBody.velocity; }
    }

    public bool Grounded
    {
        get { return IsGrounded; }
    }

    public bool Running
    {
        get { return movementSettings.Running; }
    }

    private void Start()
    {
        CharBody = GetComponent<Rigidbody>();
        CharCapsule = GetComponent<CapsuleCollider>();
        mouseLook.Init(transform, cam.transform);
    }

    private void Update()
    {
        RotateView();

        if (Input.GetButtonDown("Jump") && !Jump)
        {
            Jump = true;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Vector2 input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || IsGrounded))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;

            if (CharBody.velocity.sqrMagnitude < (Math.Pow(movementSettings.CurrentTargetSpeed, 2)))
            {
                CharBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse); // add force in the desired direction 
            }
        }

        if (IsGrounded)
        {
            CharBody.drag = 5f;

            if (Jump)
            {
                CharBody.drag = 0f;
                CharBody.velocity = new Vector3(CharBody.velocity.x, 0f, CharBody.velocity.z);
                CharBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                Jumping = true;
            }

            if (!Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && CharBody.velocity.magnitude < 1f)
            {
                CharBody.Sleep();
            }
        }
        else
        {
            CharBody.drag = 0f;
            if (PreviouslyGrounded && !Jumping)
            {
                StickToGroundHelper();
            }
        }
        Jump = false;
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(GroundContactNormal, Vector3.up); //Gets the angle of the ground
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }

    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, CharCapsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                               ((CharCapsule.height / 2f) - CharCapsule.radius) +
                               advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                CharBody.velocity = Vector3.ProjectOnPlane(CharBody.velocity, hitInfo.normal);
            }
        }
    }

    private Vector2 GetInput()
    {

        Vector2 input = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }

    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        mouseLook.LookRotation(transform, cam.transform);

        if (IsGrounded || advancedSettings.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            CharBody.velocity = velRotation * CharBody.velocity;
        }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        PreviouslyGrounded = IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, CharCapsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                               ((CharCapsule.height / 2f) - CharCapsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            IsGrounded = true;
            GroundContactNormal = hitInfo.normal;
        }
        else
        {
            IsGrounded = false;
            GroundContactNormal = Vector3.up;
        }
        if (!PreviouslyGrounded && IsGrounded && Jumping)
        {
            Jumping = false;
        }
    }
}