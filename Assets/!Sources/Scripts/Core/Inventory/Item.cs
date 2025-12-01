// --------------------------------------------------------------
// Creation Date: 2025-10-20 03:26
// Author: User
// Description: -
// --------------------------------------------------------------
using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Item : MonoBehaviour
{
    public enum itemState
    {
        equipped,
        unequipped
    }

    public itemState state = itemState.unequipped;

    [SerializeField] private Image sprite;
    [SerializeField] public ItemSO itemData;
    public ItemEffect itemEffect;
    [SerializeField] private GameObject inventoryCellPrefab;
    [SerializeField] private float shapePreviewAlpha = 0.5f;

    public Effect[] effects;
    public event Action OnEquip, OnUnequip, OnBattleStart, OnUpdate, /*OnConditionTriggerOnce,*/ OnBattleEnd, OnBattleUpdate, OnAdjacentEquip, OnDayStart;

    public ItemShapeCell[,] itemShape { get; private set; }
    public RectTransform spriteRectTransform, itemRect;
    public List<GameObject> shape = new List<GameObject>();
    int shapeLocalHeight,shapeLocalWidth;

    int cellSize = GameManager.cellSize;

    private int baseLevel = 1;
    [SerializeField] private int maxLevel = 3;
    public int level
    {
        get => baseLevel;
        set => baseLevel = Mathf.Clamp(value, 1, maxLevel);
    }

    private Quaternion baseRotation = Quaternion.identity;
    private Tween shakeTween;

    public bool flaggedForDeletion;

    #region Unity LifeCycle
    private void OnEnable()
    {
        spriteRectTransform = sprite.GetComponent<RectTransform>();
    }

    #region event invokers
    public void TriggerEffectUnequip() { OnUnequip?.Invoke(); }
    public void TriggerEffectAdjacentEquip(){ if (this != null) OnAdjacentEquip?.Invoke(); }
    public void TriggerEffectEquip() { OnEquip?.Invoke();  }
    //public void TriggerEffectConditionOnce() { OnConditionTriggerOnce?.Invoke(); }
    public void TriggerEffectUpdate() { OnUpdate?.Invoke(); }
    public void TriggerEffectBattleStart() { OnBattleStart?.Invoke(); }
    public void TriggerEffectBattleEnd() { OnBattleEnd?.Invoke(); }
    public void TriggerEffectBattleUpdate() { OnBattleUpdate?.Invoke(); }
    public void TriggerEffectDayStart() { OnDayStart?.Invoke(); }
    #endregion

    private void Start()
    {
        effects = new Effect[itemData.effects.Length];
        for (int i = 0; i < itemData.effects.Length; i++)
        {
            // Create a copy of each effect (you'll need a Clone() method in Effect)
            effects[i] = itemData.effects[i].clone();
            effects[i].owner = this; // assign this Item as the owner
        }

        foreach (Effect effect in effects)
        {
            //effect.owner = this;
            switch (effect.trigger)
            {
                case triggers.OnUpdate:
                    OnUpdate += effect.apply;
                    break;
                case triggers.OnEquip:
                    OnEquip += effect.apply;
                    break;
                case triggers.OnEquipAndAdjacentEquip:
                    OnEquip += effect.apply;
                    OnAdjacentEquip += effect.apply;
                    break;
                case triggers.OnBattleStart:
                    OnBattleStart += effect.apply;
                    OnBattleEnd += effect.remove;
                    break;
                case triggers.OnBattleUpdate:
                    OnBattleUpdate += effect.apply;
                    OnBattleEnd += effect.remove;
                    break;
                case triggers.OnBattleEnd:
                    OnBattleEnd += effect.apply;
                    break;
                case triggers.OnDayStart:
                    OnDayStart += effect.apply;
                    break;
            }
            OnUnequip += effect.remove;
            if (effect.triggerOnConditionOnce) OnBattleEnd += effect.ResetTriggered;
        }

        //Get the item Shape 2D Array 
        itemShape = itemData.GetShapeGrid();

        if (itemData != null && itemData.itemSprite != null)
        {
            changeSprite(itemData.itemSprite);
            itemRect = GetComponent<RectTransform>();
            itemRect.sizeDelta = spriteRectTransform.sizeDelta;
        }
        
        GeneratePreview();
        spriteRectTransform.SetAsLastSibling();
        GetComponent<RectTransform>().SetAsLastSibling();
    }
    #endregion

    private void Update()
    {
        if (state == itemState.equipped)
        {
            OnUpdate?.Invoke();
        }

        if(state != itemState.equipped && GetComponent<ItemDragManager>().dragging == false)
        {
            if(shakeTween == null)
            {
                Vector3 shakeStrength = new Vector3(0f, 0f, 2f);
                float duration = 0.5f;
                shakeTween = spriteRectTransform.DOShakeRotation(duration, shakeStrength,10,90,false).SetLoops(-1,LoopType.Restart);
            }
        }
        else
        {
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();
                shakeTween = null;
                spriteRectTransform.localRotation = baseRotation; // reset rotation
            }
        }
    }

    private void GeneratePreview()
    {
        shapeLocalHeight = itemShape.GetLength(1) * cellSize;
        shapeLocalWidth = itemShape.GetLength(0) * cellSize;

        for (int x = 0; x < shapeLocalWidth / cellSize; x++)
        {
            for (int y = 0; y < shapeLocalHeight / cellSize; y++)
            {
                if (itemShape[x, y].filled)
                {
                    GameObject newCell = Instantiate(inventoryCellPrefab, GetComponent<RectTransform>());

                    newCell.GetComponent<DebugInventorySlot>().setInfo((int)x, (int)y);

                    RectTransform shapeRect = newCell.GetComponent<RectTransform>();

                    shapeRect.localScale = Vector3.one;
                    Image shapeImage = newCell.GetComponent<Image>();
                    shapeImage.color = new Color(shapeImage.color.r,shapeImage.color.g,shapeImage.color.b,shapePreviewAlpha);

                    shapeRect.anchorMin = new Vector2(0f, 1f);
                    shapeRect.anchorMax = new Vector2(0f, 1f);
                    shapeRect.pivot = new Vector2(0.5f, 0.5f);

                    //get the position and offset to the centre of that cell
                    Vector2 cellPos = new Vector2(x * cellSize + cellSize * 0.5f, -y * cellSize - cellSize * 0.5f);
                    shapeRect.anchoredPosition = cellPos;

                    shape.Add(newCell);
                    //Debug.Log("added new cell");
                }
            }
        }
        //Make the Sprite of the item appear infront
        spriteRectTransform.SetAsLastSibling();
    }

    public void ShowPreview(bool showPreview)
    {
        foreach (GameObject cellPreview in shape)
        {
            cellPreview.SetActive(showPreview);
        }
    }

    public void RotateShape(ItemShapeCell[,] currentItemShape)
    {
        int shapeHeight = currentItemShape.GetLength(1);
        int shapeWidth = currentItemShape.GetLength(0);

        ItemShapeCell[,] rotatedShape = new ItemShapeCell[shapeHeight, shapeWidth];

        for (int x = 0; x < shapeWidth; x++)
        {
            for (int y = 0; y < shapeHeight; y++)
            {
                rotatedShape[shapeHeight - 1 - y, x] = currentItemShape[x, y];
            }
        }
        
        RefreshShape(rotatedShape);

        baseRotation *= Quaternion.Euler(0, 0, -90f);
        spriteRectTransform.localRotation = baseRotation;

        //resize item rect to match rotated sprite
        RectTransform itemRect = GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.y,itemRect.sizeDelta.x);

        Debug.Log("rotated!!");
        //return rotatedShape;
    }

    public void RefreshShape(ItemShapeCell[,] newShape)
    {
        foreach (GameObject cell in shape)
        {
            Destroy(cell);
        }
        shape.Clear();
        itemShape = newShape;
        GeneratePreview();
        Debug.Log("REFRESHED!");
    }

    public void changeSprite(Sprite newSprite)
    {
        sprite.sprite = newSprite;
        sprite.SetNativeSize();
    }

    public void PrepareDeletion()
    {
        if (flaggedForDeletion || itemData.mandatoryItem) return;
        flaggedForDeletion = true;

        float fadeDuration = 0.5f;

        sprite.color = Color.red;
        Tween fadeTween = sprite.DOFade(0, fadeDuration);
        fadeTween.OnComplete(OnDeletionComplete);
    }

    private void OnDeletionComplete()
    {
        OnEquip = null;
        OnUnequip = null;
        OnBattleStart = null;
        OnBattleEnd = null;
        OnUpdate = null;
        OnBattleUpdate = null;
        OnAdjacentEquip = null;
        OnDayStart = null;

        Destroy(gameObject);
    }
    
}
