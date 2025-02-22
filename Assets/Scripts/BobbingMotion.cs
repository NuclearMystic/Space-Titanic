using UnityEngine;

public class BobbingMotion : MonoBehaviour
{
    public float amplitude = 0.5f; // How high/low the object moves
    public float frequency = 1f;   // Speed of the bobbing motion

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
