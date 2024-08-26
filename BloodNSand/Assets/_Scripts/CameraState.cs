using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraState : MonoBehaviour
{
    public string _name;
    public Animator _animator;
    private void OnEnable()
    {
        _animator.Play(_name);
    }
    
}
