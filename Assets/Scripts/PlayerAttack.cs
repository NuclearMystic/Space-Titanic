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
    public float repairSpeed = 3f; // Speed of repair

    public bool isAttacking { get; private set; }
    private bool isRepairing = false;

    private float lastAttackTime;
    private float attackModeStartTime;
    private float repairProgress = 0f;

    private BreakableObject currentRepairTarget;
    private GameObject activeZapEffect;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        HandleAttacks();
        HandleAttackAnimations();

        if (isRepairing)
        {
            HandleRepair();
        }
    }

    void HandleAttacks()
    {
        if (isRepairing) return; // Prevent attacks while repairing

        if (Input.GetMouseButtonDown(0))
        {
            if (!isAttacking)
            {
                EnterAttackMode();
            }

            if (Time.time > lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                attackModeStartTime = Time.time;
                PerformAttack();
            }
        }

        if (isAttacking && Time.time - attackModeStartTime >= attackModeDuration)
        {
            ExitAttackMode();
        }
    }

    void HandleAttackAnimations()
    {
        if (!isAttacking) return;

        float movementSpeed = Mathf.Abs(Input.GetAxis("Horizontal")) *
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float sphereRadius = 1f; // Adjust as needed for easier hits
        int layerMask = LayerMask.GetMask("Enemy", "BreakableObject");

        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, Mathf.Infinity, layerMask);
        Vector3 hitPoint = ray.GetPoint(10f); // Default if nothing is hit

        foreach (RaycastHit hit in hits)
        {
            Gremlin gremlin = hit.collider.GetComponentInParent<Gremlin>();
            if (gremlin != null)
            {
                gremlin.GetShocked();
                hitPoint = hit.point; // Update hit point to the first valid hit
            }
            else if (hit.collider.CompareTag("Broken"))
            {
                BreakableObject brokenObject = hit.collider.GetComponent<BreakableObject>();
                if (brokenObject != null && brokenObject.CanBeRepaired())
                {
                    StartRepairing(brokenObject);
                    hitPoint = hit.point;
                }
            }
        }

        PlayZapEffect(hitPoint);
    }


    void PlayZapEffect(Vector3 target)
    {
        GameObject zapInstance = Instantiate(lightningZapPrefab);
        zapInstance.GetComponent<LightningZap>().Initialize(wrenchTip, target);

        if (audioSource != null && zapSound != null)
            audioSource.PlayOneShot(zapSound);
    }

    public void StartRepairing(BreakableObject brokenObject)
    {
        if (isRepairing || brokenObject == null) return;

        Debug.Log("DEBUG: Player started repairing " + brokenObject.gameObject.name);

        isRepairing = true;
        currentRepairTarget = brokenObject;
        repairProgress = 0f;

        playerController.isRepairing = true;
        UIController.Instance.ShowRepairMeter(true);

        // Use repairPoint position if it exists, otherwise fallback to object center
        Vector3 repairTargetPos = (currentRepairTarget.repairPoint != null) ?
            currentRepairTarget.repairPoint.position : currentRepairTarget.transform.position;

        MaintainZapEffect(repairTargetPos);
    }

    private void MaintainZapEffect(Vector3 targetPos)
    {
        if (activeZapEffect == null)
        {
            activeZapEffect = Instantiate(lightningZapPrefab);
        }

        activeZapEffect.GetComponent<LightningZap>().UpdateZap(wrenchTip.position, targetPos);

        if (audioSource != null && zapSound != null && !audioSource.isPlaying)
        {
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void HandleRepair()
    {
        if (Input.GetMouseButton(0) && currentRepairTarget != null)
        {
            repairProgress += Time.deltaTime * repairSpeed;
            UIController.Instance.UpdateRepairMeter(repairProgress);

            // Keep zap effect locked onto repair target
            Vector3 repairTargetPos = (currentRepairTarget.repairPoint != null) ?
                currentRepairTarget.repairPoint.position : currentRepairTarget.transform.position;

            MaintainZapEffect(repairTargetPos);

            if (repairProgress >= 10f)
            {
                CompleteRepair();
            }
        }
        else if (Input.GetMouseButtonUp(0)) // <-- Fix: Stop repairing on release
        {
            CancelRepair();
        }
    }



    private void CompleteRepair()
    {
        if (currentRepairTarget == null) return;

        Debug.Log("DEBUG: Repair complete on " + currentRepairTarget.gameObject.name);

        isRepairing = false;

        // Fully restore the object's health and state
        currentRepairTarget.Restore();

        UIController.Instance.ShowRepairMeter(false);
        playerController.isRepairing = false;
        StopRepairZap();
    }

    private void CancelRepair()
    {
        if (!isRepairing) return;

        Debug.Log("DEBUG: Repair canceled.");
        isRepairing = false;
        UIController.Instance.ShowRepairMeter(false);
        playerController.isRepairing = false;
        StopRepairZap();
    }

    private void StopRepairZap()
    {
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }

        if (activeZapEffect != null)
        {
            Destroy(activeZapEffect);
            activeZapEffect = null;
        }
    }
}
