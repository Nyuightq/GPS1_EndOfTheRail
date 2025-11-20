using UnityEngine;
using DG.Tweening;

public class CameraIntroSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraMovementTemp cameraMovement;
    [SerializeField] private RailGridScript railGrid;
    [SerializeField] private BuildRails buildRail;

    [Header("UI Settings")]
    [Tooltip("Main gameplay Canvas that should be disabled during intro.")]
    [SerializeField] private Canvas gameplayCanvas;

    [Header("Intro Settings")]
    [Tooltip("How long the camera stays at the end point before moving to start.")]
    [SerializeField] private float holdTimeAtEnd = 2f;
    [Tooltip("How long it takes to move from end to start.")]
    [SerializeField] private float moveDuration = 2f;
    [Tooltip("How long the camera stays at start before giving control to the player.")]
    [SerializeField] private float holdTimeAtStart = 1.5f;

    [Header("Zoom Settings")]
    [Tooltip("How much to zoom in when focusing (smaller = closer).")]
    [SerializeField] private float zoomedSize = 3.5f;
    [Tooltip("Speed of zoom in/out transitions.")]
    [SerializeField] private float zoomSpeed = 2f;

    private Camera cam;
    private float originalSize;

    private void Start()
    {
        if (cameraMovement == null)
            cameraMovement = FindFirstObjectByType<CameraMovementTemp>();

        if (railGrid == null)
            railGrid = FindFirstObjectByType<RailGridScript>();

        if (buildRail == null)
            buildRail = FindFirstObjectByType<BuildRails>();

        cam = Camera.main;
        originalSize = cam.orthographicSize;

        // Disable player camera input for the intro.
        if (cameraMovement != null)
            CameraMovementTemp.ToggleCameraFollowMode(false);

        // Disable gameplay UI immediately
        if (gameplayCanvas != null)
            gameplayCanvas.enabled = false;

        buildRail.enabled = false;

        RunIntro();
    }

    private void RunIntro()
    {
        // Ensure start/end are valid
        if (railGrid.startPoint == new Vector3Int(500, 500, 500) ||
            railGrid.endPoint == new Vector3Int(500, 500, 500))
        {
            Debug.LogWarning("RailGrid start/end not initialized.");
            return;
        }

        Vector3 endWorld = railGrid.snapToGrid(railGrid.endPoint);
        Vector3 startWorld = railGrid.snapToGrid(railGrid.startPoint);

        // Clamp initial position
        Vector3 endPos = ClampPositionWithCamera(endWorld);
        Vector3 startPos = ClampPositionWithCamera(startWorld);

        // Force camera to end position first
        cam.transform.position = new Vector3(endPos.x, endPos.y, -10f);

        // ---- DOTWEEN SEQUENCE ----
        Sequence seq = DOTween.Sequence();

        // STEP 1: Hold on end
        seq.AppendInterval(holdTimeAtEnd);

        // STEP 2: Move END -> START
        seq.Append(
            cam.transform.DOMove(new Vector3(startPos.x, startPos.y, -10f), moveDuration)
            .SetEase(Ease.InOutSine)
        );

        // STEP 3: Enable gameplay UI
        seq.AppendCallback(() =>
        {
            if (gameplayCanvas != null)
            {
                gameplayCanvas.enabled = true;
                GameStateManager.Instance.InitialGeneralUI();
                buildRail.enabled = true;
            }
        });

        // STEP 4: Hold at start
        seq.AppendInterval(holdTimeAtStart);

        // STEP 5: Re-enable camera input
        seq.AppendCallback(() =>
        {
            CameraMovementTemp.ToggleCameraFollowMode(false);
            this.enabled = false;
        });
    }

    private Vector3 ClampPositionWithCamera(Vector3 worldTarget)
    {
        Vector3 candidate = new Vector3(worldTarget.x, worldTarget.y, -10f);
        if (cameraMovement != null)
            candidate = cameraMovement.ClampToBounds(candidate);
        return candidate;
    }
}
