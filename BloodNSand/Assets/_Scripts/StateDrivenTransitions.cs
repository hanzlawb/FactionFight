using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDrivenTransitions : MonoBehaviour
{
   public Animator animator;
   public string _str;
    // Start is called before the first frame update
    void Start()
    {
    }
    private void OnEnable()
    {
        animator.Play(_str);

    }
}
