using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    private bool isFiring = false;
    public bool IsFiring => isFiring;

    public event System.Action<bool> OnFireStateChanged;

    public float fireBeamCameraMovement = 0.1f;
    public float normalCamDistance = 5f;
    public float fireCamDistance = 2.5f;

    public float zoomInSpeed = 20f;
    public float zoomOutSpeed = 2f;

    private float currentCamDistance;

    public float gravity = -9.81f;
    public float jumpForce = 7f; 
    public float slowFallVelocity = -2f;

    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 85f;

    private PlayerLocomotionController _playerLocomotionInput;
    private PlayerState _playerState;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity = 0f;
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    public bool _isFlying = false;
    public bool _isJumping = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();

        _playerLocomotionInput = GetComponent<PlayerLocomotionController>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentCamDistance = normalCamDistance;
    }

    private void Update()
    {
        HandleVerticalMovement();
        HandleMovement();
    }

    private void LateUpdate()
    {
        float targetDistance = isFiring ? fireCamDistance : normalCamDistance;
        float speed = (targetDistance < currentCamDistance) ? zoomInSpeed : zoomOutSpeed;
        currentCamDistance = Mathf.Lerp(currentCamDistance, targetDistance, Time.deltaTime * speed);

        float zoomFactor = isFiring ? fireBeamCameraMovement : 1f;
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x * zoomFactor;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y * zoomFactor,
                                        -lookLimitV, lookLimitV);

        _playerTargetRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x * zoomFactor;
        float smoothRotation = Mathf.LerpAngle(transform.eulerAngles.y,
                                              _playerTargetRotation.x,
                                              Time.deltaTime * 10f);
        transform.rotation = Quaternion.Euler(0f, smoothRotation, 0f);

        Quaternion targetCamRotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
        _playerCamera.transform.rotation = Quaternion.Slerp(_playerCamera.transform.rotation,
                                                            targetCamRotation,
                                                            Time.deltaTime * 10f);

        Vector3 camOffset = -_playerCamera.transform.forward * currentCamDistance;
        _playerCamera.transform.position = transform.position + Vector3.up * 2f + camOffset;
    }

    private void HandleMovement()
    {
        PlayerState.PlayerMovementState newState = PlayerState.PlayerMovementState.Idle;

        if (_isFlying)
            newState = PlayerState.PlayerMovementState.Flying;
        else if (_isJumping)
            newState = PlayerState.PlayerMovementState.Jumping;
        else if (_characterController.isGrounded)
            newState = _playerLocomotionInput.MovementInput != Vector2.zero
                       ? PlayerState.PlayerMovementState.Walking
                       : PlayerState.PlayerMovementState.Idle;
        else if (!_characterController.isGrounded && _verticalVelocity < 0)
            newState = PlayerState.PlayerMovementState.Falling;

        _playerState.SetPlayerMovementState(newState);

        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 moveDir = cameraRightXZ * _playerLocomotionInput.MovementInput.x
                        + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        float currentSpeed = _isFlying
            ? SkillSystem.Instance.GetFlySpeed()
            : SkillSystem.Instance.GetRunSpeed();

        _horizontalVelocity = moveDir * currentSpeed;
    }

    private void HandleVerticalMovement()
    {
        if (_playerLocomotionInput.FlyPressed)
            _isFlying = true;
        else if (_characterController.isGrounded)
            _isFlying = false;

        if (_isFlying)
        {
            float flyDir = _playerLocomotionInput.FlyPressed ? 1f
                           : (_playerLocomotionInput.FlyDownPressed ? -1f : 0f);

            if (Mathf.Approximately(flyDir, 0f))
                _verticalVelocity = slowFallVelocity;
            else
            {
                _verticalVelocity = flyDir * SkillSystem.Instance.GetFlySpeed();
                if (transform.position.y >= SkillSystem.Instance.GetMaxFlyHeight() && flyDir > 0)
                    _verticalVelocity = 0f;
            }
        }
        else
        {
            if (_characterController.isGrounded)
            {
                _verticalVelocity = 0f;
                _isJumping = false;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        Vector3 velocity = _horizontalVelocity;
        velocity.y = _verticalVelocity;
        _characterController.Move(velocity * Time.deltaTime);
    }

    public void SetFireCameraMode(bool firing)
    {
        if (isFiring == firing) return;
        isFiring = firing;
        OnFireStateChanged?.Invoke(isFiring);
    }
}
