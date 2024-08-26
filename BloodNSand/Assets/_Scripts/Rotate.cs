using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool XAxis = false;
    public bool YAxis = false;
    public bool ZAxis = false;

    public float speed = 25f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (XAxis)
        {
            transform.Rotate(1f * speed * Time.deltaTime, 0, 0);
        }
        if (YAxis)
        {
            transform.Rotate(0, 1f * speed * Time.deltaTime, 0);
        }
        if (ZAxis)
        {
            transform.Rotate(0, 0, 1f * speed * Time.deltaTime);
        }
        
    }
}
