using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExamineObject : MonoBehaviour
{
    public static ExamineObject instance;

    public float xRotationSpeed = 50f;  // Rotation speed for the x-axis
    public float yRotationSpeed = 50; // Rotation speed for the y-axis
    [Range(0f, 1f)]
    public float smoothTime = 0.1f;     // Normalized smooth time between 0 and 1

    public bool xRotation = true;  // Enable or disable rotation on the x-axis
    public bool yRotation = true;  // Enable or disable rotation on the y-axis

    private Vector3 previousMousePosition;
    private Vector3 currentRotation;
    private Vector3 rotationVelocity;

    //UI Detection Function Elements
    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _results;
    /// /////////////////////

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        if (!IsOverUi())
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Store the initial mouse position when the button is pressed
                previousMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                // Calculate the difference in mouse position
                Vector3 delta = Input.mousePosition - previousMousePosition;
                // Calculate desired rotation based on mouse movement
                Vector3 desiredRotation = Vector3.zero;
                if (xRotation)
                {
                    desiredRotation.x = -delta.y * xRotationSpeed;  // Invert the sign for x-axis rotation
                }
                if (yRotation)
                {
                    desiredRotation.y = -delta.x * yRotationSpeed;
                }
                // Smoothly interpolate current rotation towards desired rotation
                currentRotation = Vector3.SmoothDamp(currentRotation, currentRotation + desiredRotation, ref rotationVelocity, smoothTime);
                // Apply rotation to the object
                transform.eulerAngles = currentRotation;
                // Update the previous mouse position
                previousMousePosition = Input.mousePosition;
            }
        }
    }

    public void ResetAll()
    {
        gameObject.transform.rotation = Quaternion.identity;
        currentRotation = Vector3.zero;
    }


    public static bool IsOverUi()
    {
        _eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition,
        };

        _results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);

        return _results.Count > 0;
    }
}

