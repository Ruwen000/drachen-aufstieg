using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerLocomotionController : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    public PlayerControls PlayerControls { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }    
    public bool FlyPressed { get; private set; }
    public bool FlyDownPressed { get; private set; }
    public bool FirePressed { get; private set; }     

    private void Awake()
    {
        PlayerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        PlayerControls.Enable();
        PlayerControls.PlayerLocomotionMap.Enable();
        PlayerControls.PlayerLocomotionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        PlayerControls.PlayerLocomotionMap.Disable();
        PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }

    private void OnDestroy()
    {
        PlayerControls.Dispose();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
    }

    public void OnCameralook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) JumpPressed = true;
        else if (context.canceled) JumpPressed = false;
    }

    public void OnFlyUp(InputAction.CallbackContext context)
    {
        if (context.started) FlyPressed = true;
        else if (context.canceled) FlyPressed = false;
    }

    public void OnFlyDown(InputAction.CallbackContext context)
    {
        if (context.started) FlyDownPressed = true;
        else if (context.canceled) FlyDownPressed = false;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            FirePressed = true;
            if (PlayerController.Instance != null) PlayerController.Instance.SetFireCameraMode(true);
        }
        else if (context.canceled)
        {
            FirePressed = false;
            if (PlayerController.Instance != null) PlayerController.Instance.SetFireCameraMode(false);
        }

    }
}
