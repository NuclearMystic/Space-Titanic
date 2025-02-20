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

    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private float targetScreenX;
    private float targetZoom;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera.Follow != null)
        {
            playerTransform = virtualCamera.Follow;
            playerController = playerTransform.GetComponent<PlayerController>();
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        }

        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        targetScreenX = screenXRight; // Default to facing right
        targetZoom = virtualCamera.m_Lens.OrthographicSize; // Start at current zoom level
    }

    void Update()
    {
        HandleScreenXShift();
        HandleZoom();
    }

    void HandleScreenXShift()
    {
        if (playerController != null && playerRigidbody != null)
        {
            float playerVelocityX = playerRigidbody.velocity.x;

            if (Mathf.Abs(playerVelocityX) > minVelocityForScreenAdjust)
            {
                targetScreenX = playerController.facingRight ? screenXRight : screenXLeft;
            }

            transposer.m_ScreenX = Mathf.Lerp(transposer.m_ScreenX, targetScreenX, transitionSpeed * Time.deltaTime);
        }
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetZoom, zoomSmoothness * Time.deltaTime);
    }
}
