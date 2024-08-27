using UnityEngine;

public class CameraController_H : MonoBehaviour
{
    public Transform target; // The object to orbit around
    public float rotationSpeed = 0.5f;
    public float zoomSpeed = 5.0f;
    public float minZoom = 5.0f;
    public float maxZoom = 20.0f;
    public float yMinLimit = -20f; // Minimum Y rotation
    public float yMaxLimit = 80f;  // Maximum Y rotation
    public float distance = 10.0f; // Initial distance from target

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float initialCameraZ;
    private float pinchZoomSpeed = 0.1f; // Speed of pinch zoom
    private Camera playerCam;

    void Start()
    {
        playerCam = Camera.main;
        initialCameraZ = playerCam.transform.position.z;

        Vector3 angles = playerCam.transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Set initial position of the camera
        UpdateCameraPosition();

        distance = Vector3.Distance(this.transform.position, target.transform.position)+5f;
    }

    void Update()
    {
        HandleMouseDrag();
        HandleMouseScroll();
        HandlePinchZoom();
        UpdateCameraPosition();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButton(0))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;

            currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

            UpdateCameraPosition();
        }
    }

    void HandleMouseScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            playerCam.fieldOfView -= scroll * zoomSpeed;
            playerCam.fieldOfView = Mathf.Clamp(playerCam.fieldOfView, minZoom, maxZoom);
        }
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            playerCam.fieldOfView += deltaMagnitudeDiff * pinchZoomSpeed;
            playerCam.fieldOfView = Mathf.Clamp(playerCam.fieldOfView, minZoom, maxZoom);
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        playerCam.transform.rotation = rotation;
        playerCam.transform.position = position;
    }
}
