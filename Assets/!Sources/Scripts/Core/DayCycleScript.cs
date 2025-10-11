// --------------------------------------------------------------
// Creation Date: 2025-10-12 01:49
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting;
using UnityEngine;

public class DayCycleScript : MonoBehaviour
{
    [Header("debug field, just for looking")]
    [SerializeField] private int tilesMoved = 0;
    [SerializeField] private int day = 0;
    [SerializeField] timeOfDay currentTime = timeOfDay.day; //please help me rename it, im horrible at names
    [Header("other")]
    [SerializeField] private int dayLength;
    [SerializeField] private int nightLength;
    [SerializeField] private int dayLengthMod; //this is for when you get items that affect the day length
    
    
    enum timeOfDay{day, night};
    
    public void setTilesMoved(int val){ tilesMoved = val; }
    public void addTilesMoved(int val){ tilesMoved += val; }
    public int getTilesMoved() { return tilesMoved; }

    private void Update()
    {
        switch(currentTime)
        {
            case timeOfDay.day:
                if (tilesMoved >= dayLength+dayLengthMod)
                {
                    currentTime = timeOfDay.night;
                    tilesMoved = 0;
                }
                break;
            case timeOfDay.night:
                if (tilesMoved >= nightLength)
                {
                    currentTime = timeOfDay.day;
                    tilesMoved = 0;
                    day += 1;
                }
                break;
        }
    }

}
