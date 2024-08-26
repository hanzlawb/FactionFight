using UnityEngine;

public class UpdateShaderPlayerPosition : MonoBehaviour
{
    public Material shieldMaterial;
    public Transform playerTransform;

    void Update()
    {
        if (shieldMaterial != null && playerTransform != null)
        {
            shieldMaterial.SetVector("_PlayerPosition", playerTransform.position);
        }
    }
}
