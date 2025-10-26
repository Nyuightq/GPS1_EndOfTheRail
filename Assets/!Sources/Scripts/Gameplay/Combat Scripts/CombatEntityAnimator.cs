// --------------------------------------------------------------
// Creation Date: 2025-10-22
// Author: nyuig
// Description: Sprite-based combat animation controller (time-driven, no coroutine)
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatEntityAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatEntity entity;
    [SerializeField] private Image imageRenderer;

    [Header("Animation Data")]
    public CombatAnimationClip animationClip;

    private float timer;
    private List<CombatAnimationClip.FrameData> currentFrames;
    private int currentIndex;
    private AnimPhase _phase = AnimPhase.Idle;

    private bool _attackTriggered = false;
    private float _attackElapsed = 0f;

    private void Awake()
    {
        if (entity == null) entity = GetComponent<CombatEntity>();
        if (imageRenderer == null)
            imageRenderer = GetComponentInChildren<Image>();

        // 🔹 Subscribe to event that occur when attack interval bar already cast and return to 0.0 sec.
        entity.OnAttackReady += TriggerAttackSequence;
    }

    private void OnDestroy()
    {
        if (entity != null)
            entity.OnAttackReady -= TriggerAttackSequence;
    }

    private void Update()
    {
        if (entity == null || animationClip == null || entity.IsDead)
            return;

        float remainToAttack = entity.AttackTakenTime - entity.RemainingAttackTimer;

        // If attack triggered: Attack → Recovery → Idle
        if (_attackTriggered)
        {
            _attackElapsed += Time.deltaTime;

            float attackDuration = animationClip.AttackDuration;
            float recoveryDuration = animationClip.RecoveryDuration;

            if (_attackElapsed < attackDuration)
            {
                SetPhase(AnimPhase.Attack, animationClip.attackSprites);
                PlayFrame_TimeDriven(_attackElapsed, attackDuration);
            }
            else if (_attackElapsed < attackDuration + recoveryDuration)
            {
                SetPhase(AnimPhase.Recovery, animationClip.recoverySprites);
                PlayFrame_TimeDriven(_attackElapsed - attackDuration, recoveryDuration);
            }
            else
            {
                // attack ended, return to Idle
                _attackTriggered = false;
                _attackElapsed = 0f;
                SetPhase(AnimPhase.Idle, animationClip.idleSprites);
                PlayFrame_TimeDriven(0f, animationClip.IdleDuration);
            }

            return;
        }

        // 🌀 Windup phase：if attack not triggered, remain time always be windup
        if (!_attackTriggered && remainToAttack <= animationClip.WindupDuration)
        {
            SetPhase(AnimPhase.Windup, animationClip.windupSprites);
            float windupElapsed = animationClip.WindupDuration - remainToAttack;
            PlayFrame_TimeDriven(windupElapsed, animationClip.WindupDuration);
        }
        else
        {
            // 💤 Idle (default)
            SetPhase(AnimPhase.Idle, animationClip.idleSprites);
            float idleTime = (Time.time % animationClip.IdleDuration);
            PlayFrame_TimeDriven(idleTime, animationClip.IdleDuration);
        }
    }

    // --------------------------------------------------------------
    // Animation System
    // --------------------------------------------------------------

    private void SetPhase(AnimPhase phaseName, CombatAnimationClip.FrameData[] frames)
    {
        if (_phase == phaseName) return;

        _phase = phaseName;
        currentFrames = new List<CombatAnimationClip.FrameData>(frames);
        currentIndex = 0;
        timer = 0f;
    }

    private void PlayFrame_TimeDriven(float elapsed, float totalDuration)
    {
        if (currentFrames == null || currentFrames.Count == 0) return;
        if (imageRenderer == null) return;

        float cumulative = 0f;
        for (int i = 0; i < currentFrames.Count; i++)
        {
            cumulative += currentFrames[i].duration;
            if (elapsed <= cumulative || i == currentFrames.Count - 1)
            {
                imageRenderer.sprite = currentFrames[i].sprite;
                return;
            }
        }
    }

    // --------------------------------------------------------------
    // Attack Sequence
    // --------------------------------------------------------------

    private void TriggerAttackSequence(CombatEntity e)
    {
        _attackTriggered = true;
        _attackElapsed = 0f;
    }

    // --------------------------------------------------------------
    // Enum
    // --------------------------------------------------------------
    private enum AnimPhase
    {
        Idle,
        Windup,
        Attack,
        Recovery
    }
}
