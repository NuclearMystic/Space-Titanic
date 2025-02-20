using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 7f;
    public float zeroGSpeed = 3f;
    public float climbSpeed = 3f;

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


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        CheckGrounded();


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
        else if (!isRepairing) // Prevent movement while repairing
        {
            HandleMovement();
            ApplyGroundingForce();
        }
    }

    void HandleInput()
    {
        if (isRepairing) return; // Block input while repairing

        float horizontalInput = Input.GetAxis("Horizontal");

        if (isZeroGravity)
        {
            float verticalInput = (Input.GetButton("Jump") ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0);
            rb.velocity = new Vector3(horizontalInput * zeroGSpeed, verticalInput * zeroGSpeed, 0);
        }
        else
        {
            if (Input.GetKey(KeyCode.S) && Input.GetButtonDown("Jump")) // Hold "S" + Press "Jump"
            {
                TryDropToLowerFloor(); // Teleport to lower floor
                Debug.Log("Tried to jump down");
            }
            else if (Input.GetButtonDown("Jump") && isGrounded)
            {
                TriggerJumpAnimation(); // Normal jump
            }
        }
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


