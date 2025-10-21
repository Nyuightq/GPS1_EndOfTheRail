// --------------------------------------------------------------
// Creation Date: 2025-10-06 09:33
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UI_CombatEntity : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider attackIntervalBar;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damageTextPrefab; // a TextMeshProUGUI prefab
    [SerializeField] private Transform damageTextParent;  // UI anchor (usually same Canvas)


    
    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        healthBar.value = currentValue / maxValue;
    }

    public void UpdateAttackIntervalBar(float currentValue, float maxValue)
    {
        attackIntervalBar.value = currentValue / maxValue;
    }

    public void ShowDamageText(int damageValue, bool isCritical = false)
    {
        if (damageTextPrefab == null || damageTextParent == null) return;

        GameObject go = Instantiate(damageTextPrefab, damageTextParent);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = damageValue.ToString();

        if (isCritical)
        {
            tmp.color = Color.yellow;
            tmp.fontSize *= 1.2f;
        }
        else
        {
            tmp.color = Color.red;
        }

        // Start floating animation
        StartCoroutine(FloatingTextRoutine(go));
    }

    private IEnumerator FloatingTextRoutine(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        float duration = 1.0f;
        float time = 0f;

        Vector3 startPos = rect.localPosition;

        float horizontalOffset = Random.Range(-60f, 60f);
        float peakHeight = Random.Range(60f, 90f);
        Vector3 endPos = startPos + new Vector3(horizontalOffset, 0f, 0f);

        Color startColor = tmp.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // ChatGPT: Using a parabola-like formula: y = 4t(1 - t) will produce an upward arc
            float parabola = 4 * t * (1 - t);  //

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += parabola * peakHeight;

            rect.localPosition = pos;

            // 漸淡
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);

            yield return null;
        }

        Destroy(go);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
