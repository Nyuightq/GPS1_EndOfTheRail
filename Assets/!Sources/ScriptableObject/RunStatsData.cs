// --------------------------------------------------------------
// Creation Date: 2025-10-31
// Description: ScriptableObject to store run statistics
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(fileName = "RunStatsData", menuName = "Game/Run Stats Data")]
public class RunStatsData : ScriptableObject
{
    [Header("Run Statistics")]
    public int finalHp;
    public int finalMaxHp;
    public int finalCrystalHp;
    public int finalMaxCrystalHp;
    public int finalScraps;
    public int daysPassed;
    public int combatsFaced;
    public int encountersFaced;
    public bool didPlayerWin;
    public string loseReason;

    /// <summary>
    /// Saves the current run statistics
    /// </summary>
    public void SaveRunStats(
        int hp, int maxHp,
        int crystalHp, int maxCrystalHp,
        int scraps, int days,
        int combats, int encounters,
        bool won, string reason = "")
    {
        finalHp = hp;
        finalMaxHp = maxHp;
        finalCrystalHp = crystalHp;
        finalMaxCrystalHp = maxCrystalHp;
        finalScraps = scraps;
        daysPassed = days;
        combatsFaced = combats;
        encountersFaced = encounters;
        didPlayerWin = won;
        loseReason = reason;

        Debug.Log($"[RunStatsData] Saved stats - HP: {hp}/{maxHp}, Crystal: {crystalHp}/{maxCrystalHp}, " +
                  $"Days: {days}, Combats: {combats + encounters}, Won: {won}");
    }

    /// <summary>
    /// Gets total combat encounters (standard + encounter types)
    /// </summary>
    public int GetTotalCombats()
    {
        return combatsFaced + encountersFaced;
    }

    /// <summary>
    /// Clears all saved stats (call when starting new run)
    /// </summary>
    public void ClearStats()
    {
        finalHp = 0;
        finalMaxHp = 0;
        finalCrystalHp = 0;
        finalMaxCrystalHp = 0;
        finalScraps = 0;
        daysPassed = 0;
        combatsFaced = 0;
        encountersFaced = 0;
        didPlayerWin = false;
        loseReason = "";
        
        Debug.Log("[RunStatsData] Stats cleared");
    }
}