// --------------------------------------------------------------
// Creation Date: 2025-10-22
// Author: nyuig
// Description: Sprite-based combat animation controller (time-driven, no coroutine)
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System;

[RequireComponent(typeof(CombatEntity))]
public class CombatEntityAnimator : MonoBehaviour
{
    private enum AnimPhase
    {
        Idle,
        Windup,
        Attack,
        Recovery
    }

    [Header("References")]
    private CombatEntity _entity;
    [SerializeField] private Image imageRenderer;

    [Header("Animation Data")]
    public CombatAnimationClip animationClip;

    private List<CombatAnimationClip.FrameData> currentFrames;
    private AnimPhase _phase = AnimPhase.Idle;

    private bool _attackTriggered = false;
    private bool _initial = true;
    private float _attackElapsed = 0f;

    private void Awake()
    {
        if (_entity == null) _entity = GetComponent<CombatEntity>();
        if (imageRenderer == null)
            imageRenderer = GetComponentInChildren<Image>();

        // 🔹 Subscribe to event that occur when attack interval bar already cast and return to 0.0 sec.
        _entity.OnAttackReady += TriggerAttackSequence;
        _entity.OnTakeDamage += TriggerOnHit;
        _entity.OnDeath += TriggerOnDeath;
        _attackTriggered = true;
    }

    private void Start()
    {
        if (animationClip != null)
        {
            Sprite firstFrame = animationClip.idleSprites[0].sprite;
            float spriteX = firstFrame.rect.width;
            float spriteY = firstFrame.rect.height;
            RectTransform imageRendererRect = imageRenderer.GetComponent<RectTransform>();
            imageRendererRect.sizeDelta = new Vector2(spriteX, spriteY);
        }
    }

    private void OnDestroy()
    {
        if (_entity != null)
        {
            _entity.OnAttackReady -= TriggerAttackSequence;
            _entity.OnTakeDamage -= TriggerOnHit;
            _entity.OnDeath -= TriggerOnDeath;
        }
    }

    private void Update()
    {
        if (_entity == null || animationClip == null || (_entity.IsDead && !_entity.IsComponent))
            return;

        float remainToAttack = _entity.AttackTakenTime - _entity.RemainingAttackTimer;

        // If attack triggered: Attack → Recovery → Idle
        if (_attackTriggered)
        {
            float attackDuration = animationClip.AttackDuration;
            float recoveryDuration = animationClip.RecoveryDuration;

            if (_entity.RemainingAttackTimer < attackDuration)
            {
                SetPhase(AnimPhase.Attack, animationClip.attackSprites);
                PlayFrame_TimeDriven(_entity.RemainingAttackTimer, attackDuration);
            }
            else if (_entity.RemainingAttackTimer < attackDuration + recoveryDuration)
            {
                SetPhase(AnimPhase.Recovery, animationClip.recoverySprites);
                PlayFrame_TimeDriven(_entity.RemainingAttackTimer - attackDuration, recoveryDuration);
            }
            else
            {
                // attack ended, return to Idle
                _attackTriggered = false;
                // _attackElapsed = 0f;
                SetPhase(AnimPhase.Idle, animationClip.idleSprites);
                PlayFrame_TimeDriven(0f, animationClip.IdleDuration);
            }

            if (_initial == true)
            {
                _attackTriggered = false;
                _initial = false;
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
            float idleTime = Time.time % animationClip.IdleDuration;
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
    // (EXTRA) Onhit doTween animation
    // --------------------------------------------------------------
    private void TriggerOnHit(CombatEntity e, int damageValue)
    {
        imageRenderer.rectTransform.DOKill();
        imageRenderer.DOKill();

        imageRenderer.rectTransform.anchoredPosition = Vector2.zero;
        imageRenderer.color = Color.white;

        float speedMult = 1f / OnSpeedToggle.SpeedMultiplier;

        Sequence hitSeq = DOTween.Sequence();

        hitSeq.Join(
            imageRenderer.rectTransform
                .DOShakeAnchorPos(0.2f * speedMult, new Vector2(4f, 0f), 10, 90f, false, true)
                .SetEase(Ease.InOutBack)
        );

        hitSeq.Join(
            imageRenderer
                .DOColor(Color.red, 0.1f * speedMult)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.Linear)
        );

        hitSeq.OnComplete(() =>
        {
            imageRenderer.rectTransform.anchoredPosition = Vector2.zero;
            imageRenderer.color = Color.white;
        });
    }

    private void TriggerOnDeath(CombatEntity e)
    {
        if (animationClip == null) return;
        SetPhase(AnimPhase.Idle, animationClip.idleSprites);
        PlayFrame_TimeDriven(0f, animationClip.IdleDuration);

        SoundManager.Instance.PlaySFX("SFX_Enemy_OnDeath");
    }
}
