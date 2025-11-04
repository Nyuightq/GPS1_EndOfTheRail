// --------------------------------------------------------------
// Creation Date: 2025-10-11 13:56
// Author: nyuig
// Description: attach to Hierarchy object [GameManager],
//              current GameManager.cs worked as TileMap utility function script
//              therefore create an actual GameManager in name [GameStateManager.cs]
// --------------------------------------------------------------
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// <para> Plan   = Enable [camera movement function, build rail, show UI]. </para>
/// <para> Travel = Enable [camera follow train].                           </para>
/// <para> Combat = Disable [configuration interaction].                    </para>
/// </summary>
public enum Phase
{
    Plan,
    Travel,
    Combat,

    Win,
    Lose
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // private CycleManager cycleManeger;

    public PlayerStatusManager playerStatus;
    private Phase _phase = Phase.Plan;
    private bool _pause = false;
    // private int _scraps;
    public static Phase CurrentPhase => Instance._phase;
    // public int Scraps => _scraps;

    // Managers Reference to disable and enable
    // private CameraManager _cameraManager;
    // private UIManager _uiManager;
    [SerializeField] private GameObject _railBuilderManager;
    [SerializeField] private GameObject _planPhasePanel;
    private RectTransform _planPhaseRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //
        _planPhaseRect = _planPhasePanel.GetComponent<RectTransform>();
        // DontDestroyOnLoad(gameObject);

        // Find Manager inside the scene
        // _cameraManager = FindObjectOfType<CameraManager>();
        // _uiManager = FindObjectOfType<UIManager>();
        // _railBuilderManager = FindObjectOfType<BuildRails>();
    }

    /// <summary>
    /// Used by Pause Menu in UI layer
    /// <para> Avoid using Time.deltaTime = 0, which potentially make UI no transition animation.</para>
    /// </summary>
    public static void SetPause(bool value)
    {
        Instance._pause = value;

        // Perform disable all module logic.
        // if (pause) disable all, else enable all + PhaseModulesController.
    }

    /// <summary>
    /// Trigger condition
    /// <para> Plan   = [Reached to rest tile]. </para>
    /// <para> Travel = [Validate path and generate train], [Combat ended]</para>
    /// <para> Combat = [CombatManager called battle]. </para>
    /// </summary>
    public static void SetPhase(Phase phase)
    {
        Instance._phase = phase;
        Instance.PhaseModulesController();
    }

    private void PhaseModulesController()
    {
        //if (_cameraManager == null) _cameraManager = Object.FindObjectOfType<CameraManager>();
        //if (_uiManager == null) _uiManager = Object.FindObjectOfType<UIManager>();
        // if(_railBuilderManager == null) _railBuilderManager = FindObjectOfType<BuildRails>();

        if (_phase == Phase.Plan)
        {
            // Disable camera follow train

            // Enable camera Move
            // Enable Planning phase related UI
            // Enable Rail building and hover UI
            _railBuilderManager.SetActive(true);
            TogglePlanPanel(true);
            CameraMovementTemp.ToggleCameraFollowMode(false);
        }
        else if (_phase == Phase.Travel)
        {
            _railBuilderManager.SetActive(false);
            TogglePlanPanel(false);
            CameraMovementTemp.ToggleCameraFollowMode(true);
            // Disable camera Move
            // Disable Planning phase related UI
            // Disable Rail building and hover UI

            // Enable camera follow train
        }


        if (_phase == Phase.Combat)
        {
            // Disable configuration interactive system
        }
        else
        {
            // Enable configuration interactive system
        }

        if (_phase == Phase.Win)
        {
            // Disable configuration interactive system
        }
        else
        {
            // Enable configuration interactive system
        }
        if (_phase == Phase.Lose)
        {
            // Disable configuration interactive system
        }
        else
        {
            // Enable configuration interactive system
        }
    }
    
    private void TogglePlanPanel(bool show)
    {
        // Ensure active if showing
        if (show)
        {
            _planPhasePanel.SetActive(true);
            _planPhaseRect.DOKill(); // stop previous tweens
            // _planPhaseRect.anchoredPosition = new Vector2(0, -200f); // start off-screen (adjust value as needed)
            _planPhaseRect.DOAnchorPosY(80f, 0.6f).SetEase(Ease.OutBack);
        }
        else
        {
            _planPhaseRect.DOKill();
            _planPhaseRect.DOAnchorPosY(0f, 0.6f).SetEase(Ease.InBack)
                .OnComplete(() => _planPhasePanel.SetActive(false));
        }
    }
}
