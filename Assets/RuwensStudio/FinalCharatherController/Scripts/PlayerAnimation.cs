using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private PlayerLocomotionController _playerLocomotionController;

    private PlayerState _playerState;

    private static int inputXHash = Animator.StringToHash("InputX");
    private static int inputYHash = Animator.StringToHash("InputY");

    private static int isGroundesHash = Animator.StringToHash("isGrounded");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isFlyingHash = Animator.StringToHash("isFlying");
    private static int isFallingHash = Animator.StringToHash("isFalling");

    private void Awake()
    {
        _playerLocomotionController = GetComponent<PlayerLocomotionController> ();
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        

        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerState.PlayerMovementState.Idle;
        bool isRunning = _playerState.CurrentPlayerMovementState == PlayerState.PlayerMovementState.Running;
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerState.PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerState.PlayerMovementState.Falling;
        bool isGrounded = _playerState.CurrentPlayerMovementState == PlayerState.PlayerMovementState.Grounded;
        bool isFlying = _playerState.CurrentPlayerMovementState == PlayerState.PlayerMovementState.Flying;

        Vector2 inputTarget = _playerLocomotionController.MovementInput;


        _animator.SetBool(isGroundesHash, isIdling);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isFlyingHash, isFlying);
        _animator.SetBool(isFallingHash, isFalling);

        _animator.SetFloat(inputXHash, inputTarget.x); 
        _animator.SetFloat(inputYHash, inputTarget.y);
    }
}
