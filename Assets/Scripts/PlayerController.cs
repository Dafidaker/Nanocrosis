using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public Controls PlayerControls { get; set; }

    public Transform cam;
    
    [Header("Dashing")]
    public float DashForce;
    public float DashUpwardForce;
    public float DashDuration;

    [Header("Dash Cooldown")]
    public float DashCd;
    private float DashCdTimer;
    
    
    [Header("Rotation Speed")]
    public float rotationSpeed = 6f;
    public float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;

    private Rigidbody _rb;
    private Vector2 _lookDirection = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;

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

    private const float MoveSpeed = 500f; //placeholder
    private Vector3 _jumpForce; //placeholder
    //private const float DashForce = 200f;

    private bool _isSprinting = false;
    private bool _isGrounded = true;
    private bool _canDoubleJump = true;

    private float _ySpeed;
    private bool _jumpEnded;

    private int _sprintMultiplier = 1;

    private Vector2 turn;
    private void Awake()
    {
        PlayerControls = new Controls();
    }

    private void OnEnable()
    {
        EnableInputSystem();
    }

    private void OnDisable()
    {
        DisableInputSystem();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _jumpForce = new Vector3(0, 100, 0);

        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _moveDirection = _iMove.ReadValue<Vector3>();
        _moveDirection = _moveDirection.normalized;
        //RotatePlayer();
        
        if (_isSprinting) _sprintMultiplier = 2;
        else _sprintMultiplier = 1;

        _ySpeed += Physics.gravity.y * Time.deltaTime * 10;
        
        if(_jumpEnded)
        {
            _ySpeed -= 100 * Time.deltaTime;
        }

        if (_isGrounded)
        {
            _ySpeed = -0.5f;
        }
    }

    private void FixedUpdate()
    {
        Vector3 velocity = new Vector3(_moveDirection.x * MoveSpeed, _ySpeed * 10, _moveDirection.z * MoveSpeed) * Time.deltaTime * _sprintMultiplier;

        _rb.velocity = velocity;
    }

    #region Input System Functions

    private void ToggleSprint(InputAction.CallbackContext context)
        {
            _isSprinting = !_isSprinting;
        }
    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded || _canDoubleJump)
        {
            _ySpeed = _jumpForce.y;
            _isGrounded = false;
            if (_canDoubleJump && _jumpEnded)
            {
                _canDoubleJump = false;
            }
        }
    }
    private void JumpEnded(InputAction.CallbackContext context)
    {
        _jumpEnded = true;
    }
    private void Dash(InputAction.CallbackContext context)
    {
        Vector3 forceToApply = transform.forward * DashForce + transform.up * DashUpwardForce;

        _rb.AddForce(forceToApply, ForceMode.Impulse);

        Invoke(nameof(ResetDash), DashDuration);
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
        _iJump.performed += Jump;
        _iJump.canceled += JumpEnded;
        _iJump.Enable();

        _iDash = PlayerControls.Player.Dash;
        _iDash.performed += Dash;
        _iDash.Enable();

        _iSprint = PlayerControls.Player.Sprint;
        _iSprint.performed += ToggleSprint;
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

    private void RotatePlayer()
    {
        //Debug.DrawRay(transform.position, transform.forward, Color.red,1000);
        //if (_moveDirection == Vector3.zero)return;
        var targetAngle = MathF.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f , angle , 0f );   
         
        if (_moveDirection == Vector3.zero)return;
        _moveDirection = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;
    }

    #region Collisions

    private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Ground"))
            {
                _isGrounded = true;
                _canDoubleJump = true;
                _jumpEnded = false;
                Debug.Log("Bruh");
            }
        }

    #endregion
    
}
