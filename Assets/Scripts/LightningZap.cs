using UnityEngine;
using System.Collections;

public class LightningZap : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;
    public float zapDuration = 0.2f;
    public int segments = 10;
    public float jaggedness = 0.5f;
    public float zapRadius = 0.5f; // New: Increases zap hitbox size

    public void Initialize(Vector3 start, Vector3 end)
    {
        RaycastHit hit;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        // Exclude the Ignore Raycast layer from the mask
        int layerMask = LayerMask.GetMask("Enemy") & ~LayerMask.GetMask("Ignore Raycast");

        // Using SphereCast for better hit detection
        if (Physics.SphereCast(start, zapRadius, direction, out hit, distance, layerMask))
        {
            Gremlin gremlin = hit.collider.GetComponentInParent<Gremlin>(); // Ensure we get the right script
            if (gremlin != null)
            {
                gremlin.GetShocked();
            }

            end = hit.point; // Adjust the zap effect to stop at the hit point
        }

        StartCoroutine(AnimateZap(start, end));
    }

    IEnumerator AnimateZap(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = segments;
        Vector3 direction = (end - start) / (segments - 1);

        for (int i = 0; i < segments; i++)
        {
            Vector3 offset = Vector3.zero;
            if (i > 0 && i < segments - 1)
            {
                offset = new Vector3(Random.Range(-jaggedness, jaggedness), Random.Range(-jaggedness, jaggedness), 0);
            }
            lineRenderer.SetPosition(i, start + direction * i + offset);
        }

        // Play impact effect
        impactEffect.transform.position = end;
        impactEffect.Play();

        // Destroy after duration
        yield return new WaitForSeconds(zapDuration);
        Destroy(gameObject);
    }
}
