// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:46
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
public class CombatSystem : UI_BaseEventPanel
{
    [SerializeField] private UI_CombatRewardPanel rewardPanelRef;
    public CombatPlayerEntity player;
    public List<CombatComponentEntity> components = new List<CombatComponentEntity>();
    public List<CombatEnemyEntity> enemies = new List<CombatEnemyEntity>();
    private int rewardScraps;
    private float battleSpeed = 1.0f;

    private bool isBattling = false;
    private UI_CombatTooltipDetail tooltip;
   
    public delegate void GameEvent(bool value, int remainHp);
    public event GameEvent onBattleEnd;


    public void InitializeBattle(CombatPlayerEntity playerEntity, List<CombatEnemyEntity> enemyEntities, List<CombatComponentEntity> componentEntities)
    {
        if (tooltip == null) tooltip = UI_CombatTooltipDetail.Instance;

        player = playerEntity;
        components = componentEntities;
        enemies = enemyEntities;

        if (player != null)
        {
            player.OnDeath += HandleDeath;
        }

        foreach (var component in components)
        {
            component.OnAttackReady += HandleAttack;
        }

        foreach (var enemy in enemies)
        {
            enemy.OnAttackReady += HandleAttack;
            enemy.OnDeath += HandleDeath;
        }

        Debug.Log("[CombatSystem] Battle initialized. Ready to start.");
        StartBattle();
    }

    public void StartBattle()
    {
        Debug.Log("[CombatManager] Battle Start");
        isBattling = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isBattling == false) return;

        // Support custom battleSpeed without affecting Time.timescale that affect UI layer.
        // If overall game is affected by Time.deltaTime, then battleSpeed keep in 1.0f;
        float delta = Time.deltaTime * battleSpeed * OnSpeedToggle.SpeedMultiplier;

        if (player != null && !player.IsDead)
        {
            foreach (var component in components)
            {
                component.UpdateCombat(delta);
            }
        }

        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                enemy.UpdateCombat(delta);
        }
    }
    

    // Subscribed event from StartBattle(), instance.OnAttackReady return (CombatEntity this);
    private void HandleAttack(CombatEntity attacker)
    {
        if (attacker is CombatComponentEntity componentAttacker)
        {
            var target = enemies.Find(e => e != null && !e.IsDead);
            if (target != null)
            {
                componentAttacker.Attack(target);
            }
        }
        // Assume attacker == enemy
        else if (player != null && !player.IsDead)
        {
            attacker.Attack(player);
        }
    }

    private void HandleDeath(CombatEntity death)
    {
        death.OnAttackReady -= HandleAttack;
        death.OnDeath -= HandleDeath;
        ValidateEndCondition();

        // Fetch death entity's UI_Component
        UI_CombatEntityTooltipTrigger death_TooltipTrigger = death.GetComponent<UI_CombatEntityTooltipTrigger>();
        UI_CombatEntity death_CombatUI = death.GetComponentInChildren<UI_CombatEntity>();
        UnityEngine.UI.Image death_sprite = death.GetComponentInChildren<UnityEngine.UI.Image>();

        death_TooltipTrigger.enabled = false;
        death_CombatUI?.HideStatusBar();

        // Handle Visual effect logic for only that entity here
        // The HandleDeath function also shared by disable entities when combat end and disable train combat UI
        if (death.IsDead)
        {
            Sequence seq = DOTween.Sequence();

            seq.Join(death.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.15f).SetEase(Ease.OutCubic));
            seq.Join(death_sprite.DOFade(0.5f, 0.15f * 0.8f)); // fade in            
        }

    }

    private void ValidateEndCondition()
    {
        // Player lose condition
        if (player == null || player.IsDead)
        {
            EndBattle(false);
            return;
        }

        // Player win condition
        bool allEnemiesDead = true;
        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
        {
            EndBattle(true);
        } // Return player win
    }

    private int TotalScrapsFromEnemies()
    {
        int total = 0;
        foreach (CombatEnemyEntity enemy in enemies)
        {
            total += enemy.RewardScrapsCount;
        }
        return total;
    }

    private void EndBattle(bool playerWon)
    {
        Debug.Log("[CombatManager] Battle End. PlayerWon: " + playerWon);
        isBattling = false;
        int remainHp = player.CurrentHp;
        int rewardAmount = TotalScrapsFromEnemies();

        if (playerWon)
        {
            GiveReward(rewardAmount, () =>
            {
                PlayerStatusManager playerStatus = GameStateManager.Instance.playerStatus;
                playerStatus.RewardScraps(rewardAmount);

                CleanupBattle(remainHp, playerWon);
            });
        }
        else
        {
            CleanupBattle(remainHp, playerWon);
        }
    }

    private void CleanupBattle(int remainHp, bool playerWon)
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        enemies.Clear();

        onBattleEnd?.Invoke(playerWon, remainHp);
        tooltip.Hide();
    }

    private void GiveReward(int amount, Action onRewardComplete)
    {
        // Initialize the panel with the reward amount and callback
        rewardPanelRef.Setup(amount, onRewardComplete);
    }
    
}
