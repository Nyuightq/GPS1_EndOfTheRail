// --------------------------------------------------------------
// Creation Date: 2025-11-11 09:51
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class TEsting : MonoBehaviour
{
    [SerializeReference] public Effect[] effects;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (Effect effect in effects)
            {
                effect.apply();
            }
        }
    }
}
