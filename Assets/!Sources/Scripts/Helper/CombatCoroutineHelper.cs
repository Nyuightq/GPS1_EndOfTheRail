// --------------------------------------------------------------
// Creation Date: 2025-10-22 22:13
// Author: ZQlie
// Description: Helper for combat Tile
// --------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;

public class CombatCoroutineHelper : MonoBehaviour
{
    private static CombatCoroutineHelper _instance;

    public static void RunNextFrame(Action action)
    {
        if (_instance == null)
        {
            GameObject helper = new GameObject("CombatCoroutineHelper");
            _instance = helper.AddComponent<CombatCoroutineHelper>();
            DontDestroyOnLoad(helper);
        }

        _instance.StartCoroutine(_instance.RunNextFrameRoutine(action));
    }

    private IEnumerator RunNextFrameRoutine(Action action)
    {
        yield return null; // wait one frame
        action?.Invoke();
    }
}
