using System.Collections;
using UnityEngine;

public class Gremlin : MonoBehaviour
{
    public Transform spawnPointA;
    public Transform spawnPointB;
    public float moveSpeed = 3f;
    public float fleeSpeed = 6f;
    public float detectionRadius = 5f;
    public float idleTime = 1f;
    public float fleeDuration = 2f;

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

    void Update()
    {
        if (isIdle) return;

        if (isFleeing)
        {
            FleeFromPlayer();
        }
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRadius)
            {
                StartCoroutine(Flee());
            }
            else
            {
                MoveToTarget();
            }
        }

        if (isAttacking && currentTarget != null && !currentTarget.isBroken)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
        else if (currentTarget != null && currentTarget.isBroken)
        {
            isAttacking = false;
            currentTarget = null;
        }
    }

    private void Attack()
    {
        if (currentTarget == null || currentTarget.isBroken) return;

        // Trigger Claw animation
        animator.SetTrigger("Claw");

        // Schedule damage to apply when animation plays
        Invoke(nameof(ApplyDamage), 0.3f); // Adjust timing based on animation

        // Set next attack time for cooldown
        nextAttackTime = Time.time + attackCooldown;
    }

    private void ApplyDamage()
    {
        if (currentTarget != null && !currentTarget.isBroken)
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
        isFleeing = true;
        rb.velocity = Vector3.zero;
        animator.SetTrigger("Shock");

        yield return new WaitForSeconds(0.5f);

        float fleeTime = 0f;
        while (fleeTime < fleeDuration)
        {
            FleeFromPlayer();
            fleeTime += Time.deltaTime;
            yield return null;
        }

        isFleeing = false;
    }

    public void GetShocked()
    {
        if (isFleeing) return;

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
            GremlinSpawnPoint spawnPoint = GetComponentInParent<GremlinSpawnPoint>();
            if (spawnPoint)
            {
                spawnPoint.NotifyGremlinDespawn();
            }

            Destroy(transform.parent ? transform.parent.gameObject : gameObject);
        }
        else if (other.CompareTag("BreakableObject"))
        {
            BreakableObject breakable = other.GetComponent<BreakableObject>();
            if (breakable != null && !breakable.isBroken)
            {
                currentTarget = breakable;
                isAttacking = true;
                Attack();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the gremlin collides with another gremlin, check for panic chain reaction
        Gremlin otherGremlin = collision.collider.GetComponent<Gremlin>();
        if (otherGremlin != null && isFleeing && !otherGremlin.isFleeing)
        {
            otherGremlin.GetShocked(); // Make the other gremlin flee too
        }
    }

}
