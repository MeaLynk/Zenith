using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public Members
    [Header("Camera: ", order = 0)]
    public Camera camera;
    public float cameraSensitivity = 300.0f;
    public float cameraClampAngle = 80.0f;

    [Header("Movement: ", order = 1)]
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float jumpSpeed = 10.0f;
    public float stickToGroundForce = 10.0f;
    public float gravityMultiplier = 2.0f;
    public bool useFovKick = true;
    public AnimationCurve FOVIncreaseCurve;
    public bool useHeadBob = true;
    public float runstepLengthen = 0.7f;
    public float stepInterval = 5.0f;

    [Header("Audio: ", order = 2)]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public int PlayerNumber { get; set; }
    #endregion

    #region Private Members
    private bool isWalking;
    private bool jump;
    private bool jumping;
    private bool previouslyGrounded;
    private float stepCycle;
    private float nextStep;
    private float rotationX;
    private float rotationY;
    private Vector2 userInput;
    private Vector3 moveDirection;
    private Vector3 originalCameraPosition;
    private CollisionFlags collisionFlags;
    private CharacterController characterController;
    private AudioSource audioSource;
    [SerializeField] private FOVKick fovKick;
    private CurveControlledBob headBob;
    private LerpControlledBob jumpBob;
    #endregion

    // Use this for initialization
    void Awake()
    {
        isWalking = true;
        jump = false;
        jumping = false;
        previouslyGrounded = true;
        stepCycle = 0.0f;
        nextStep = stepCycle / 2.0f;
        rotationX = camera.transform.localRotation.eulerAngles.x;
        rotationY = this.transform.localRotation.eulerAngles.y;
        userInput = Vector2.zero;
        moveDirection = Vector3.zero;
        originalCameraPosition = camera.transform.localPosition;
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        fovKick = new FOVKick();
        fovKick.IncreaseCurve = FOVIncreaseCurve;
        fovKick.Setup(camera, 3.0f, 1.0f, 1.0f);
        headBob = new CurveControlledBob();
        headBob.Setup(camera, stepInterval, 0.1f, 0.1f, 2.0f);
        jumpBob = new LerpControlledBob
        {
            BobDuration = 0.2f,
            BobAmount = 0.1f
        };
        PlayerNumber = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (!jump && PlayerNumber == 1)
            jump = Input.GetButtonDown("PlayerOne_Jump");
        if (!jump && PlayerNumber == 2)
            jump = Input.GetButtonDown("PlayerTwo_Jump");

        if (!previouslyGrounded && characterController.isGrounded)
        {
            StartCoroutine(jumpBob.DoBobCycle());
            PlayLandingSound();
            moveDirection.y = 0.0f;
            jumping = false;
        }
        if (!characterController.isGrounded && !jumping && previouslyGrounded)
        {
            moveDirection.y = 0.0f;
        }

        previouslyGrounded = characterController.isGrounded;

    }

    void PlayLandingSound()
    {
        audioSource.clip = landSound;
        audioSource.Play();
        nextStep = stepCycle + 0.5f;
    }

    void FixedUpdate()
    {
        GetInput(out float speed);
        Vector3 desiredMove = transform.forward * userInput.x + transform.right * userInput.y;

        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out RaycastHit hitInfo,
                           characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

        if (characterController.isGrounded)
        {
            moveDirection.y = -stickToGroundForce;

            if (jump)
            {
                moveDirection.y = jumpSpeed;
                PlayJumpSound();
                jump = false;
                jumping = true;
            }
        }
        else
        {
            moveDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }
        collisionFlags = characterController.Move(moveDirection * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        float rotX = 0.0f;
        float rotY = 0.0f;

        if (PlayerNumber == 1)
        {
            rotX = Input.GetAxis("PlayerOne_VerticalLook");
            rotY = Input.GetAxis("PlayerOne_HorizontalLook");
        }
        if (PlayerNumber == 2)
        {
            rotX = Input.GetAxis("PlayerTwo_VerticalLook");
            rotY = Input.GetAxis("PlayerTwo_HorizontalLook");
        }

        rotationX += rotX * cameraSensitivity * Time.deltaTime;
        rotationY += rotY * cameraSensitivity * Time.deltaTime;

        rotationX = Mathf.Clamp(rotationX, -cameraClampAngle, cameraClampAngle);

        camera.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
        this.transform.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
    }

    void PlayJumpSound()
    {
        audioSource.clip = jumpSound;
        audioSource.Play();
    }

    void ProgressStepCycle(float speed)
    {
        if (characterController.velocity.sqrMagnitude > 0.0f && (userInput.x != 0.0f || userInput.y != 0.0f))
        {
            if (isWalking)
            {
                stepCycle += (characterController.velocity.magnitude + (speed * 1.0f)) * Time.fixedDeltaTime;
            }
            else
            {
                stepCycle += (characterController.velocity.magnitude + (speed * runstepLengthen)) * Time.fixedDeltaTime;
            }
        }

        if (!(stepCycle > nextStep))
            return;

        nextStep = stepCycle + stepInterval;

        PlayFootStepAudio();
    }

    void PlayFootStepAudio()
    {
        if (!characterController.isGrounded)
            return;

        int randNum = UnityEngine.Random.Range(1, footstepSounds.Length);

        audioSource.clip = footstepSounds[randNum];
        audioSource.PlayOneShot(audioSource.clip);

        //move the chosen sound to index one, so it isn't played next time to allow unique sound to play
        footstepSounds[randNum] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;
    }

    void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;

        if (!useHeadBob)
            return;

        if (characterController.velocity.magnitude > 0 && characterController.isGrounded)
        {
            if (isWalking)
            {
                camera.transform.localPosition = headBob.DoHeadBob(characterController.velocity.magnitude + (speed * 1.0f));
            }
            else
            {
                camera.transform.localPosition = headBob.DoHeadBob(characterController.velocity.magnitude + (speed * runstepLengthen));
            }

            newCameraPosition = camera.transform.localPosition;
            newCameraPosition.y = camera.transform.localPosition.y - jumpBob.Offset();
        }
        else
        {
            newCameraPosition = camera.transform.localPosition;
            newCameraPosition.y = originalCameraPosition.y - jumpBob.Offset();
        }

        camera.transform.localPosition = newCameraPosition;
    }

    public void GetInput(out float speed)
    {
        float horizontal = 0.0f;
        float vertical = 0.0f;

        if (PlayerNumber == 1)
        {
            horizontal = -Input.GetAxis("PlayerOne_Vertical");
            vertical = Input.GetAxis("PlayerOne_Horizontal");
        }
        if (PlayerNumber == 2)
        {
            horizontal = -Input.GetAxis("PlayerTwo_Vertical");
            vertical = Input.GetAxis("PlayerTwo_Horizontal");
        }

        bool waswalking = isWalking;
        if (PlayerNumber == 1)
        {
            isWalking = !Input.GetButtonDown("PlayerOne_Jump");
        }
        if (PlayerNumber == 2)
        {
            isWalking = !Input.GetButtonDown("PlayerTwo_Jump");
        }

        if (isWalking)
        {
            speed = walkSpeed;
        }
        else
        {
            speed = runSpeed;
        }

        userInput = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (userInput.sqrMagnitude > 1)
        {
            userInput.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (isWalking != waswalking && useFovKick && characterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            if (isWalking)
            {
                StartCoroutine(fovKick.FOVKickUp());
            }
            else
            {
                StartCoroutine(fovKick.FOVKickDown());
            }

        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (collisionFlags == CollisionFlags.Below)
            return;

        if (body == null || body.isKinematic)
            return;

        body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
