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
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Animator animator;
    public float horizontalInput;
    private float verticalInput;
    private bool jumpRequested = false;
    public bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isClimbing)
        {
            HandleLadderMovement();
        }
        else
        {
            HandleInput();
            HandleAnimations();
            FlipCharacter();
        }

        CheckGrounded();
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.velocity = new Vector3(0, verticalInput * climbSpeed, 0);

            if (currentLadder != null)
            {
                currentLadder.CheckIfAtTop(this);
            }
        }
        else
        {
            HandleMovement();
            ApplyGroundingForce();
        }
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isZeroGravity)
        {
            TriggerJumpAnimation();
        }

        if (isZeroGravity)
        {
            verticalInput = 0;
            if (Input.GetKey(KeyCode.Space)) verticalInput = 1;
            if (Input.GetKey(KeyCode.LeftControl)) verticalInput = -1;
        }
    }

    void HandleMovement()
    {
        float speed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= runSpeedMultiplier;

        Vector3 moveDirection = new Vector3(horizontalInput * speed, 0, 0);

        if (!isZeroGravity)
        {
            rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, 0);
        }
        else
        {
            rb.velocity = new Vector3(moveDirection.x, verticalInput * zeroGSpeed, 0);
        }
    }

    void HandleLadderMovement()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(verticalInput) > 0)
        {
            animator.speed = 1;
        }
        else
        {
            animator.speed = 0;
        }

        if (isGrounded && horizontalInput != 0)
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
        isClimbing = true;
        currentLadder = ladder;

        // Snap to the ladder, keeping Y from snapPosition (dynamically follows player Y)
        transform.position = new Vector3(snapPosition.x, snapPosition.y, snapPosition.z);

        // Disable gravity & normal movement
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // Play climbing animation
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
        if (GetComponent<PlayerAttack>().isAttacking) return; // Let PlayerAttack handle animations

        animator.SetBool("isGrounded", isGrounded);

        if (!isClimbing)
        {
            if (!isZeroGravity)
            {
                float movementSpeed = Mathf.Abs(horizontalInput) * (Input.GetKey(KeyCode.LeftShift) ? runSpeedMultiplier : 1);
                animator.SetFloat("Speed", movementSpeed);
                animator.SetBool("isZeroGravity", false);
            }
            else
            {
                float floatingSpeed = Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput);
                animator.SetFloat("FloatSpeed", floatingSpeed);
                animator.SetBool("isZeroGravity", true);
            }
        }
    }


    void FlipCharacter()
    {
        if (!isClimbing)
        {
            if (horizontalInput > 0 && !facingRight)
            {
                facingRight = true;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
            }
            else if (horizontalInput < 0 && facingRight)
            {
                facingRight = false;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -1);
            }
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    void TriggerJumpAnimation()
    {
        jumpRequested = true;
        animator.SetTrigger("Jump");
    }

    public void JumpLaunch()
    {
        if (jumpRequested)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            jumpRequested = false;
        }
    }

    void ApplyGroundingForce()
    {
        if (isGrounded && !isZeroGravity && rb.velocity.y <= 0)
        {
            rb.AddForce(Vector3.down * groundingForce, ForceMode.Acceleration);
        }
    }
}
