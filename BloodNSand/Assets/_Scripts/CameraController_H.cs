using UnityEngine;

public class CameraController_H : MonoBehaviour
{
    public Transform target; // The default object to orbit around
    public float rotationSpeed = 0.5f;
    public float zoomSpeed = 5.0f;
    public float minZoom = 5.0f;
    public float maxZoom = 20.0f;
    public float yMinLimit = -20f; // Minimum Y rotation
    public float yMaxLimit = 80f;  // Maximum Y rotation
    public float distance = 10.0f; // Initial distance from target

    private Transform defaultTarget; // To store the default target
    private Vector3 cameraPosition;  // Store the camera position
    private Quaternion cameraRotation; // Store the camera rotation
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float initialCameraZ;
    private float pinchZoomSpeed = 0.1f; // Speed of pinch zoom
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        initialCameraZ = this.transform.position.z;

        Vector3 angles = this.transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Store the default target
        defaultTarget = target;

        // Set initial position of the camera
        UpdateCameraPosition(defaultTarget);
    }

    void Update()
    {
        HandleMouseDrag();
        HandleMouseScroll();
        HandlePinchZoom();
        HandleObjectClick(); // Check for object click
        // Only update the camera's position and rotation based on the stored values
        this.transform.position = cameraPosition;
        this.transform.rotation = cameraRotation;
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
            playerCamera.fieldOfView -= scroll * zoomSpeed;
            playerCamera.fieldOfView = Mathf.Clamp(playerCamera.fieldOfView, minZoom, maxZoom);
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

            playerCamera.fieldOfView += deltaMagnitudeDiff * pinchZoomSpeed;
            playerCamera.fieldOfView = Mathf.Clamp(playerCamera.fieldOfView, minZoom, maxZoom);
        }
    }
    Transform currentTarget;
    void HandleObjectClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    target = hit.transform;
                    UpdateCameraPosition(target); // Update the camera to the new target once
                    currentTarget = target;
                }
            }
        }
    }

    void UpdateCameraPosition(Transform targetPos=null)
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        //Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        //Vector3 position = rotation * negDistance + target.position;

        // Store the calculated position and rotation
        if(targetPos!= null)
        cameraPosition = targetPos.position;

        cameraRotation = rotation;

        // Set the camera's position and rotation to the stored values
        this.transform.position = cameraPosition;
        this.transform.rotation = cameraRotation;
    }

    public void ResetToDefaultTarget()
    {
        target = defaultTarget;
        UpdateCameraPosition(target);
    }
}
