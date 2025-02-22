using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 7f;
    public float climbSpeed = 3f;

    [Header("Zero-G Settings")]
    public float floatForce = 3f;  // How much force is applied in Zero-G
    public float zeroGDrag = 0.1f; // Air resistance in Zero-G
    public float slowDownFactor = 0.98f; // How much velocity slows over time

    [Header("Gravity Settings")]
    public bool isGrounded;
    public bool isZeroGravity = false;
    public float groundingForce = 5f;

    [Header("Ladder Settings")]
    public bool isClimbing = false;
    private Ladder currentLadder;

    [Header("Ground Check Settings")]
    public Transform groundCheckPoint; // Assign in Inspector (Empty GameObject)
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;


    private Rigidbody rb;
    private Animator animator;
    private bool facingRight = true;

    public bool isRepairing { get; set; } = false; // Track repair status
    private bool jumpRequested = false; // Track jump request from input

    [Header("Jump-Through Floor Settings")]
    public LayerMask jumpableFloorLayer; // Assign in Inspector


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        CheckGrounded();
        animator.SetBool("isGrounded", isGrounded);

        if (isClimbing)
        {
            if (!isRepairing) HandleLadderMovement();
            return; // Stop further updates while climbing
        }

        if (!isRepairing)
        {
            HandleInput();
            HandleAnimations();
            FlipCharacter();
        }
    }

    void FixedUpdate()
    {

        if (isClimbing)
        {
            rb.velocity = new Vector3(0, Input.GetAxis("Vertical") * climbSpeed, 0);

            if (currentLadder != null)
            {
                currentLadder.CheckIfAtTop(this);
            }
        }
        else if (!isRepairing)
        {
            if (isZeroGravity)
            {
                ApplyZeroGMovement();
            }
            else
            {
                HandleMovement();
                ApplyGroundingForce();
            }
        }
    }

    void HandleInput()
    {
        if (isRepairing) return;

        float horizontalInput = Input.GetAxis("Horizontal");

        if (isZeroGravity)
        {
            float verticalInput = (Input.GetButton("Jump") ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0);

            float horizontalThrustMultiplier = 20f;  // Quadruple X movement
            float verticalThrustMultiplier = 1f; // Quarter Y movement

            Vector3 moveDirection = new Vector3(
                horizontalInput * horizontalThrustMultiplier,
                verticalInput * verticalThrustMultiplier,
                0
            );

            float maxSpeed = 75f; // Set max speed limit

            // **Fix: Apply force using Time.fixedDeltaTime**
            if (rb.velocity.sqrMagnitude < maxSpeed * maxSpeed)
            {
                rb.AddForce(moveDirection * floatForce * Time.deltaTime, ForceMode.Acceleration);
            }

            // **Improved Deceleration: Use Drag Instead of a Manual Counter-Force**
            float dragAmount = 0.98f; // Adjust for desired slow-down speed
            rb.velocity *= dragAmount;
        }
        else
        {
            if (Input.GetKey(KeyCode.S) && Input.GetButtonDown("Jump"))
            {
                TryDropToLowerFloor();
            }
            else if (Input.GetButtonDown("Jump") && isGrounded)
            {
                TriggerJumpAnimation();
            }
        }
    }




    void ApplyZeroGMovement()
    {
        rb.useGravity = false;
        rb.drag = zeroGDrag;

        // Slow the player over time to prevent infinite drifting
        rb.velocity *= slowDownFactor;
    }

    public void ActivateZeroG()
    {
        isZeroGravity = true;
        rb.useGravity = false;
        rb.drag = zeroGDrag;

        // Reset horizontal movement so the player doesn't keep running forward
        //rb.velocity = new Vector3(0, rb.velocity.y, 0);

        // Apply a small upward push when Zero-G activates to start floating
        rb.AddForce(Vector3.up * 3f, ForceMode.Force);



        // Start Lerp Coroutine for Animation Speed
        StartCoroutine(LerpAnimationSpeedToZero());


        animator.SetBool("isZeroGravity", true);
    }

    private IEnumerator LerpAnimationSpeedToZero()
    {
        float currentSpeed = animator.GetFloat("Speed");
        float lerpDuration = 1.5f; // Duration in seconds
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;
            float newSpeed = Mathf.Lerp(currentSpeed, 0f, elapsedTime / lerpDuration);
            animator.SetFloat("Speed", newSpeed);
            yield return null;
        }

        animator.SetFloat("Speed", 0f); // Ensure it fully reaches zero
    }

    public void DeactivateZeroG()
    {
        Debug.Log("Player exiting Zero-G");

        isZeroGravity = false;
        rb.useGravity = true;
        rb.drag = 0f;

        // Reset horizontal velocity again when Zero-G ends
        rb.velocity = new Vector3(0, rb.velocity.y, 0);



        animator.SetBool("isZeroGravity", false);
    }

    private void TryDropToLowerFloor()
    {
        Debug.Log(IsStandingOnJumpableFloor());
        if (IsStandingOnJumpableFloor()) // Check if the player is on a drop-through floor
        {
            transform.position += new Vector3(0, 0, -5f); // Instantly move -5 on Z-axis
        }
    }

    private bool IsStandingOnJumpableFloor()
    {
        RaycastHit hit;
        float sphereRadius = 0.3f; // Small sphere to check ground
        float sphereCastDistance = 1f; // Short distance to check just below feet

        if (Physics.SphereCast(transform.position, sphereRadius, Vector3.down, out hit, sphereCastDistance))
        {
            return hit.collider.CompareTag("JumpableFloor");
        }

        return false;
    }


    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float speed = walkSpeed * (Input.GetButton("Run") ? runSpeedMultiplier : 1);
        rb.velocity = new Vector3(horizontalInput * speed, rb.velocity.y, 0);
    }

    void HandleLadderMovement()
    {
        if (isRepairing) return; // Block ladder movement while repairing

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        animator.speed = (Mathf.Abs(verticalInput) > 0) ? 1 : 0;

        if (isGrounded && Mathf.Abs(horizontalInput) > 0.3f)
        {
            ExitLadder();
        }

        if (Input.GetButtonDown("Jump"))
        {
            ExitLadder();
        }
    }

    public void StartClimbing(Ladder ladder, Vector3 snapPosition)
    {
        if (isRepairing) return; // Prevent climbing while repairing

        isClimbing = true;
        currentLadder = ladder;

        transform.position = new Vector3(snapPosition.x, snapPosition.y, snapPosition.z);

        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        animator.SetBool("isClimbing", true);
    }

    public void ExitLadder()
    {
        isClimbing = false;
        rb.useGravity = true;
        animator.SetBool("isClimbing", false);
        animator.speed = 1;
    }

    public void ExitLadderAtTop(Vector3 topExitPos)
    {
        isClimbing = false;
        rb.useGravity = true;
        transform.position = topExitPos;

        currentLadder = null;
        animator.SetBool("isClimbing", false);
    }

    void HandleAnimations()
    {
        if (GetComponent<PlayerAttack>().isAttacking || isRepairing) return;

        animator.SetBool("isGrounded", isGrounded);

        if (!isClimbing)
        {
            float movementSpeed = Mathf.Abs(Input.GetAxis("Horizontal")) * (Input.GetKey(KeyCode.LeftShift) ? runSpeedMultiplier : 1);
            animator.SetFloat("Speed", movementSpeed);
            animator.SetBool("isZeroGravity", isZeroGravity);
        }
    }

    void FlipCharacter()
    {
        if (isRepairing) return; // Prevent flipping while repairing

        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0 && !facingRight)
        {
            facingRight = true;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
        }
        else if (horizontalInput < 0 && facingRight)
        {
            facingRight = false;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
        }
    }

    void CheckGrounded()
    {
        if (!isZeroGravity)
        {

            if (groundCheckPoint == null)
            {
                Debug.LogWarning("GroundCheckPoint is not assigned!");
                return;
            }

            // Simple sphere check at groundCheckPoint's position
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

            // If grounded and falling, ensure the player stays firmly on the ground
            if (isGrounded && rb.velocity.y < 0f)
            {
                rb.velocity = new Vector3(rb.velocity.x, -2f, rb.velocity.z); // Prevents floating issues
            }

        }else if (isZeroGravity)
        {
            isGrounded = false;
        }
    }



    void TriggerJumpAnimation()
    {
        if (!isGrounded) return; // Prevent double jumping

        jumpRequested = true; // Set flag for JumpLaunch()
        animator.SetTrigger("Jump");
    }

    public void JumpLaunch()
    {
        if (!jumpRequested) return; // Ensure jump was requested before launching

        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        jumpRequested = false; // Reset after jumping
    }

    void ApplyGroundingForce()
    {
        if (isGrounded && !isZeroGravity && rb.velocity.y < -0.1f)
        {
            rb.AddForce(Vector3.down * groundingForce, ForceMode.Acceleration);
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red; // Green if grounded, red if not

        // Draw a wire sphere where the ground check occurs
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}


