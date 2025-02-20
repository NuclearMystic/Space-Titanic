using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer transposer;
    private Transform playerTransform;

    [Header("Camera Settings")]
    public float screenXRight = 0.2f;
    public float screenXLeft = 0.8f;
    public float transitionSpeed = 2f;
    public float minVelocityForScreenAdjust = 0.1f;

    [Header("Zoom Settings")]
    public float minZoom = 3f;
    public float maxZoom = 10f;
    public float zoomSpeed = 2f;
    public float zoomSmoothness = 5f;

    private Rigidbody playerRigidbody;
    private float targetScreenX;
    private float targetZoom;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera.Follow != null)
        {
            playerTransform = virtualCamera.Follow;
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        }

        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (playerTransform != null)
        {
            targetScreenX = (playerTransform.localScale.z > 0) ? screenXRight : screenXLeft;
        }

        if (virtualCamera != null)
        {
            targetZoom = virtualCamera.m_Lens.OrthographicSize;
        }
    }

    void Update()
    {
        HandleScreenXShift();
        HandleZoom();
    }

    void HandleScreenXShift()
    {
        if (playerTransform == null || playerRigidbody == null || transposer == null) return;

        float playerVelocityX = playerRigidbody.velocity.x;

        if (Mathf.Abs(playerVelocityX) > minVelocityForScreenAdjust)
        {
            targetScreenX = (playerTransform.localScale.z > 0) ? screenXRight : screenXLeft;
        }

        transposer.m_ScreenX = Mathf.Lerp(transposer.m_ScreenX, targetScreenX, transitionSpeed * Time.deltaTime);
    }

    void HandleZoom()
    {
        if (Time.timeScale == 0 || virtualCamera == null) return; // Prevent zooming while paused

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetZoom, zoomSmoothness * Time.deltaTime);
    }
}
