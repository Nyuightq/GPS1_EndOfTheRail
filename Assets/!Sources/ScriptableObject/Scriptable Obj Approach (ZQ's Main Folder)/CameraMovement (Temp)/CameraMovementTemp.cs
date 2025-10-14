// --------------------------------------------------------------
// Creation Date: 2025-10-12
// Author: ZQlie
// Description: Basic 2D Camera Movement using W, A, S, D keys
// --------------------------------------------------------------
using UnityEngine;

public class CameraMovementTemp : MonoBehaviour
{
    public static CameraMovementTemp Instance;
    [Header("Camera Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float followSmoothSpeed = 5f;

    [Header("Follow Target Settings")]
    [Tooltip("Optional: Manually assign target. If null, will auto-find by component type.")]
    [SerializeField] private Transform followTarget;

    private Vector3 moveDirection;
    private bool _isFollowing = false;
    public bool IsFollowing => _isFollowing;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_isFollowing)
        {
            FollowTarget();
        }
        else
        {
            HandleMovementInput();
        }
        MoveCamera();
    }

    public static void ToggleCameraFollowMode(bool value)
    {
        Instance._isFollowing = value;
    }

    private void FollowTarget()
    {
        if (followTarget == null)
        {
            TrainMovement train = FindObjectOfType<TrainMovement>();
            if (train != null)
                followTarget = train.transform;
        }
        
        Vector3 targetPos = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSmoothSpeed * Time.deltaTime);
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

