using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Transform _playerTransform;
    public Transform _playerIcon;
    public float _playerIconOffset = 1.5f;

    private void Start()
    {

    }

    void Update()
    {
        if (_playerTransform != null && _playerIcon != null)
        {
            // Match the sprite's position to the player's position
            _playerIcon.transform.position = new Vector3(_playerTransform.position.x, transform.position.y - _playerIconOffset, _playerTransform.position.z);

            // Calculate the desired rotation for the player icon
            Quaternion desiredRotation = Quaternion.Euler(90f, _playerTransform.eulerAngles.y, 0f);

            // Match the player icon's rotation to the desired rotation
            _playerIcon.rotation = desiredRotation;

        }
    }
}