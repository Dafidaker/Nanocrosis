using System;
using System.Collections;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Controls PlayerControls { get; set; }

    [Header("Debug Variables"), Space(10)] 
    public bool isGrounded;
    public GameObject debugGameObject;
    
    [Header("Movement"), Space(10)]
    public float walkSpeed;
    public float sprintSpeed;
    public float dashSpeed;
    public float groundDrag;
    private MovementState _movementState;
    private float _moveSpeed;
    private bool _isSprinting;

    [Header("Jump"), Space(10)] 
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float fallGravity;
    private bool _readyToJump = true;
    [field: SerializeField, Range(1,10)] private int numberOfJumps;
    private int _currentJumps;
    [field: HideInInspector] public bool isJumpOver;
    
    [Header("Dash"), Space(10)] 
    [field: SerializeField] private float dashForce;
    [field: SerializeField] private float dashUpwardForce;
    [field: SerializeField] private float dashDuration;
    [field: SerializeField] private float dashCooldown;
    [field: SerializeField] private float dashSpeedChangeFactor;
    private float _dashCooldownTimer;
    private bool _dashing;
    
    [Header("Rotate Player"), Space(10)] 
    public Transform cam;

    [Header("Slope Handling"), Space(10)] 
    public float minSlopeAngle;
    public float slopeDragMultiplier;
    private RaycastHit _slopeHit;
    
    //Keeping Momentum
    private float _speedChangeFactor;
    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private MovementState _lastMovementState;
    private bool _keepMomentum;

    [Header("Detect Ground"), Space(10)] 
    [field: SerializeField] private LayerMask layerMask;


    private Vector3 _delayedForceToApply;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private Vector3 _moveDirection;
    private Rigidbody _rb;
    
    
    //Input actions.
    private InputAction _iRotate;
    private InputAction _iMove;
    private InputAction _iJump;
    private InputAction _iDash;
    private InputAction _iSprint;
    private InputAction _iMelee;
    private InputAction _iShootLeft;
    private InputAction _iShootRight;
    private InputAction _iCycleLeft;
    private InputAction _iCycleRight;
    private InputAction _iHeal;
    private InputAction _iCritical;


    #region Unity Funtions

    private void OnEnable()
    {
        EnableInputSystem();
    }
    
    private void OnDisable()
    {
        DisableInputSystem();
    }

    private void Awake()
    {
        PlayerControls = new Controls();
        Settings.GameStart(); //todo change this to a game controller or something :)
    }
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        //debugGameObject = Instantiate(debugGameObject);

        _currentJumps = 0;
    }
    
    private void Update()
    {
        _horizontalInput = _iMove.ReadValue<Vector2>().x;
        _verticalInput = _iMove.ReadValue<Vector2>().y;
        
        //adds drag if player is grounded
        if (_movementState == MovementState.Sprint || _movementState == MovementState.Walk)
        {
            _rb.drag = groundDrag;
            if (OnSlope()) { _rb.drag = groundDrag * slopeDragMultiplier;}
        }
        else { _rb.drag = 0; }
        
        //limits the player speed
        SpeedControl();
        
        //rotates the player to the direction of the camera if the camera is moving
        RotatePlayer();
        
        //changes the movement state
        StateHandler();

        
        //'increase' gravity if player is on air and the jump is over
        if (isJumpOver && _movementState == MovementState.Airborne)
        {
            _rb.AddForce(Physics.gravity * fallGravity , ForceMode.Acceleration);
        }

        //if its on a slope apply more gravity
        if (OnSlope())
        {
            _rb.useGravity = false;
            _rb.AddForce(_slopeHit.normal * -Physics.gravity.magnitude , ForceMode.Acceleration);
        }
        else
        {
            _rb.useGravity = true;
        }

        //if the dash cooldown hasn't ended count it down
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
        
        
        Debug.DrawRay(transform.position , _rb.velocity.normalized, Color.green, 100f);
    }

    private void FixedUpdate()
    {
        DetectGround();
        
        MovePlayer();
    }

    #endregion

    #region Funtions

    #region Movement

    private void MovePlayer()
    {
        var playerTransform = transform;
        _moveDirection = playerTransform.forward * _verticalInput + playerTransform.right * _horizontalInput;

        //on a slope
        if (OnSlope())
        {
            _rb.AddForce(GetSlopeMoveDirection(_moveDirection).normalized * (_moveSpeed * 10f), ForceMode.Force);
        }
        else if (isGrounded)
        {
            _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10f), ForceMode.Force); 
        }
        else if (!isGrounded)
        {
            _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10f * airMultiplier), ForceMode.Force); 
        }
        
    }
    private void SpeedControl()
    {
        var velocity = _rb.velocity;
        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);

        if (!(flatVelocity.magnitude > _moveSpeed)) return;
        Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
        _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);

        if (_rb.velocity.y <= 0 )
        {
            isJumpOver = true;
        }
    }
    private void Jump()
    {
        _currentJumps++;
        isJumpOver = false;
        
        var velocity = _rb.velocity;
        _rb.velocity = new Vector3(velocity.x, 0, velocity.z);
        
        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        _readyToJump = true;
    }

    private void Dash()
    {
        //if the cooldown hasn't ended just returns
        if (_dashCooldownTimer > 0) return;
        
        //if the dash goes through the timer gets as big as the cooldown
        _dashCooldownTimer = dashCooldown;

        _dashing = true;
        
        Vector3 dashDirection = Vector3.zero;
        
        if (Settings.DashMovementDirection)
        {
            dashDirection = _moveDirection;
        }
        
        if (!Settings.DashMovementDirection || (_verticalInput == 0 && _horizontalInput == 0))
        {
            dashDirection = cam.forward; 
        }

        var forceToApply = dashDirection * dashForce + transform.up * dashUpwardForce;

        _delayedForceToApply = forceToApply;

        _rb.useGravity = false;
        
        Invoke(nameof(DelayedForceToApply), 0.025f);
        
        Invoke(nameof(ResetDash), dashDuration);
    }
    
    private void DelayedForceToApply()
    {
        _rb.velocity = Vector3.zero;
        
        _rb.AddForce(_delayedForceToApply, ForceMode.Impulse);  
    }
    
    private void ResetDash()
    {
        _rb.useGravity = true;
        _dashing = false;
    }
    
    #endregion
    
    private void RotatePlayer()
    {
        //var targetAngle = MathF.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        //var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
         
        
        if (!_iRotate.triggered) return;
        //_moveDirection = (Quaternion.Euler(0f, angle, 0f) * Vector3.forward).normalized;
        var camTransformForward = cam.transform.forward;
        transform.forward = new Vector3(camTransformForward.x, 0f, camTransformForward.z);
    }

    private void StateHandler()
    {
        if (_dashing)
        {
            _movementState = MovementState.Dash;
            _desiredMoveSpeed = dashSpeed;
            _speedChangeFactor = dashSpeedChangeFactor;
        }
        
        else if (isGrounded && _isSprinting)
        {
            _movementState = MovementState.Sprint;
            _desiredMoveSpeed = sprintSpeed;
        }
        
        else if (isGrounded)
        {
            _movementState = MovementState.Walk;
            _desiredMoveSpeed = walkSpeed;
        }

        else
        {
            _movementState = MovementState.Airborne;
            
            //if the player speed is lower than the sprint speed it makes the desired the walk
            //other wise it makes the desired the sprint speed
            _desiredMoveSpeed = _desiredMoveSpeed < sprintSpeed ? walkSpeed : sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = _desiredMoveSpeed != _lastDesiredMoveSpeed;
        
        if (_lastMovementState == MovementState.Dash)
        {
            _keepMomentum = true;
        }
        
        if (desiredMoveSpeedHasChanged)
        {
            if (_keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                _moveSpeed = _desiredMoveSpeed;
            }
        }

        _lastDesiredMoveSpeed = _desiredMoveSpeed;
        _lastMovementState = _movementState;
    }

    private bool OnSlope()
    {
        //if it doesnt hit a slope it returns false 
        if (!Physics.SphereCast(transform.position,0.5f ,Vector3.down, out _slopeHit,0.6f, layerMask )) return false;

        
        //if it hits we check it the angles is smaller that the max slope angle
        float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
        return angle > minSlopeAngle && angle != 0;
    }

    private void DetectGround()
    {
        isGrounded = Physics.SphereCast(transform.position,0.5f ,Vector3.down, out _,0.6f, layerMask);
        
        //if _readyToJump is false it means the player just jumped meaning that they arent on the ground
        if (!_readyToJump) isGrounded = false;
        
        //if the player is on the ground their jumps will reset
        if (isGrounded) { _currentJumps = 0; }
    }
    private Vector3 GetSlopeMoveDirection(Vector3 vector)
    {
        return Vector3.ProjectOnPlane(vector, _slopeHit.normal);
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(_desiredMoveSpeed - _moveSpeed);
        float startValue = _moveSpeed;

        float boostFactor = _speedChangeFactor;

        while (time < difference)
        {
            _moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        
        _moveSpeed = _desiredMoveSpeed;
        _speedChangeFactor = 1f;
        _keepMomentum = false;
    }
    
    #endregion

    #region Input System Functions
    
    private void SprintInput(InputAction.CallbackContext context)
    {
        if (Settings.IsSprintToggle)
        {
            _isSprinting = !_isSprinting;
        }
        else if (!Settings.IsSprintToggle)
        {
            _isSprinting = true;
        }
        
    }
    private void SprintEndedInput(InputAction.CallbackContext context)
    {
        if (!Settings.IsSprintToggle)
        {
            _isSprinting = false;
        }
    }
    private void JumpInput(InputAction.CallbackContext context)
    {
        //if the player isn't _readyToJump or
        //the nº of jumps that they took are higher or equal to the amount allowed it returns
        if (!_readyToJump || _currentJumps >= numberOfJumps) return;
        
        //if its your first jump and you arent on the ground u cant jump 
        if (_currentJumps == 0  && !isGrounded) return;
        _readyToJump = false;

        Jump();

        Invoke(nameof(ResetJump), jumpCooldown);
    }
    private void JumpEndedInput(InputAction.CallbackContext context)
    {
        isJumpOver = true;
    }
    private void DashInput(InputAction.CallbackContext context)
    {
        Dash();
    }
    private void EnableInputSystem()
    {
        _iMove = PlayerControls.Player.Walk;
        _iMove.Enable();
        
        _iRotate = PlayerControls.Player.Rotate;
        _iRotate.Enable();
        
        _iJump = PlayerControls.Player.Jump;
        _iJump.performed += JumpInput;
        _iJump.canceled += JumpEndedInput;
        _iJump.Enable();

        _iDash = PlayerControls.Player.Dash;
        _iDash.performed += DashInput;
        _iDash.Enable();

        _iSprint = PlayerControls.Player.Sprint;
        _iSprint.performed += SprintInput;
        _iSprint.canceled += SprintEndedInput;
        _iSprint.Enable();

        _iMelee = PlayerControls.Player.Melee;
        _iMelee.Enable();

        _iShootLeft = PlayerControls.Player.ShootLeft;
        _iShootLeft.Enable();

        _iShootRight = PlayerControls.Player.ShootRight;
        _iShootRight.Enable();

        _iCycleLeft = PlayerControls.Player.CycleLeft;
        _iCycleLeft.Enable();

        _iCycleRight = PlayerControls.Player.CycleRight;
        _iCycleRight.Enable();

        _iHeal = PlayerControls.Player.Healing;
        _iHeal.Enable();

        _iCritical = PlayerControls.Player.CriticalMesures;
        _iCritical.Enable();
    }
    private void DisableInputSystem()
    {
        _iMove.Disable();
        _iRotate.Disable();
        _iJump.Disable();
        _iDash.Disable();
        _iSprint.Disable();
        _iMelee.Disable();
        _iShootLeft.Disable();
        _iShootRight.Disable();
        _iCycleLeft.Disable();
        _iCycleRight.Disable();
        _iHeal.Disable();
        _iCritical.Disable();
    }
    
    #endregion

    private void OnGUI()
    {
        /*GUI.Box(new Rect(0, 0, Screen.width * 0.1f, Screen.height * 0.1f), _rb.velocity.ToString());
        GUI.Box(new Rect(0, 50, Screen.width * 0.1f, Screen.height * 0.1f), isJumpOver.ToString());*/
    }
}