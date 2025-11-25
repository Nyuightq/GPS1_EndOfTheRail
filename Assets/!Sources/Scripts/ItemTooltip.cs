using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform tooltipRectTransform;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    [Header("Positioning & Clamping")]
    [SerializeField] private Vector3 offset = new Vector3(20, -20, 0);
    [SerializeField] private float padding = 5f;

    private Item _currentItem;
    private Canvas canvas;
    private Canvas tooltipCanvas;

    private void Awake()
    {
        // Enforce Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get necessary references
        canvas = GetComponentInParent<Canvas>();
        if (!tooltipRectTransform)
        {
            tooltipRectTransform = GetComponent<RectTransform>();
        }

        // Setup tooltip canvas for highest rendering priority
        SetupTooltipCanvas();

        Hide();
    }

    private void Update()
    {
        if (tooltipRectTransform.gameObject.activeSelf)
        {
            // Follow mouse and clamp to screen bounds
            FollowMouseAndClamp();
        }
    }

    /// <summary>
    /// Shows the tooltip with the given item's data
    /// </summary>
    public void Show(Item item)
    {
        if (item == null || item.itemData == null)
        {
            Debug.LogWarning("ItemTooltip.Show: Item or itemData is null");
            Hide();
            return;
        }

        Debug.Log($"<color=green>ItemTooltip.Show called for: {item.itemData.itemName}</color>");

        _currentItem = item;
        UpdateTooltipContent();

        // Activate the tooltip
        gameObject.SetActive(true);

        Debug.Log($"<color=green>Tooltip activated! Content: {itemDescriptionText.text}</color>");

        // Force layout rebuild to ensure proper sizing for clamping
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);

        // Set initial position and clamp
        FollowMouseAndClamp();
    }

    /// <summary>
    /// Hides the tooltip
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        _currentItem = null;
    }

    /// <summary>
    /// Updates the tooltip text content from the current item
    /// </summary>
    private void UpdateTooltipContent()
    {
        if (_currentItem == null || _currentItem.itemData == null) return;

        Debug.Log($"<color=cyan>UpdateTooltipContent for: {_currentItem.itemData.itemName}</color>");
        Debug.Log($"<color=cyan>Effects count: {(_currentItem.itemData.effects != null ? _currentItem.itemData.effects.Length : 0)}</color>");

        // Set item name with styling
        itemNameText.text = $"<size=110%><b>{_currentItem.itemData.itemName}</b></size>";

        // Build description from effects
        StringBuilder descBuilder = new StringBuilder();

        // Parse effects and display their stats
        if (_currentItem.itemData.effects != null && _currentItem.itemData.effects.Length > 0)
        {
            for (int i = 0; i < _currentItem.itemData.effects.Length; i++)
            {
                Effect effect = _currentItem.itemData.effects[i];
                
                if (effect == null) 
                {
                    Debug.LogWarning("Null effect found in effects array");
                    continue;
                }

                Debug.Log($"<color=yellow>Processing effect {i}: {effect.GetType().Name}</color>");

                string effectDescription = GetEffectDescription(effect);
                if (!string.IsNullOrEmpty(effectDescription))
                {
                    if (i > 0 && descBuilder.Length > 0)
                        descBuilder.AppendLine(); // Add spacing between effects
                    
                    descBuilder.Append(effectDescription);
                    Debug.Log($"<color=yellow>Effect description added: {effectDescription}</color>");
                }
            }
        }
        else
        {
            descBuilder.AppendLine("<color=#888888><i>No effects</i></color>");
            Debug.LogWarning("No effects found on item");
        }

        itemDescriptionText.text = descBuilder.ToString().TrimEnd();
        Debug.Log($"<color=cyan>Final tooltip text: {itemDescriptionText.text}</color>");
    }

    /// <summary>
    /// Generates a readable description based on effect type
    /// </summary>
    private string GetEffectDescription(Effect effect)
    {
        if (effect == null) return string.Empty;

        StringBuilder effectDesc = new StringBuilder();
        Type effectType = effect.GetType();

        Debug.Log($"<color=magenta>GetEffectDescription for type: {effectType.Name}</color>");

        // Handle each effect type
        if (effect is WeaponSpawnEffect weaponEffect)
        {
            effectDesc.Append(GetWeaponSpawnDescription(weaponEffect));
        }
        else if (effect is HealEffect healEffect)
        {
            effectDesc.Append(GetHealEffectDescription(healEffect));
        }
        else if (effect is BuffStatEffect buffEffect)
        {
            effectDesc.Append(GetBuffStatDescription(buffEffect));
        }
        else if (effect is AdjacentWeaponBoosterEffect boosterEffect)
        {
            effectDesc.Append(GetAdjacentBoosterDescription(boosterEffect));
        }
        else
        {
            // Fallback for unknown effect types
            string effectTypeName = effectType.Name;
            if (effectTypeName.EndsWith("Effect"))
                effectTypeName = effectTypeName.Substring(0, effectTypeName.Length - 6);
            
            effectDesc.AppendLine($"<color=#FFA500><b>{FormatCamelCase(effectTypeName)}</b></color>");
        }

        // Add conditions if present
        if (effect.conditions != null && effect.conditions.Length > 0)
        {
            string conditionsText = GetConditionsDescription(effect.conditions);
            if (!string.IsNullOrEmpty(conditionsText))
            {
                effectDesc.Append(conditionsText);
            }
        }

        string result = effectDesc.ToString();
        Debug.Log($"<color=magenta>Generated description: {result}</color>");
        return result;
    }

    /// <summary>
    /// Gets description for WeaponSpawnEffect using reflection
    /// </summary>
    private string GetWeaponSpawnDescription(WeaponSpawnEffect effect)
    {
        StringBuilder desc = new StringBuilder();
        Type effectType = effect.GetType();

        var weaponNameField = effectType.GetField("weaponName", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseDamageField = effectType.GetField("baseAttackDamage", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseSpeedField = effectType.GetField("baseAttackSpeed", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseVarianceField = effectType.GetField("baseAttackVariance", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (weaponNameField != null)
        {
            string weaponName = weaponNameField.GetValue(effect)?.ToString() ?? "Unknown Weapon";
            desc.AppendLine($"<color=#FFA500><b>{weaponName}</b></color>");
        }

        if (baseDamageField != null)
        {
            int damage = Convert.ToInt32(baseDamageField.GetValue(effect) ?? 0);
            int actualDamage = damage * _currentItem.level;
            desc.AppendLine($"<color=#C23753>Damage: {actualDamage}</color>");
        }

        if (baseSpeedField != null)
        {
            int speed = Convert.ToInt32(baseSpeedField.GetValue(effect) ?? 0);
            int actualSpeed = speed * _currentItem.level;
            string colorCode = actualSpeed <= 2 ? "#C23753" : actualSpeed >= 6 ? "#EBB85B" : "#FFFFFF";
            desc.AppendLine($"<color={colorCode}>Attack Speed: {actualSpeed}</color>");
        }

        if (baseVarianceField != null)
        {
            int variance = Convert.ToInt32(baseVarianceField.GetValue(effect) ?? 0);
            int actualVariance = variance * _currentItem.level;
            if (actualVariance > 0)
                desc.AppendLine($"<color=#88AAFF>Variance: {actualVariance}</color>");
        }

        return desc.ToString();
    }

    /// <summary>
    /// Gets description for HealEffect using reflection
    /// </summary>
    private string GetHealEffectDescription(HealEffect effect)
    {
        StringBuilder desc = new StringBuilder();
        Type effectType = effect.GetType();

        var inputTypeField = effectType.GetField("inputType", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var healAmountField = effectType.GetField("healAmount", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var usesField = effectType.GetField("uses", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (healAmountField != null)
        {
            float healAmount = Convert.ToSingle(healAmountField.GetValue(effect) ?? 0f);
            InputType inputType = InputType.flat;
            
            if (inputTypeField != null)
            {
                inputType = (InputType)(inputTypeField.GetValue(effect) ?? InputType.flat);
            }

            if (inputType == InputType.percentage)
            {
                float actualHeal = healAmount * _currentItem.level;
                desc.AppendLine($"<color=#77FF77><b>Heal: {actualHeal}% Max HP</b></color>");
            }
            else
            {
                int actualHeal = Mathf.FloorToInt(healAmount * _currentItem.level);
                desc.AppendLine($"<color=#77FF77><b>Heal: +{actualHeal} HP</b></color>");
            }
        }

        if (usesField != null)
        {
            int uses = Convert.ToInt32(usesField.GetValue(effect) ?? 0);
            if (uses > 0)
                desc.AppendLine($"<color=#AAAAAA>Uses: {uses}</color>");
        }

        return desc.ToString();
    }

    /// <summary>
    /// Gets description for BuffStatEffect using reflection
    /// </summary>
    private string GetBuffStatDescription(BuffStatEffect effect)
    {
        StringBuilder desc = new StringBuilder();
        Type effectType = effect.GetType();

        var inputTypeField = effectType.GetField("inputType", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var buffAmountField = effectType.GetField("buffAmount", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var statTypeField = effectType.GetField("statType", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        string statName = "Stat";
        if (statTypeField != null)
        {
            object statValue = statTypeField.GetValue(effect);
            if (statValue != null)
                statName = FormatCamelCase(statValue.ToString());
        }

        if (buffAmountField != null)
        {
            float buffAmount = Convert.ToSingle(buffAmountField.GetValue(effect) ?? 0f);
            InputType inputType = InputType.flat;
            
            if (inputTypeField != null)
            {
                inputType = (InputType)(inputTypeField.GetValue(effect) ?? InputType.flat);
            }

            if (inputType == InputType.percentage)
            {
                float actualBuff = buffAmount * _currentItem.level;
                string sign = actualBuff > 0 ? "+" : "";
                string color = actualBuff > 0 ? "#77FF77" : "#FF7777";
                desc.AppendLine($"<color={color}><b>{statName}: {sign}{actualBuff}%</b></color>");
            }
            else
            {
                int actualBuff = Mathf.FloorToInt(buffAmount * _currentItem.level);
                string sign = actualBuff > 0 ? "+" : "";
                string color = actualBuff > 0 ? "#77FF77" : "#FF7777";
                desc.AppendLine($"<color={color}><b>{statName}: {sign}{actualBuff}</b></color>");
            }
        }

        return desc.ToString();
    }

    /// <summary>
    /// Gets description for AdjacentWeaponBoosterEffect using reflection
    /// </summary>
    private string GetAdjacentBoosterDescription(AdjacentWeaponBoosterEffect effect)
    {
        StringBuilder desc = new StringBuilder();
        Type effectType = effect.GetType();

        desc.AppendLine("<color=#FF88FF><b>Adjacent Weapon Boost</b></color>");

        var statTypeField = effectType.GetField("statType", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var inputTypeField = effectType.GetField("inputType", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var buffAmountField = effectType.GetField("buffAmount", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        string statName = "Stat";
        if (statTypeField != null)
        {
            object statValue = statTypeField.GetValue(effect);
            if (statValue != null)
                statName = FormatCamelCase(statValue.ToString());
        }

        if (buffAmountField != null)
        {
            int buffAmount = Convert.ToInt32(buffAmountField.GetValue(effect) ?? 0);
            InputType inputType = InputType.flat;
            
            if (inputTypeField != null)
            {
                inputType = (InputType)(inputTypeField.GetValue(effect) ?? InputType.flat);
            }

            int actualBuff = buffAmount * _currentItem.level;
            string sign = actualBuff > 0 ? "+" : "";
            
            if (inputType == InputType.percentage)
            {
                desc.AppendLine($"<color=#FFAAFF>{statName}: {sign}{actualBuff}%</color>");
            }
            else
            {
                desc.AppendLine($"<color=#FFAAFF>{statName}: {sign}{actualBuff}</color>");
            }
        }

        return desc.ToString();
    }

    /// <summary>
    /// Generates a description for all conditions
    /// </summary>
    private string GetConditionsDescription(Conditions[] conditions)
    {
        if (conditions == null || conditions.Length == 0) return string.Empty;

        StringBuilder condDesc = new StringBuilder();
        condDesc.AppendLine("<color=#FFFF88>Conditions:</color>");

        foreach (Conditions condition in conditions)
        {
            if (condition != null)
            {
                string singleCondDesc = GetConditionDescription(condition);
                if (!string.IsNullOrEmpty(singleCondDesc))
                    condDesc.AppendLine($"  • {singleCondDesc}");
            }
        }

        return condDesc.ToString();
    }

    /// <summary>
    /// Generates a readable description for a condition
    /// </summary>
    private string GetConditionDescription(Conditions condition)
    {
        if (condition == null) return string.Empty;

        Type conditionType = condition.GetType();
        string conditionName = conditionType.Name;

        // Remove "Condition" suffix if present
        if (conditionName.EndsWith("Condition"))
            conditionName = conditionName.Substring(0, conditionName.Length - 9);

        StringBuilder condDesc = new StringBuilder();

        // Handle specific condition types
        if (condition is LessThanEqualCondition || condition is MoreThanEqualCondition || condition is EqualCondition)
        {
            var valueTypeField = conditionType.GetField("valueType", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var thresholdField = conditionType.GetField("threshold", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var comparedValueField = conditionType.GetField("comparedValue", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxValueTypeField = conditionType.GetField("maxValueType", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            string valueTypeName = "Value";
            if (valueTypeField != null)
            {
                object val = valueTypeField.GetValue(condition);
                if (val != null) valueTypeName = FormatCamelCase(val.ToString());
            }

            float threshold = 0f;
            if (thresholdField != null)
            {
                threshold = Convert.ToSingle(thresholdField.GetValue(condition) ?? 0f);
            }
            else if (comparedValueField != null)
            {
                threshold = Convert.ToSingle(comparedValueField.GetValue(condition) ?? 0f);
            }

            string comparisonOperator = "";
            if (condition is LessThanEqualCondition) comparisonOperator = "<";
            else if (condition is MoreThanEqualCondition) comparisonOperator = ">";
            else if (condition is EqualCondition) comparisonOperator = "=";

            if (threshold <= 1f)
            {
                string maxValueTypeName = "Max";
                if (maxValueTypeField != null)
                {
                    object maxVal = maxValueTypeField.GetValue(condition);
                    if (maxVal != null) maxValueTypeName = FormatCamelCase(maxVal.ToString());
                }
                condDesc.Append($"<color=#88AAFF>{valueTypeName} {comparisonOperator} {threshold * 100}% {maxValueTypeName}</color>");
            }
            else
            {
                condDesc.Append($"<color=#88AAFF>{valueTypeName} {comparisonOperator} {threshold}</color>");
            }
        }
        else if (condition is AdjacentCondition)
        {
            var specificAdjacentField = conditionType.GetField("specificAdjacentItem", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredAdjacentField = conditionType.GetField("requiredAdjacentItem", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bool specificAdjacent = false;
            if (specificAdjacentField != null)
            {
                specificAdjacent = Convert.ToBoolean(specificAdjacentField.GetValue(condition) ?? false);
            }

            if (specificAdjacent && requiredAdjacentField != null)
            {
                ItemSO requiredItem = requiredAdjacentField.GetValue(condition) as ItemSO;
                if (requiredItem != null)
                {
                    condDesc.Append($"<color=#88AAFF>Adjacent to: {requiredItem.itemName}</color>");
                }
                else
                {
                    condDesc.Append("<color=#88AAFF>Adjacent to specific item</color>");
                }
            }
            else
            {
                condDesc.Append("<color=#88AAFF>Has adjacent items</color>");
            }
        }
        else
        {
            // Generic condition display
            condDesc.Append($"<color=#88AAFF>{FormatCamelCase(conditionName)}</color>");

            // Try to get all public fields
            var fields = conditionType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (fields.Length > 0)
            {
                condDesc.Append(": ");
                bool first = true;
                foreach (var field in fields)
                {
                    object value = field.GetValue(condition);
                    if (value != null && field.Name != "owner")
                    {
                        if (!first) condDesc.Append(", ");
                        condDesc.Append($"{FormatCamelCase(field.Name)} = {value}");
                        first = false;
                    }
                }
            }
        }

        return condDesc.ToString();
    }

    /// <summary>
    /// Formats camelCase or PascalCase to readable text
    /// </summary>
    private string FormatCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        StringBuilder result = new StringBuilder();
        result.Append(char.ToUpper(text[0]));

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && i > 0 && !char.IsUpper(text[i - 1]))
            {
                result.Append(' ');
            }
            result.Append(text[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Moves the tooltip to follow the mouse and adjusts its position to prevent it from going off-screen
    /// </summary>
    private void FollowMouseAndClamp()
    {
        // Get tooltip dimensions in screen pixels
        float width = tooltipRectTransform.rect.width * canvas.scaleFactor;
        float height = tooltipRectTransform.rect.height * canvas.scaleFactor;

        // Calculate starting position in screen space
        Vector3 newPos = Input.mousePosition + offset * canvas.scaleFactor;
        newPos.x += width * 0.5f;
        newPos.y -= height * 0.5f;
        newPos.z = 0f;

        // --- Clamping Logic ---

        // Right Edge Clamping
        float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + width * (1f - tooltipRectTransform.pivot.x)) - padding;
        if (rightEdgeToScreenEdgeDistance < 0)
        {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }

        // Left Edge Clamping
        float leftEdgeToScreenEdgeDistance = 0 - (newPos.x - width * tooltipRectTransform.pivot.x) + padding;
        if (leftEdgeToScreenEdgeDistance > 0)
        {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }

        // Top Edge Clamping
        float topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + height * (1f - tooltipRectTransform.pivot.y)) - padding;
        if (topEdgeToScreenEdgeDistance < 0)
        {
            newPos.y += topEdgeToScreenEdgeDistance;
        }

        // Bottom Edge Clamping
        float bottomEdgeToScreenEdgeDistance = (newPos.y - height * tooltipRectTransform.pivot.y) - padding;
        if (bottomEdgeToScreenEdgeDistance < 0)
        {
            newPos.y -= bottomEdgeToScreenEdgeDistance;
        }

        // Apply the clamped position in screen space
        tooltipRectTransform.transform.position = newPos;
    }

    /// <summary>
    /// Sets up a Canvas component on the tooltip for highest rendering priority
    /// </summary>
    private void SetupTooltipCanvas()
    {
        // Add Canvas component to tooltip if it doesn't exist
        tooltipCanvas = tooltipRectTransform.GetComponent<Canvas>();
        if (tooltipCanvas == null)
        {
            tooltipCanvas = tooltipRectTransform.gameObject.AddComponent<Canvas>();
        }

        // Configure canvas to override sorting
        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = 9999; // Very high value to ensure it's on top

        // Add GraphicRaycaster so it can receive pointer events
        if (tooltipRectTransform.GetComponent<GraphicRaycaster>() == null)
        {
            tooltipRectTransform.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}