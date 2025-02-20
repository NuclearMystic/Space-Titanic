using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    public GameObject lightningZapPrefab;
    public Transform wrenchTip;
    public GameObject wrenchLight;
    public AudioSource audioSource;
    public AudioClip drawSound;
    public AudioClip zapSound;

    public float attackCooldown = 0.5f;
    public float attackModeDuration = 5f;

    public bool isAttacking { get; private set; }

    private float lastAttackTime;
    private float attackModeStartTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>(); // Get movement component
    }

    void Update()
    {
        HandleAttackMode();
        HandleAttack();
        HandleAttackAnimations(); // Update movement animations in attack mode
    }

    void HandleAttackMode()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            EnterAttackMode();
        }

        if (isAttacking && Time.time - attackModeStartTime >= attackModeDuration)
        {
            ExitAttackMode();
        }
    }

    void HandleAttack()
    {
        if (isAttacking && Input.GetMouseButtonDown(0) && Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            attackModeStartTime = Time.time;
            PerformAttack();
        }
    }

    void HandleAttackAnimations()
    {
        if (!isAttacking) return; // Only handle animations in attack mode

        float movementSpeed = Mathf.Abs(playerController.horizontalInput) *
            (Input.GetKey(KeyCode.LeftShift) ? playerController.runSpeedMultiplier : 1);

        animator.SetFloat("Speed", movementSpeed);
    }

    void EnterAttackMode()
    {
        isAttacking = true;
        attackModeStartTime = Time.time;
        animator.SetBool("inAttackMode", true);

        if (wrenchLight != null)
            wrenchLight.SetActive(true);

        if (audioSource != null && drawSound != null)
            audioSource.PlayOneShot(drawSound);
    }

    void ExitAttackMode()
    {
        isAttacking = false;
        animator.SetBool("inAttackMode", false);

        if (wrenchLight != null)
            wrenchLight.SetActive(false);
    }

    void PerformAttack()
    {
        Vector3 targetPosition = GetMouseWorldPosition();

        GameObject zapInstance = Instantiate(lightningZapPrefab);
        zapInstance.GetComponent<LightningZap>().Initialize(wrenchTip.position, targetPosition);

        if (audioSource != null && zapSound != null)
            audioSource.PlayOneShot(zapSound);
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return transform.position + transform.forward * 2f;
    }
}
