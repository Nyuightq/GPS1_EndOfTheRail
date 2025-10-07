using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int componentCount { get; private set; }
    public int scrapCount { get; private set; }

    public void AddComponent()
    {
        componentCount++;
        Debug.Log("Component collected! Total: " + componentCount);
    }

    public void AddScrap()
    {
        scrapCount++;
        Debug.Log("Scrap collected! Total: " + scrapCount);
    }

    public bool SpendScrap(int amount)
    {
        if (scrapCount >= amount)
        {
            scrapCount -= amount;
            return true;
        }
        return false;
    }
}
