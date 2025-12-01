// --------------------------------------------------------------
// Creation Date: 2025-10-12
// Author: ZQlie
// Description: Basic 2D Camera Movement using W, A, S, D keys
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraMovementTemp : MonoBehaviour
{
    public static CameraMovementTemp Instance;

    [Header("Camera Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float followSmoothSpeed = 5f;
    [SerializeField] private float focusSmoothSpeed = 8f; // Speed for focusing on stationary target

    [Header("Follow Target Settings")]
    [Tooltip("Optional, Manually assign target.")]
    [SerializeField] private Transform followTarget;

    [Header("Boundary Settings")]
    [Tooltip("Reference to RailGridScript")]
    [SerializeField] private RailGridScript gridScript;

    [Header("UI Padding (in tiles)")]
    [Tooltip("Number of tiles to subtract from each edge to account for UI overlay")]
    [SerializeField] private int leftPaddingTiles = 2;
    [SerializeField] private int rightPaddingTiles = 6;
    [SerializeField] private int topPaddingTiles = 3;
    [SerializeField] private int bottomPaddingTiles = 3;

    private Vector3 moveDirection;
    private bool _isFollowing = false;
    public bool IsFollowing => _isFollowing;

    // Focus mode variables
    private bool _isFocusing = false;
    private Vector3 _focusTargetPosition;
    private float _focusThreshold = 0.01f; // Distance threshold to stop focusing

    private float minX, maxX, minY, maxY;
    private bool hasBounds = false;
    private Camera cam;
    private float tileSize = 1f; // Assuming 1 unit per tile, adjust if different

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    void Start()
    {
        // Try auto-assign gridScript if not assigned
        if (gridScript == null)
            gridScript = FindFirstObjectByType<RailGridScript>();

        if (gridScript != null)
            CalculateBoundsFromLargestTilemap();
    }

    void Update()
    {
        if (_isFocusing)
        {
            FocusOnTarget();
        }
        else if (_isFollowing)
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

    /// <summary>
    /// Smoothly moves the camera to focus on the train's current position
    /// </summary>
    /// <param name="train">Optional: specific train transform to focus on. If null, will find TrainMovement automatically</param>
    public void FocusCameraOnTrain(Transform train = null)
    {
        Transform targetTrain = train;
        
        // If no train provided, find it automatically
        if (targetTrain == null)
        {
            TrainMovement trainMovement = FindFirstObjectByType<TrainMovement>();
            if (trainMovement != null)
                targetTrain = trainMovement.transform;
        }

        if (targetTrain == null)
        {
            Debug.LogWarning("[CameraMovementTemp] No train found to focus on!");
            return;
        }

        // Set focus target position
        _focusTargetPosition = new Vector3(targetTrain.position.x, targetTrain.position.y, transform.position.z);
        _isFocusing = true;
        _isFollowing = false; // Disable follow mode when focusing

        Debug.Log($"[CameraMovementTemp] Focusing camera on train at position: {_focusTargetPosition}");
    }

    /// <summary>
    /// Static method to call focus from other scripts
    /// </summary>
    public static void FocusOnTrain(Transform train = null)
    {
        if (Instance != null)
            Instance.FocusCameraOnTrain(train);
    }

    /// <summary>
    /// Stops the camera from focusing and allows manual control
    /// </summary>
    public void StopFocusing()
    {
        _isFocusing = false;
    }

    private void FocusOnTarget()
    {
        Vector3 currentPos = transform.position;
        Vector3 targetPos = ClampPosition(_focusTargetPosition);
        
        // Smoothly lerp to target position
        Vector3 newPos = Vector3.Lerp(currentPos, targetPos, focusSmoothSpeed * Time.deltaTime);
        transform.position = newPos;

        // Stop focusing when close enough to target
        if (Vector3.Distance(currentPos, targetPos) < _focusThreshold)
        {
            transform.position = targetPos;
            _isFocusing = false;
            Debug.Log("[CameraMovementTemp] Focus complete");
        }
    }

    private void FollowTarget()
    {
        if (followTarget == null)
        {
            TrainMovement train = FindFirstObjectByType<TrainMovement>();
            if (train != null)
                followTarget = train.transform;
        }

        if (followTarget == null) return;

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
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPausing) moveDirection = new Vector3(0f, 0f, 0f).normalized;
        
        // Stop focusing if player manually moves camera
        if (moveDirection != Vector3.zero)
        {
            _isFocusing = false;
        }
    }

    private void MoveCamera()
    {
        if (!_isFocusing) // Only apply manual movement when not focusing
        {
            Vector3 newPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;
            transform.position = ClampPosition(newPos);
        }
    }

    private void CalculateBoundsFromLargestTilemap()
    {
        if (gridScript == null) return;
        Tilemap[] tilemaps = gridScript.GetComponentsInChildren<Tilemap>();
        if (tilemaps == null || tilemaps.Length == 0) return;

        BoundsInt largestBounds = tilemaps[0].cellBounds;
        Tilemap largestTilemap = tilemaps[0];

        // Find the tilemap with largest world size
        foreach (var tm in tilemaps)
        {
            tm.CompressBounds();
            if (tm.cellBounds.size.x * tm.cellBounds.size.y > largestBounds.size.x * largestBounds.size.y)
            {
                largestBounds = tm.cellBounds;
                largestTilemap = tm;
            }
        }

        // Get cell size from the tilemap
        tileSize = largestTilemap.cellSize.x; // Assuming square tiles

        Vector3 min = largestTilemap.CellToWorld(largestBounds.min);
        Vector3 max = largestTilemap.CellToWorld(largestBounds.max);

        // Apply UI padding (convert tiles to world units)
        minX = min.x + (leftPaddingTiles * tileSize);
        maxX = max.x - (rightPaddingTiles * tileSize);
        minY = min.y + (bottomPaddingTiles * tileSize);
        maxY = max.y - (topPaddingTiles * tileSize);

        hasBounds = true;

        Debug.Log($"[CameraMovementTemp] Using bounds from largest Tilemap: {largestTilemap.name}");
        Debug.Log($"[CameraMovementTemp] Tile Size: {tileSize}, Applied Padding - L:{leftPaddingTiles} R:{rightPaddingTiles} T:{topPaddingTiles} B:{bottomPaddingTiles}");
        Debug.Log($"[CameraMovementTemp] Final Bounds - X:[{minX}, {maxX}], Y:[{minY}, {maxY}]");
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        if (!hasBounds || cam == null) return pos;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        // Clamp camera center position so viewport stays within padded bounds
        float clampedX = Mathf.Clamp(pos.x, minX + horzExtent, maxX - horzExtent);
        float clampedY = Mathf.Clamp(pos.y, minY + vertExtent, maxY - vertExtent);

        return new Vector3(clampedX, clampedY, pos.z);
    }

    public Vector3 ClampToBounds(Vector3 pos)
    {
        return ClampPosition(pos);
    }

    // Helper method to visualize bounds in Scene view
    private void OnDrawGizmos()
    {
        if (!hasBounds) return;

        // Draw the playable area (green)
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);

        // Draw corner markers
        Gizmos.color = Color.yellow;
        float markerSize = 0.5f;
        Gizmos.DrawWireSphere(new Vector3(minX, minY, 0f), markerSize);
        Gizmos.DrawWireSphere(new Vector3(maxX, minY, 0f), markerSize);
        Gizmos.DrawWireSphere(new Vector3(minX, maxY, 0f), markerSize);
        Gizmos.DrawWireSphere(new Vector3(maxX, maxY, 0f), markerSize);

        // Draw focus target if focusing
        if (_isFocusing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_focusTargetPosition, 0.5f);
        }
    }
}