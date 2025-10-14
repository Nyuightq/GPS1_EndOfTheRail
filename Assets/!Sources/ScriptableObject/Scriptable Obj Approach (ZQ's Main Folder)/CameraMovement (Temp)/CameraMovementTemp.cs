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

    [Header("Boundary Settings")]
    [Tooltip("Reference to RailGridScript or any GameObject with a Tilemap.")]
    [SerializeField] private RailGridScript gridScript;

    private Vector3 moveDirection;
    private bool _isFollowing = false;
    public bool IsFollowing => _isFollowing;

    private float minX, maxX, minY, maxY;
    private bool hasBounds = false;
    private Camera cam;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    void Start()
    {
        // Try auto-assign gridScript if not assigned
        if (gridScript == null)
            gridScript = FindObjectOfType<RailGridScript>();

        if (gridScript != null)
            CalculateBoundsFromGrid();
    }

    void Update()
    {
        if (_isFollowing)
            FollowTarget();
        else
            HandleMovementInput();

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
        Vector3 smoothed = Vector3.Lerp(transform.position, targetPos, followSmoothSpeed * Time.deltaTime);
        transform.position = ClampPosition(smoothed);
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
        Vector3 newPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        transform.position = ClampPosition(newPos);
    }

    private void CalculateBoundsFromGrid()
    {
        if (gridScript == null) return;
        var tilemap = gridScript.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
        if (tilemap == null) return;

        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        Vector3 min = tilemap.CellToWorld(bounds.min);
        Vector3 max = tilemap.CellToWorld(bounds.max);

        minX = min.x;
        maxX = max.x;
        minY = min.y;
        maxY = max.y;

        hasBounds = true;
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        if (!hasBounds || cam == null) return pos;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        float clampedX = Mathf.Clamp(pos.x, minX + horzExtent, maxX - horzExtent);
        float clampedY = Mathf.Clamp(pos.y, minY + vertExtent, maxY - vertExtent);

        return new Vector3(clampedX, clampedY, pos.z);
    }
}
