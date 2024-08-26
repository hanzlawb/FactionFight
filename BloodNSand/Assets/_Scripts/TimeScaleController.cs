using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public float _timescale;
    private void OnEnable()
    {
        Time.timeScale = _timescale;
    }
    private void OnDisable()
    {
        Time.timeScale = 1.0f;

    }
}
