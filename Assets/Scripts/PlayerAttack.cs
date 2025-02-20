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

    private BreakableObject currentRepairTarget;
    private bool isRepairing = false;
    private float repairSpeed = 3f;
    private float repairProgress = 0f;


    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        HandleAttackMode();
        HandleAttack();
        HandleAttackAnimations();

        if (isRepairing)
        {
            HandleRepair();
        }
    }

    void HandleAttackMode()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isRepairing)
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
        if (!isAttacking) return;

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
        Vector3 clickPosition = Input.mousePosition; // Get the mouse position on screen

        // Fire raycast from camera to clicked position
        Ray ray = Camera.main.ScreenPointToRay(clickPosition);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Enemy", "BreakableObject");

        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        Vector3 hitPoint = hitSomething ? hit.point : ray.GetPoint(10f); // Default to 10 units away if nothing is hit

        if (hitSomething)
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            //  Prioritize hitting a gremlin first
            Gremlin gremlin = hit.collider.GetComponentInParent<Gremlin>();
            if (gremlin != null)
            {
                Debug.Log("Gremlin hit! Calling GetShocked()");
                gremlin.GetShocked();
            }
            else if (hit.collider.CompareTag("Broken"))
            {
                //  If no gremlin was hit, check for a broken object
                Debug.Log("Hit a Broken object!");
                BreakableObject brokenObject = hit.collider.GetComponent<BreakableObject>();

                if (brokenObject != null)
                {
                    Debug.Log("Found BreakableObject component!");
                    StartRepairing(brokenObject);
                }
                else
                {
                    Debug.LogError("BreakableObject component NOT found on Broken object!");
                }
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing. Playing visual zap.");
        }

        // Play zap animation from wrench to clicked location (even if nothing was hit)
        GameObject zapInstance = Instantiate(lightningZapPrefab);
        zapInstance.GetComponent<LightningZap>().Initialize(wrenchTip, hitPoint);

        if (audioSource != null && zapSound != null)
            audioSource.PlayOneShot(zapSound);
    }

    public void StartRepairing(BreakableObject brokenObject)
    {
        if (isRepairing || brokenObject == null) return;

        isRepairing = true;
        currentRepairTarget = brokenObject;
        repairProgress = 0f;

        playerController.enabled = false;
        UIController.Instance.ShowRepairMeter(true);
        FireRepairZap();
    }

    private void FireRepairZap()
    {
        GameObject zapInstance = Instantiate(lightningZapPrefab);
        zapInstance.GetComponent<LightningZap>().Initialize(wrenchTip, currentRepairTarget.transform.position);

        if (audioSource != null && zapSound != null)
        {
            audioSource.loop = true;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void HandleRepair()
    {
        if (Input.GetMouseButton(0) && currentRepairTarget != null)
        {
            repairProgress += Time.deltaTime * repairSpeed;
            UIController.Instance.UpdateRepairMeter(repairProgress);

            if (repairProgress >= 10f)
            {
                CompleteRepair();
            }
            else if (Time.time > lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                FireRepairZap();
            }
        }
        else
        {
            CancelRepair();
        }
    }

    private void CompleteRepair()
    {
        isRepairing = false;
        if (currentRepairTarget != null)
        {
            currentRepairTarget.Restore();
        }

        UIController.Instance.ShowRepairMeter(false);
        playerController.enabled = true;
        StopRepairZap();
    }

    private void CancelRepair()
    {
        isRepairing = false;
        UIController.Instance.ShowRepairMeter(false);
        playerController.enabled = true;
        StopRepairZap();
    }

    private void StopRepairZap()
    {
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }
}
