
using UnityEngine;

public class EngineerManager : MonoBehaviour
{
    public static EngineerManager Instance { get; private set; }
    public static event System.Action OnEngineerClosed;

    [Header("Engineer UI")]
    [SerializeField] private GameObject engineerUIPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OpenEngineerUI(GameObject player = null)
    {
        engineerUIPanel.SetActive(true);
    }

    public void CloseEngineerUI()
    {
        engineerUIPanel.SetActive(false);

        // Broadcast to listeners
        OnEngineerClosed?.Invoke();
    }
}
