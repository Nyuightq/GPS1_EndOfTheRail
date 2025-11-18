using UnityEngine;
using System.Collections;

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
            

        railGrid.enabled = false;
        buildRail.enabled = false;
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
        yield return StartCoroutine(SmoothZoom(cam.orthographicSize, zoomedSize));
        yield return new WaitForSeconds(holdTimeAtEnd);

        // --- STEP 2: Zoom back out to original size ---
        yield return StartCoroutine(SmoothZoom(cam.orthographicSize, originalSize));

        // --- STEP 3: Pan END -> START ---
        yield return StartCoroutine(PanToPosition(new Vector3(startWorld.x, startWorld.y, cam.transform.position.z), moveDuration));

        // --- STEP 4: Enable UI Canvas once movement is done ---
        if (gameplayCanvas != null)
        {
            gameplayCanvas.enabled = true;
            railGrid.enabled = true;
            buildRail.enabled = true;
        }

        // --- STEP 5: Zoom in on START ---
        yield return StartCoroutine(SmoothZoom(cam.orthographicSize, zoomedSize));
        yield return new WaitForSeconds(holdTimeAtStart);

        // --- STEP 6: Restore original zoom ---
        yield return StartCoroutine(SmoothZoom(cam.orthographicSize, originalSize));

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

    private IEnumerator SmoothZoom(float fromSize, float toSize)
    {
        float t = 0f;
        float duration = Mathf.Max(0.01f, 1f / zoomSpeed);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.orthographicSize = Mathf.Lerp(fromSize, toSize, t);
            yield return null;
        }

        cam.orthographicSize = toSize;
    }
}
