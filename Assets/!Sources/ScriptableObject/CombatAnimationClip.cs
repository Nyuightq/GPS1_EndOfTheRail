// --------------------------------------------------------------
// Creation Date: 2025-10-24
// Author: nyuig
// Description: Defines sprite-based animation data for combat entities
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(fileName = "CombatAnimationClip", menuName = "Combat/Animation Clip", order = 1)]
public class CombatAnimationClip : ScriptableObject
{
    [System.Serializable]
    public class FrameData
    {
        public Sprite sprite;
        [Tooltip("Duration in seconds this sprite should display")]
        public float duration = 0.1f;
    }

    [Header("Idle Animation")]
    public FrameData[] idleSprites;

    [Header("Windup Animation")]
    public FrameData[] windupSprites;

    [Header("Attack Animation")]
    public FrameData[] attackSprites;

    [Header("Recovery Animation")]
    public FrameData[] recoverySprites;

    /// <summary>
    /// Automatically calculates the total duration of a FrameData array.
    /// </summary>
    /// <param name="frames">Target frame array (e.g. idleSprites)</param>
    /// <returns>Total time in seconds</returns>
    public float GetTotalDuration(FrameData[] frames)
    {
        if (frames == null || frames.Length == 0)
            return 0f;

        float total = 0f;
        foreach (var f in frames)
        {
            if (f != null)
                total += Mathf.Max(0f, f.duration);
        }
        return total;
    }

    /// <summary>
    /// Quick access helpers for each animation type.
    /// </summary>
    public float IdleDuration => GetTotalDuration(idleSprites);
    public float WindupDuration => GetTotalDuration(windupSprites);
    public float AttackDuration => GetTotalDuration(attackSprites);
    public float RecoveryDuration => GetTotalDuration(recoverySprites);
}
