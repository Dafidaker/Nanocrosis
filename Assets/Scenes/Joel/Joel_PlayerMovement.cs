using System;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class Joel_PlayerMovement : MonoBehaviour
{
    private Controls PlayerControls { get; set; }

    [Header("Movement"), Space(10)]
    public float walkSpeed;
    public float sprintSpeed;
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
    
    [Header("RotatePlayer"), Space(10)] 
    public Transform cam;
    public float rotationSpeed = 6f;
    public float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;
    
    [Header("Slope Handling"), Space(10)] 
    public float minSlopeAngle;
    private RaycastHit _slopeHit;

    [Header("DetectGround"), Space(10)] 
    [field: SerializeField] private LayerMask _layerMask;

    [Header("DebugVariables"), Space(10)] 
    public bool isGrounded;
    public GameObject DebugGameObject;
    
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
        DebugGameObject = Instantiate(DebugGameObject);

        _currentJumps = 0;
    }
    
    private void Update()
    {
        _horizontalInput = _iMove.ReadValue<Vector2>().x;
        _verticalInput = _iMove.ReadValue<Vector2>().y;
        
        //adds drag if player is grounded
        if (isGrounded) { _rb.drag = groundDrag;}
        else if (OnSlope()) { _rb.drag = groundDrag * 3;}
        else { _rb.drag = 0; }
        
        //limits the player speed
        SpeedControl();
        
        //rotates the player to the direction of the camera if the camera is moving
        RotatePlayer();
        
        //changes the movement state
        StateHandler();

        
        //'increase' gravity if player is on air and the jump is over
        if (isJumpOver && _movementState == MovementState.Airbourne)
        {
            _rb.AddForce(Physics.gravity * fallGravity , ForceMode.Acceleration);
        }

        //if its on a slope apply more gravity
        if (OnSlope())
        {
            _rb.useGravity = false;
            _rb.AddForce(_slopeHit.normal * -Physics.gravity.magnitude , ForceMode.Acceleration);
            Debug.Log("hit slope");
            
        }
        else
        {
            _rb.useGravity = true;
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
            Debug.Log("slope hit");
            
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
        if (isGrounded && _isSprinting)
        {
            _movementState = MovementState.Sprint;
            _moveSpeed = sprintSpeed;
        }
        
        else if (isGrounded)
        {
            _movementState = MovementState.Walk;
            _moveSpeed = walkSpeed;
        }

        else
        {
            _movementState = MovementState.Airbourne;
            
        }
    }

    private bool OnSlope()
    {
        //if it doesnt hit a slope it returns false 
        if (!Physics.SphereCast(transform.position,0.5f ,Vector3.down, out _slopeHit,0.6f, _layerMask )) return false;

        
        //if it hits we check it the angles is smaller that the max slope angle
        float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
        return angle > minSlopeAngle && angle != 0;
    }

    private void DetectGround()
    {
        isGrounded = Physics.SphereCast(transform.position,0.5f ,Vector3.down, out _,0.6f, _layerMask);
        
        //if _readyToJump is false it means the player just jumped meaning that they arent on the ground
        if (!_readyToJump) isGrounded = false;
        
        //if the player is on the ground their jumps will reset
        if (isGrounded) { _currentJumps = 0; }
    }
    private Vector3 GetSlopeMoveDirection(Vector3 vector)
    {
        return Vector3.ProjectOnPlane(vector, _slopeHit.normal);
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
        Debug.Log("_readyToJump" + _readyToJump);
        Debug.Log("isGrounded" + isGrounded);
        if (_readyToJump && (isGrounded || _currentJumps < numberOfJumps))
        {
            _readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void JumpEndedInput(InputAction.CallbackContext context)
    {
        isJumpOver = true;
    }
    private void DashInput(InputAction.CallbackContext context)
    {
        
    }
    private void ResetDash()
    {
        
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
        GUI.Box(new Rect(0, 0, Screen.width * 0.1f, Screen.height * 0.1f), _rb.velocity.ToString());
        GUI.Box(new Rect(0, 50, Screen.width * 0.1f, Screen.height * 0.1f), isJumpOver.ToString());
    }
}