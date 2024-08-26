using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        // Get the direction from the object to the camera
        Vector3 direction = Camera.main.transform.position - transform.position;

        // Ignore the Y component of the direction
        direction.y = 0;

        // If the direction is not zero, rotate the object to face the camera
        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
    }
}
