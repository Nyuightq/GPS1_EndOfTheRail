// --------------------------------------------------------------
// Creation Date: 2025-10-12
// Author: ZQlie
// Description: Basic 2D Camera Movement using W, A, S, D keys
// --------------------------------------------------------------
using UnityEngine;

public class CameraMovementTemp : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 moveDirection;

    void Update()
    {
        HandleMovementInput();
        MoveCamera();
    }

    private void HandleMovementInput()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) moveY = 1f;
        if (Input.GetKey(KeyCode.S)) moveY = -1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        moveDirection = new Vector3(moveX, moveY, 0f).normalized;
    }

    private void MoveCamera()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}

