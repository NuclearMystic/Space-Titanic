using System.Collections;
using UnityEngine;
using static BreakableObject;

public class Gremlin : MonoBehaviour
{
    public Transform spawnPointA;
    public Transform spawnPointB;
    public float moveSpeed = 3f;
    public float fleeSpeed = 6f;
    public float detectionRadius = 5f;
    public float idleTime = 1f;
    public float baseFleeDuration = 2f; // Base flee time
    private float fleeTimeMultiplier = 1f; // 1x for normal flee, 2x for zap flee

    public Light zapLight;
    public AudioSource zapSound;
    public float lightDuration = 0.3f;

    private Transform targetPoint;
    private Rigidbody rb;
    private Animator animator;
    private bool isFleeing = false;
    private bool isIdle = false;
    private Transform player;
    private bool facingRight = true;

    private BreakableObject currentTarget;
    private bool isAttacking = false;
    private float attackCooldown = 1f;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        targetPoint = spawnPointA;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!player)
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");

        if (zapLight)
            zapLight.enabled = false;
    }

    private void Update()
    {
        if (isIdle) return;

        if (isFleeing)
        {
            FleeFromPlayer();
            return; // Stop further logic while fleeing
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            fleeTimeMultiplier = 1f;
            StartCoroutine(Flee());
        }
        else
        {
            MoveToTarget();
        }

        if (isAttacking && currentTarget != null)
        {
            if (currentTarget.state != ObjectState.Broken && Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
    }

    private void Attack()
    {
        if (isFleeing || currentTarget == null) return; // Stop attacks while fleeing

        if (currentTarget.state == ObjectState.Broken)
        {
            isAttacking = false;
            currentTarget = null;
            return;
        }

        animator.SetTrigger("Claw");
        Invoke(nameof(ApplyDamage), 0.3f);
        nextAttackTime = Time.time + attackCooldown;
    }

    private void ApplyDamage()
    {
        if (currentTarget != null && currentTarget.state != ObjectState.Broken)
        {
            currentTarget.AddDamage(1);
        }
    }

    private void MoveToTarget()
    {
        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
        rb.velocity = new Vector3(direction * moveSpeed, rb.velocity.y, 0f);

        animator.SetFloat("Speed", moveSpeed);

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
        {
            Flip();
        }

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            StartCoroutine(PauseAtPoint());
        }
    }

    private IEnumerator PauseAtPoint()
    {
        isIdle = true;
        rb.velocity = Vector3.zero;
        animator.SetFloat("Speed", 0);

        yield return new WaitForSeconds(idleTime);

        isIdle = false;
        targetPoint = (targetPoint == spawnPointA) ? spawnPointB : spawnPointA;
    }

    private void FleeFromPlayer()
    {
        float direction = Mathf.Sign(transform.position.x - player.position.x);
        rb.velocity = new Vector3(direction * fleeSpeed, rb.velocity.y, 0f);
        animator.SetFloat("Speed", fleeSpeed);

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
        {
            Flip();
        }
    }

    private IEnumerator Flee()
    {
        if (currentTarget != null)
        {
            currentTarget.NotifyGremlinLeft(); // Let the object know it's no longer under attack
            currentTarget = null;
        }

        isFleeing = true;
        isAttacking = false; // Stop attacking when fleeing

        rb.velocity = Vector3.zero;
        animator.SetTrigger("Shock");

        yield return new WaitForSeconds(0.5f);

        float totalFleeTime = baseFleeDuration * fleeTimeMultiplier;
        float fleeTime = 0f;

        while (fleeTime < totalFleeTime)
        {
            FleeFromPlayer();
            fleeTime += Time.deltaTime;
            yield return null;
        }

        isFleeing = false;
        fleeTimeMultiplier = 1f;
    }


    public void GetShocked()
    {
        if (isFleeing) return;

        fleeTimeMultiplier = 2f; // Double flee time when zapped

        if (zapLight)
            StartCoroutine(FlashLight());

        if (zapSound)
            zapSound.Play();

        StartCoroutine(Flee());
    }

    private IEnumerator FlashLight()
    {
        zapLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        zapLight.enabled = false;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GremlinHole"))
        {
            if (currentTarget != null)
            {
                currentTarget.NotifyGremlinLeft(); //  Make sure we stop marking it as attacked
            }

            GremlinSpawnPoint spawnPoint = GetComponentInParent<GremlinSpawnPoint>();
            if (spawnPoint)
            {
                spawnPoint.NotifyGremlinDespawn();
            }

            Destroy(transform.parent ? transform.parent.gameObject : gameObject);
        }
        else if (!isFleeing && other.CompareTag("BreakableObject")) // Ignore if fleeing
        {
            BreakableObject breakable = other.GetComponent<BreakableObject>();
            if (breakable != null && breakable.state != ObjectState.Broken) // Check state properly
            {
                currentTarget = breakable;
                isAttacking = true;
                Attack();
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        Gremlin otherGremlin = collision.collider.GetComponent<Gremlin>();
        if (otherGremlin != null && isFleeing && !otherGremlin.isFleeing)
        {
            otherGremlin.GetShocked();
        }
    }

    public void DespawnGremlin()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
