// --------------------------------------------------------------
// Creation Date: 2025-10-30 18:33
// Author: User
// Description: -
// --------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private InputActionMap playerActionMap;
    private InputAction leftClickAction;
    private InputAction rightClickAction;

    public static event Action OnLeftClick, OnLeftRelease, OnLeftHold;
    public static event Action OnRightClick, OnRightRelease, OnRightHold;

    private void OnEnable()
    {
        playerActionMap = inputActions.FindActionMap("Player");
        leftClickAction = playerActionMap.FindAction("LeftClick");
        rightClickAction = playerActionMap.FindAction("RightClick");

        leftClickAction.performed += LeftClicked;
        leftClickAction.canceled += LeftReleased;

        rightClickAction.performed += RightClicked;
        rightClickAction.canceled += RightReleased;
    }

    private void OnDisable()
    {
        leftClickAction.performed -= LeftClicked;
        leftClickAction.canceled -= LeftReleased;

        rightClickAction.performed -= RightClicked;
        rightClickAction.canceled -= RightReleased;
    }

    private void Update()
    {
        if(leftClickAction.IsPressed())
        {
            OnLeftHold?.Invoke();
        } 
        
        if (rightClickAction.IsPressed())
        {
            OnRightHold?.Invoke();
        }
    }

    private void LeftClicked(InputAction.CallbackContext ctx){ OnLeftClick?.Invoke(); }
    private void LeftReleased(InputAction.CallbackContext ctx){ OnLeftRelease?.Invoke(); }
    private void RightClicked(InputAction.CallbackContext ctx) { OnRightClick?.Invoke(); }
    private void RightReleased(InputAction.CallbackContext ctx) { OnRightRelease?.Invoke(); }

}
