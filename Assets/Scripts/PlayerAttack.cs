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
        if (isRepairing) return;

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
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Enemy", "BreakableObject");

        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        Vector3 hitPoint = hitSomething ? hit.point : ray.GetPoint(10f);

        if (hitSomething)
        {
            Gremlin gremlin = hit.collider.GetComponentInParent<Gremlin>();
            if (gremlin != null)
            {
                gremlin.GetShocked();
            }
            else if (hit.collider.CompareTag("Broken"))
            {
                BreakableObject brokenObject = hit.collider.GetComponent<BreakableObject>();
                if (brokenObject != null)
                {
                    StartRepairing(brokenObject);
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

        isRepairing = true;
        currentRepairTarget = brokenObject;
        repairProgress = 0f;

        playerController.isRepairing = true; // Prevent input but allow movement
        UIController.Instance.ShowRepairMeter(true);
        MaintainZapEffect();
    }

    private void MaintainZapEffect()
    {
        if (activeZapEffect == null)
        {
            activeZapEffect = Instantiate(lightningZapPrefab);
        }
        activeZapEffect.GetComponent<LightningZap>().Initialize(wrenchTip, currentRepairTarget.transform.position);

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

            if (repairProgress >= 10f)
            {
                CompleteRepair();
            }
        }
        else
        {
            Invoke(nameof(CancelRepair), 0.5f); // Grace period before canceling
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
        playerController.isRepairing = false;
        StopRepairZap();
    }

    private void CancelRepair()
    {
        if (!isRepairing) return; // Prevent duplicate cancels

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
            Destroy(activeZapEffect, 0.5f); // Fade out effect instead of instant stop
            activeZapEffect = null;
        }
    }
}
