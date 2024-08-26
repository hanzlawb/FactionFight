using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Arena1Duplicate", LoadSceneMode.Additive);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
