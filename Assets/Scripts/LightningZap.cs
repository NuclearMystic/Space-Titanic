using UnityEngine;
using System.Collections;

public class LightningZap : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;
    public float zapDuration = 0.2f;
    public int segments = 10;
    public float jaggedness = 0.5f;

    public void Initialize(Transform wrenchTip, Vector3 targetPosition)
    {
        StartCoroutine(AnimateZap(wrenchTip.position, targetPosition));
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

        impactEffect.transform.position = end;
        impactEffect.Play();

        yield return new WaitForSeconds(zapDuration);
        Destroy(gameObject);
    }

    public void UpdateZap(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) return;

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

        impactEffect.transform.position = end;
        if (!impactEffect.isPlaying)
        {
            impactEffect.Play();
        }
    }

}
