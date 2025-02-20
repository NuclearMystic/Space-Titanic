using UnityEngine;

public class RotateY : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f; // Adjustable in the Inspector

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
