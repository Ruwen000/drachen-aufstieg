using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idle;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }

    public enum PlayerMovementState
    {
        Idle = 0,
        Walking = 1,
        Flying = 2,
        Running = 3,
        Jumping = 4,
        Falling = 5,
        Grounded = 6,
    }
}
