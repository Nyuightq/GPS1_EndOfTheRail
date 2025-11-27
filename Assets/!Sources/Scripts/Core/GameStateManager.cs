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
    None,
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
    public bool IsPausing => _pause;
    [SerializeField] private bool _isInitial = false;
    [SerializeField] private TutorialCrystalManager _tutorialCrystalManager;
    // private int _scraps;
    public static Phase CurrentPhase => Instance._phase;
    // public int Scraps => _scraps;

    // Managers Reference to disable and enable
    // private CameraManager _cameraManager;
    // private UIManager _uiManager;
    [Header("Plan Phase UI")]
    [SerializeField] private GameObject _planPhasePanel;
    [SerializeField] private GameObject _planPhasePanel_2;
    private RectTransform _planPhaseRect;
    private RectTransform _planPhaseRect_2;
    [Header("General UI")]
    [SerializeField] private GameObject _topPanel;
    [SerializeField] private GameObject _asidePanel;
    private RectTransform _topPanelRect;
    private RectTransform _asidePanelRect;
    private BuildRails _buildRails;
    private GameObject _railBuilderManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);

        // Find Manager inside the scene
        // _cameraManager = FindObjectOfType<CameraManager>();
        // _uiManager = FindObjectOfType<UIManager>();
        // _railBuilderManager = FindObjectOfType<BuildRails>();
    }

    private void Start()
    {
        if (_planPhasePanel != null ) _planPhaseRect = _planPhasePanel.GetComponent<RectTransform>();
        if (_planPhasePanel_2 != null ) _planPhaseRect_2 = _planPhasePanel_2.GetComponent<RectTransform>();

        if (_topPanel != null ) _topPanelRect = _topPanel.GetComponent<RectTransform>();
        if (_asidePanel != null ) _asidePanelRect = _asidePanel.GetComponent<RectTransform>();

        BuildRails _buildRails = FindFirstObjectByType<BuildRails>();
        if (_buildRails != null) _railBuilderManager = _buildRails.gameObject;

        if (!_isInitial)
        {
            Vector2 pos = _topPanelRect.anchoredPosition;
            pos.y += 24f;
            _topPanelRect.anchoredPosition = pos;

            pos = _asidePanelRect.anchoredPosition;
            pos.x += 136f;
            _asidePanelRect.anchoredPosition = pos;

            _planPhasePanel.SetActive(false);
            _planPhasePanel_2.SetActive(false);

            _tutorialCrystalManager.onCloseEvent += ActivateInitialPlanPanel;
        }
    }

    public void InitialGeneralUI()
    {
        if (_isInitial) return;

        float duration = 0.6f;

        _topPanelRect.DOKill();
        _topPanelRect.DOAnchorPosY(-135f, duration)
            .SetEase(Ease.OutCubic);
        
        _asidePanelRect.DOKill();
        _asidePanelRect.DOAnchorPosX(0f, duration)
            .SetEase(Ease.OutCubic);

        _tutorialCrystalManager.ShowRewards();

        _isInitial = true;
    }

    private void ActivateInitialPlanPanel(bool value)
    {
        _tutorialCrystalManager.onCloseEvent -= ActivateInitialPlanPanel;
        SetPhase(Phase.Plan);
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
            //_buildRails.enabled = true;
            TogglePlanPanel(true);
            CameraMovementTemp.ToggleCameraFollowMode(false);
        }
        else if (_phase == Phase.Travel)
        {
            _railBuilderManager.SetActive(false);
            //_buildRails.enabled = false;
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
            _planPhaseRect.anchoredPosition = new Vector2(
                _planPhaseRect.anchoredPosition.x,
                -100f
            );
            _planPhasePanel.SetActive(true);
            _planPhaseRect.DOKill();
            _planPhaseRect.DOAnchorPosY(0f, 0.6f).SetEase(Ease.OutBack);


            _planPhaseRect_2.anchoredPosition = new Vector2(
                -420f,
                _planPhaseRect_2.anchoredPosition.y
            );
            _planPhasePanel_2.SetActive(true);
            _planPhaseRect_2.DOKill();
            _planPhaseRect_2.DOAnchorPosX(-480f, 0.6f).SetEase(Ease.OutBack);
        }
        else
        {
            _planPhaseRect.DOKill();
            _planPhaseRect.DOAnchorPosY(-100f, 0.6f).SetEase(Ease.InBack)
                .OnComplete(() => _planPhasePanel.SetActive(false));

            _planPhaseRect_2.DOKill();
            _planPhaseRect_2.DOAnchorPosX(-420f, 0.6f).SetEase(Ease.InBack)
                .OnComplete(() => _planPhasePanel_2.SetActive(false));

            InventoryGridScript inventory = GameManager.instance.inventoryScript;
            inventory.OnToggleInventoryState(true);
        }
    }
    public void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null; // OK because this is inside CombatManager
            Debug.Log("[GameStateManager] Instance destroyed for replay.");
        }
    }

}
