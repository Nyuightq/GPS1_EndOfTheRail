using UnityEngine;
using System.Collections;

public class CameraIntroSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraMovementTemp cameraMovement;
    [SerializeField] private RailGridScript railGrid;
    [SerializeField] private BuildRails buildRail;
    [SerializeField] private GameObject _railBuilderManager;

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

    // [Header("Zoom Settings")]
    // [Tooltip("How much to zoom in when focusing (smaller = closer).")]
    // [SerializeField] private float zoomedSize = 3.5f;
    // [Tooltip("Speed of zoom in/out transitions.")]
    // [SerializeField] private float zoomSpeed = 2f;

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

        if (buildRail != null && _railBuilderManager != null) _railBuilderManager = buildRail.gameObject;

        cam = Camera.main;
        originalSize = cam.orthographicSize;

        // Disable player camera input for the intro.
        if (cameraMovement != null)
            CameraMovementTemp.ToggleCameraFollowMode(true);

        // Disable gameplay UI immediately
        if (gameplayCanvas != null)
            gameplayCanvas.enabled = false;

        _railBuilderManager.SetActive(false);

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // Wait for railGrid to initialize start/end (avoid using Vector3Int.zero if you use a different default)
        yield return new WaitUntil(() => railGrid != null &&
            railGrid.startPoint != new Vector3Int(500, 500, 500) &&
            railGrid.endPoint != new Vector3Int(500, 500, 500));

        Vector3 endWorld = railGrid.snapToGrid(railGrid.endPoint);
        Vector3 startWorld = railGrid.snapToGrid(railGrid.startPoint);

        // --- STEP 1: Focus on END with zoom-in ---
        cam.transform.position = ClampPositionWithCamera(endWorld);
        // --- STEP 3: Pan END -> START ---
        yield return new WaitForSeconds(holdTimeAtEnd);
        yield return StartCoroutine(PanToPosition(new Vector3(startWorld.x, startWorld.y, cam.transform.position.z), moveDuration));

        // --- STEP 4: Enable UI Canvas once movement is done ---
        if (gameplayCanvas != null)
        {
            gameplayCanvas.enabled = true;
            Debug.Log("GameStateManager.Instance > " + GameStateManager.Instance != null + " ");
            GameStateManager.Instance.InitialGeneralUI();
        }

        yield return new WaitForSeconds(holdTimeAtStart);
        // --- STEP 7: Re-enable normal camera movement ---
        CameraMovementTemp.ToggleCameraFollowMode(false);
        this.enabled = false;
    }

    // Pan while clamping each frame using CameraMovementTemp's ClampToBounds
    private IEnumerator PanToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 fromPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 candidate = Vector3.Lerp(fromPos, targetPosition, t);

            if (cameraMovement != null)
                candidate = cameraMovement.ClampToBounds(candidate);

            cam.transform.position = new Vector3(candidate.x, candidate.y, cam.transform.position.z);
            yield return null;
        }

        Vector3 final = targetPosition;
        if (cameraMovement != null)
            final = cameraMovement.ClampToBounds(final);
        cam.transform.position = new Vector3(final.x, final.y, cam.transform.position.z);
    }

    private Vector3 ClampPositionWithCamera(Vector3 worldTarget)
    {
        Vector3 candidate = new Vector3(worldTarget.x, worldTarget.y, cam.transform.position.z);
        if (cameraMovement != null)
            candidate = cameraMovement.ClampToBounds(candidate);
        return candidate;
    }
}
