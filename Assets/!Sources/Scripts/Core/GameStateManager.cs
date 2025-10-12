// --------------------------------------------------------------
// Creation Date: 2025-10-11 13:56
// Author: nyuig
// Description: attach to Hierarchy object [GameManager],
//              current GameManager.cs worked as TileMap utility function script
//              therefore create an actual GameManager in name [GameStateManager.cs]
// --------------------------------------------------------------
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
    Combat
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // private CycleManager cycleManeger;

    private PlayerStatusManager playerStatus;
    private Phase _phase = Phase.Plan;
    private bool _pause = false;
    // private int _scraps;
    public static Phase CurrentPhase => Instance._phase;
    // public int Scraps => _scraps;

    // Managers Reference to disable and enable
    // private CameraManager _cameraManager;
    // private UIManager _uiManager;
    [SerializeField] private GameObject _railBuilderManager;

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
        }
        else if (_phase == Phase.Travel)
        {
            _railBuilderManager.SetActive(false);
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
    }
}
