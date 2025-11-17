// --------------------------------------------------------------
// Creation Date: 2025-11-16
// Author: User
// Description: Singleton tooltip manager for item display
// --------------------------------------------------------------
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
        itemNameText.text = $"<size=110%>{_currentItem.itemData.itemName}</size>";

        // Build description from effects
        StringBuilder descBuilder = new StringBuilder();

        // Parse effects and display their stats
        if (_currentItem.itemData.effects != null && _currentItem.itemData.effects.Length > 0)
        {
            foreach (Effect effect in _currentItem.itemData.effects)
            {
                if (effect == null) 
                {
                    Debug.LogWarning("Null effect found in effects array");
                    continue;
                }

                Debug.Log($"<color=yellow>Processing effect: {effect.GetType().Name}</color>");

                string effectDescription = GetEffectDescription(effect);
                if (!string.IsNullOrEmpty(effectDescription))
                {
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

        // Use proper reflection flags to get ALL fields (public, private, inherited)
        var allFields = effectType.GetFields(System.Reflection.BindingFlags.Public | 
                                            System.Reflection.BindingFlags.NonPublic | 
                                            System.Reflection.BindingFlags.Instance);

        Debug.Log($"<color=magenta>Found {allFields.Length} fields in {effectType.Name}</color>");
        foreach (var f in allFields)
        {
            Debug.Log($"<color=magenta>  Field: {f.Name} ({f.FieldType.Name}) = {f.GetValue(effect)}</color>");
        }

        // Check for WeaponSpawnEffect
        if (effectType.Name == "WeaponSpawnEffect")
        {
            var weaponNameField = effectType.GetField("weaponName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseDamageField = effectType.GetField("baseAttackDamage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseSpeedField = effectType.GetField("baseAttackSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseVarianceField = effectType.GetField("baseAttackVariance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (weaponNameField != null)
            {
                string weaponName = weaponNameField.GetValue(effect)?.ToString() ?? "Unknown";
                effectDesc.AppendLine($"<color=#FFA500><b>{weaponName}</b></color>");
            }

            if (baseDamageField != null)
            {
                int damage = (int)(baseDamageField.GetValue(effect) ?? 0);
                effectDesc.AppendLine($"Base Attack Damage: <color=#C23753>{damage}</color>");
            }

            if (baseSpeedField != null)
            {
                int speed = (int)(baseSpeedField.GetValue(effect) ?? 0);
                string colorCode = speed <= 2 ? "#C23753" : speed >= 6 ? "#EBB85B" : "#FFFFFF";
                effectDesc.AppendLine($"Base Attack Speed: <color={colorCode}>{speed}</color>");
            }

            if (baseVarianceField != null)
            {
                int variance = (int)(baseVarianceField.GetValue(effect) ?? 0);
                if (variance > 0)
                    effectDesc.AppendLine($"Base Attack Variance: <color=#88AAFF>{variance}</color>");
            }
        }
        // Check for HealEffect
        else if (effectType.Name == "HealEffect")
        {
            var healAmountField = effectType.GetField("healAmount", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var conditionsField = effectType.GetField("conditions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (healAmountField != null)
            {
                int healAmount = (int)(healAmountField.GetValue(effect) ?? 0);
                effectDesc.AppendLine($"<color=#77FF77><b>Heal: +{healAmount} HP</b></color>");
            }

            // Display conditions if they exist
            if (conditionsField != null)
            {
                Conditions[] conditions = conditionsField.GetValue(effect) as Conditions[];
                if (conditions != null && conditions.Length > 0)
                {
                    effectDesc.AppendLine("<color=#FFFF88>Conditions:</color>");
                    foreach (Conditions condition in conditions)
                    {
                        if (condition != null)
                        {
                            string conditionDesc = GetConditionDescription(condition);
                            if (!string.IsNullOrEmpty(conditionDesc))
                                effectDesc.AppendLine($"  • {conditionDesc}");
                        }
                    }
                }
            }
        }
        // Check for BuffStatEffect
        else if (effectType.Name == "BuffStatEffect")
        {
            var statTypeField = effectType.GetField("statType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var buffAmountField = effectType.GetField("buffAmount", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var conditionsField = effectType.GetField("conditions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            string statName = "Stat";
            if (statTypeField != null)
            {
                object statValue = statTypeField.GetValue(effect);
                if (statValue != null)
                    statName = FormatCamelCase(statValue.ToString());
            }

            if (buffAmountField != null)
            {
                int buffAmount = (int)(buffAmountField.GetValue(effect) ?? 0);
                string sign = buffAmount > 0 ? "+" : "";
                string color = buffAmount > 0 ? "#77FF77" : "#FF7777";
                effectDesc.AppendLine($"<color={color}><b>{statName}: {sign}{buffAmount}</b></color>");
            }

            // Display conditions if they exist
            if (conditionsField != null)
            {
                Conditions[] conditions = conditionsField.GetValue(effect) as Conditions[];
                if (conditions != null && conditions.Length > 0)
                {
                    effectDesc.AppendLine("<color=#FFFF88>Conditions:</color>");
                    foreach (Conditions condition in conditions)
                    {
                        if (condition != null)
                        {
                            string conditionDesc = GetConditionDescription(condition);
                            if (!string.IsNullOrEmpty(conditionDesc))
                                effectDesc.AppendLine($"  • {conditionDesc}");
                        }
                    }
                }
            }
        }
        // Fallback for unknown effect types
        else
        {
            string effectTypeName = effectType.Name;
            if (effectTypeName.EndsWith("Effect"))
                effectTypeName = effectTypeName.Substring(0, effectTypeName.Length - 6);
            
            effectDesc.AppendLine($"<color=#FFA500><b>{FormatCamelCase(effectTypeName)}</b></color>");
        }

        string result = effectDesc.ToString();
        Debug.Log($"<color=magenta>Generated description: {result}</color>");
        return result;
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
        condDesc.Append($"<color=#88AAFF>{FormatCamelCase(conditionName)}</color>");

        // Get all public fields from the condition
        var fields = conditionType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (fields.Length > 0)
        {
            condDesc.Append(": ");
            bool first = true;
            foreach (var field in fields)
            {
                object value = field.GetValue(condition);
                if (value != null)
                {
                    if (!first) condDesc.Append(", ");
                    condDesc.Append($"{FormatCamelCase(field.Name)} = {value}");
                    first = false;
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