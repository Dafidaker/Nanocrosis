using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Enums;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    private Controls PlayerControls { get; set; }

    [Header("Debug Variables"), Space(10)] 
    public bool isGrounded;
    public GameObject debugGameObject;
    public CinemachineFreeLook CinemachineFreeLook;
    public CinemachineVirtualCamera CinemachineVirtual;
    
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
    private Coroutine _momentumCoroutine;
    
    [Header("Rotate Player"), Space(10)] 
    public Transform cam;

    [Header("Slope Handling"), Space(10)] 
    public float minSlopeAngle;
    public float slopeDragMultiplier;
    private RaycastHit _slopeHit;

    [Header("Jump Buffering / Coyote Time"), Space(10)]
   //Jump Buffering
    public float jumpBufferingDuration;
    private float _jumpInputTimer;
    private bool _jumpWasPressed;
    //Coyote Time
    public float coyoteDuration;
    private float _canJumpTimer;
    private bool _canJump;

    //Keeping Momentum
    private float _speedChangeFactor;
    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private MovementState _lastMovementState;
    private bool _keepMomentum;

    [Header("Detect Ground"), Space(10)] 
    [field: SerializeField] private LayerMask layerMask;

    [Header("Weapon Variables"), Space(10)]
    [SerializeField] private Transform FirePoint;
    [SerializeField] private Transform BulletParent;
    [SerializeField] private float MissDistance = 25f;
    [SerializeField] private GameObject[] Weapons;
    [SerializeField] private GameObject BombPrefab;
    [SerializeField] private LayerMask bulletsHit;
    [SerializeField] private WeaponInfoController WeaponUI;
    [SerializeField] private PickupInfoController PickupUI;
    public GameObject FakeBomb;
    public bool BombAttached;
    public int AvailableBuffs;
    private int _hitShieldAmount;
    
    
    [Header("Aim"), Space(10)]
    [SerializeField] private float mouseYSentivity;
    [SerializeField] private float mouseXSentivity;
    [SerializeField] private GameObject mesh;
    [SerializeField] private float yThreshold;
    private float _camXSpeed;
    private float _camYSpeed;
    
    public GameObject CurrentWeapon;
    private int _currrentWeaponIndex = 0;
    private Coroutine _shooting;
    private float _fireCountdown;

    [Header("Melee Attack Variables"), Space(10)]
    [SerializeField] private GameObject MeleeAttack;
    [SerializeField] private Transform MeleeAttackInitialPos;

    [Header("Health System"), Space(10)]
    [SerializeField] private HealthBarController HealthBar;
    [SerializeField] private Transform RespawnPoint;
    [SerializeField] private GameObject AliveUI;
    [SerializeField] private GameObject DeadUI;
    public float TimeToRepair;

    //animations
    private Animator _animator;
    
    
    
    private float _meleeCountdown;

    private Vector3 _delayedForceToApply;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    [HideInInspector] public Vector3 moveDirection;
    private Rigidbody _rb;

    private PlayerStats _playerStats;
    
    
    //Input actions.
    private InputAction _iLook;
    private InputAction _iMove;
    private InputAction _iJump;
    private InputAction _iDash;
    private InputAction _iSprint;
    private InputAction _iMelee;
    private InputAction _iShoot;
    private InputAction _iCycle;
    private InputAction _iHeal;
    private InputAction _iCritical;
    private InputAction _iReload;
    private InputAction _iInteract;
    private InputAction _iEnhanceAmmo;

    //Item interactions.
    private GameObject _currentBomb;

    private bool _damaged;
    public bool dying;
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Airborne = Animator.StringToHash("Airborne");
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int Dashed = Animator.StringToHash("Dashed");
    private static readonly int Shooting = Animator.StringToHash("Shooting");
    private static readonly int OneTime = Animator.StringToHash("OneTime");
    private static readonly int ShootSpread = Animator.StringToHash("ShootSpread");
    private static readonly int Change = Animator.StringToHash("Change");

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
        Instance = this;
        //Settings.GameStart(); //todo change this to a game controller or something :)
        CurrentWeapon = Weapons[_currrentWeaponIndex];
        FirePoint = CurrentWeapon.GetComponent<WeaponController>().FirePoint;
        FakeBomb.SetActive(false);
    }
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _animator = GetComponent<Animator>();
        
        CurrentWeapon.SetActive(true);

        _fireCountdown = CurrentWeapon.GetComponent<WeaponController>().FireRate;
        
        Cursor.lockState = CursorLockMode.Locked;
        //debugGameObject = Instantiate(debugGameObject);

        _currentJumps = 0;

        _playerStats = GetComponent<PlayerStats>();

        
        _camXSpeed = CinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed;
        _camYSpeed = CinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed;
        

        UpdateCamSentivity();
    }
    
    private void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward * 10 , Color.red);
        
        if (GameManager.Instance.gamePaused) return;

        Vector2 moveInput = _iMove.ReadValue<Vector2>();

        var isRunning = moveInput != Vector2.zero;
        _animator.SetBool(Running, isRunning);
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsTag("Running"))
        {
            _animator.SetTrigger(Change);
        }
        
        //moveInput = transform.InverseTransformDirection(moveInput);
        
        _horizontalInput = moveInput.x;
        _verticalInput = moveInput.y;

        if (CinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value <= yThreshold)
        {
            mesh.SetActive(false);
        }
        else
        {
            mesh.SetActive(true);
        }
        
        /*_horizontalInput = _iMove.ReadValue<Vector2>().x;
        _verticalInput = _iMove.ReadValue<Vector2>().y;*/
        
        Hacks();
        
        //adds drag if player is grounded
        if (_movementState == MovementState.Sprint || _movementState == MovementState.Walk)
        {
            _rb.drag = groundDrag;
            if (OnSlope()) { _rb.drag = groundDrag * slopeDragMultiplier;}
        }
        else { _rb.drag = 0; }
        
        //limits the player speed
        SpeedControl();

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
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
        
        if (_jumpInputTimer > 0) _jumpInputTimer -= Time.deltaTime;
        else _jumpWasPressed = false;

        if (_canJumpTimer > 0) _canJumpTimer -= Time.deltaTime;
        else _canJump = false;
        
        //if the jump was pressed in the (jumpbuffering) duration and the player can jump they jump
        if (_jumpWasPressed && CanJump()) Jump();

        /*//if they can jump it saves that they can jump for the coyoteDuration
        if (CanJump() && !_canJump)
        {
            _canJump = true;
            Invoke(nameof(EndCoyoteTime), coyoteDuration);
        }*/

        if(_fireCountdown > 0) _fireCountdown -= Time.deltaTime;
        if(_meleeCountdown > 0) _meleeCountdown -= Time.deltaTime;

        if (CanJump())
        {
            _canJump = true;
            _canJumpTimer = coyoteDuration;
        }
        
        //Debug.DrawRay(cam.position, cam.transform.forward* 100f, Color.red, 0.1f);
    }

    private void FixedUpdate()
    {
        DetectGround();
        
        MovePlayer();
        
        //rotates the player to the direction of the camera if the camera is moving
        RotatePlayer();
    }

    #endregion
    
    #region Funtions

    #region Movement

    private void MovePlayer()
    {
        var playerTransform = transform;
        moveDirection = playerTransform.forward * _verticalInput + playerTransform.right * _horizontalInput;

        //on a slope
        if (OnSlope())
        {
            _rb.AddForce(GetSlopeMoveDirection(moveDirection).normalized * (_moveSpeed * 10f), ForceMode.Force);
        }
        else if (isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * (_moveSpeed * 10f), ForceMode.Force); 
        }
        else if (!isGrounded)
        {
            _rb.AddForce(moveDirection.normalized * (_moveSpeed * 10f * airMultiplier), ForceMode.Force); 
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
        _animator.SetTrigger(Change);
        _animator.SetBool(Jumping, true);
        _readyToJump = false;
        _jumpWasPressed = false;
        isJumpOver = false;
        
        var velocity = _rb.velocity;
        _rb.velocity = new Vector3(velocity.x, 0, velocity.z);
        
        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        _currentJumps++;
        
        Invoke(nameof(ResetJump), jumpCooldown);
    }
    private void ResetJump()
    {
        _readyToJump = true;
    }

    private void Dash()
    {
        //if the cooldown hasn't ended just returns
        if (_dashCooldownTimer > 0) return;
        
        _animator.SetTrigger(Change);
        _animator.SetTrigger(OneTime);
        _animator.SetTrigger(Dashed);
        
        //if the dash goes through the timer gets as big as the cooldown
        _dashCooldownTimer = dashCooldown;

        _dashing = true;
        
        Vector3 dashDirection = Vector3.zero;
        
        if (Settings.DashMovementDirection)
        {
            dashDirection = moveDirection;
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
    
    #region  Shooting

    private void GetTargetBomb(RaycastHit hit, BombController bombController, GameObject bomb)
    {
        if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity,bulletsHit ))
        {
            bombController.Target = hit.point;
            bombController.Hit = true;
            Debug.DrawRay(FirePoint.position, bomb.transform.forward, Color.red, 2f);
        }
        else
        {
            bombController.Target = cam.position + cam.forward * MissDistance;
            bombController.Hit = false;
            Debug.DrawRay(FirePoint.position, bomb.transform.forward, Color.red, 2f);
        }
    }
    
    private void GetTargetBullet(RaycastHit hit, BulletController bulletController, GameObject bullet)
    {
        if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity,bulletsHit))
        {
            bulletController.Target = hit.point;
            bulletController.Hit = true;
            Debug.DrawRay(FirePoint.position, bullet.transform.forward, Color.red, 2f);
        }
        else
        {
            bulletController.Target = bullet.transform.position + bullet.transform.forward * MissDistance;
            bulletController.Hit = false;
            Debug.DrawRay(FirePoint.position, bullet.transform.forward, Color.red, 2f);
        }
    }
    
    private IEnumerator FullAuto()
    {
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();

        while (!weaponController.Reloading)
        {
            SemiAutoFire();
            WeaponUI.SetValues();
            if (weaponController.CurrentMag <= weaponController.MagSize * 0.3f) WeaponUI.LowAmmo();
            yield return new WaitForSeconds(weaponController.FireRate);
        }
    }
    private void SingleFire(WeaponController w)
    {
        if (_fireCountdown > 0) return;

        _fireCountdown = w.FireRate;

        SemiAutoFire();

        WeaponUI.SetValues();
        if (w.CurrentMag <= w.MagSize * 0.3f) WeaponUI.LowAmmo();
    }
    private void SemiAutoFire()
    {
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();

        if (weaponController.CurrentMag <= 0 && _shooting != null)
        {
            Invoke(nameof(StopShootingAnimation), 1f);
            StopCoroutine(_shooting);
            return;
        }

        weaponController.CurrentMag--;

        RaycastHit hit = default;

        GameObject bullet;

        if (weaponController.FullAuto)
        {
            _animator.SetTrigger(OneTime);
            _animator.SetBool(Shooting , true);
            _animator.SetTrigger(Change);
            
            bullet = Instantiate(weaponController.CurrentBulletPrefab, FirePoint.position, Quaternion.LookRotation(cam.forward)); // BulletParent
            BulletController bulletController = bullet.GetComponent<BulletController>();

            GetTargetBullet(hit, bulletController, bullet);

        }

        else if (weaponController.SpreadShot) //bullet = GameObject.Instantiate(weaponController.BulletPrefab, FirePoint.position, Quaternion.LookRotation(cam.forward), BulletParent);
        {
            _animator.SetTrigger(OneTime);
            _animator.SetTrigger(ShootSpread);
            _animator.SetTrigger(Change);
            int i = 0;
            foreach (Quaternion q in weaponController.Pellets.ToArray())
            {
                weaponController.Pellets[i] = Random.rotation;
                
                //cam forward
                bullet = Instantiate(weaponController.CurrentBulletPrefab, FirePoint.position, 
                    Quaternion.LookRotation(weaponController.transform.forward), BulletParent); //(weaponController.CurrentBulletPrefab, FirePoint.position, Quaternion.LookRotation(weaponController.transform.forward), BulletParent);
                
                //bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, weaponController.Pellets[i], weaponController.SpreadAngle);

                BulletController b = bullet.GetComponent<BulletController>();

                GetTargetBullet(hit, b, bullet);
                
                bullet.transform.rotation = Quaternion.LookRotation((b.Target - FirePoint.position));
                Debug.DrawRay(FirePoint.position, (b.Target - FirePoint.position) *100 , Color.yellow , 20f);
                Debug.DrawLine(FirePoint.position, b.Target , Color.magenta , 20f);
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, weaponController.Pellets[i], weaponController.SpreadAngle);
                i++;
            }
        }
    }
    
    private IEnumerator ReloadCountdown(WeaponController w)
    {
        PauseMenuController.Instance.Reload(w.ReloadTime);
        
        w.Reloading = true;

        //Changes colour
        foreach (GameObject bit in w.ColoredBits)
        {
            bit.GetComponent<MeshRenderer>().material = w.ReloadMaterial;
        }

        int ammoNeeded = w.MagSize - w.CurrentMag;

        if (ammoNeeded > w.CurrentAmmoReserve)
        {
            ammoNeeded = w.CurrentAmmoReserve;
        }

        yield return new WaitForSeconds(w.ReloadTime);

        w.CurrentAmmoReserve -= ammoNeeded;
        w.CurrentMag += ammoNeeded;
        
        //Changes colour
        foreach (GameObject bit in w.ColoredBits)
        {
            if(!w.IsEnhanced) bit.GetComponent<MeshRenderer>().material = w.NormalMaterial;
            else bit.GetComponent<MeshRenderer>().material = w.EnhancedMaterial;
        }

        WeaponUI.SetValues();
        if (w.CurrentMag > w.MagSize * 0.3f) WeaponUI.ResetMagColor();
        else WeaponUI.LowAmmo();

        if (w.CurrentAmmoReserve <= w.AmmoReserve * 0.3) WeaponUI.LowReserve();

        w.Reloading = false;
    }

    private void Reload()
    {
        if (GameManager.Instance.gamePaused)
        {
            return;
        } 
        
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();

        if (weaponController.CurrentMag < weaponController.MagSize && weaponController.CurrentAmmoReserve > 0 && !weaponController.Reloading)
        {
            StartCoroutine(ReloadCountdown(weaponController));
            WeaponUI.ResetEnhancedWeapon();
        }
        else if (weaponController.CurrentMag == weaponController.MagSize) Debug.Log("MAG FULL");
        else if (weaponController.CurrentAmmoReserve <= 0)
        {
            Debug.Log("OUT OF AMMO");
            DoctorManager.Instance.AddToQueue("OutOfAmmo");
            StartCoroutine(WeaponUI.FlashReserve());
        }

        foreach (GameObject bit in weaponController.ColoredBits)
        {
            if (weaponController.Reloading) bit.GetComponent<MeshRenderer>().material = weaponController.ReloadMaterial;
            else bit.GetComponent<MeshRenderer>().material = weaponController.NormalMaterial;
        }
        
        weaponController.IsEnhanced = false;
    }
    
    public void HitEnemyShield()
    {
        _hitShieldAmount++;
        if (_hitShieldAmount < 5) return;
        _hitShieldAmount = 0;
        DoctorManager.Instance.AddToQueue("HitEnemyWithShield");
    }
    
    private void StopShootingAnimation()
    {
        _animator.SetBool(Shooting, false);
    }
    #endregion

    #region other

    private void RotatePlayer()
    {
        if (!_iLook.triggered) return;
        
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
                if (_momentumCoroutine != null) { StopCoroutine(_momentumCoroutine); }
                _momentumCoroutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                if (_momentumCoroutine != null) { StopCoroutine(_momentumCoroutine); }
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

            if (!isGrounded)
            {
                _animator.SetTrigger(Change);
            }
            _animator.SetBool(Airborne, !isGrounded);
            
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
        
    private bool CanJump()
        {
            //if the player isn't _readyToJump or
            //the nº of jumps that they took are higher or equal to the amount allowed it returns
            if (!_readyToJump || _currentJumps >= numberOfJumps) return false;
            
            //if its your first jump and you arent on the ground u cant jump 
            if (_currentJumps == 0  && !isGrounded) return false;
    
            return true;
        }

    private void UpdateCamSentivity()
    {
        CinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _camXSpeed * mouseXSentivity;
        CinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = _camYSpeed * mouseYSentivity;
        
    }

    #endregion
    
    
    #endregion

    
    
    #region Input System Functions
    
    private void SprintInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
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
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        
        if (!Settings.IsSprintToggle)
        {
            _isSprinting = false;
        }
    }
    
    private void JumpInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("Jump while game paused");
            return;
        }
        
        _jumpWasPressed = true;
        _jumpInputTimer = jumpBufferingDuration;
        
        //if (!CanJump()) return;
        
        if (!_canJump) return;
        
        Jump();
        
    }
    private void JumpEndedInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        
        _animator.SetBool(Jumping, false);
        
        isJumpOver = true;
    }
    
    private void DashInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        Dash();
    }
    
    private void Shoot(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            //Debug.Log("GAME IS PAUSED");
            return;
        }
        
        _isSprinting = false;
        
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();

        RaycastHit hit = default;

        GameObject bomb;
        
        if (!BombAttached)
        {
            if (weaponController.CurrentMag <= 0 && !weaponController.Reloading)
            {
                //Changes colour
                /*foreach (GameObject bit in weaponController.ColoredBits)
                {
                    bit.GetComponent<MeshRenderer>().material = weaponController.EmptyMagMaterial;
                }
                StartCoroutine(WeaponUI.FlashMag());*/
                Reload();
                return;
            }

            if (weaponController.Reloading) return;

            _animator.SetBool(Shooting, true);
            if (!weaponController.FullAuto) SingleFire(weaponController);
            else _shooting = StartCoroutine(FullAuto());
        }
        else
        {
            bomb = Instantiate(BombPrefab, FirePoint.position, Quaternion.LookRotation(cam.forward));
            Debug.DrawRay(FirePoint.position, bomb.transform.forward, Color.red, 2f);
            BombController bombController = bomb.GetComponent<BombController>();

            GetTargetBomb(hit, bombController, bomb);
            FakeBomb.SetActive(false);
            BombAttached = false;
            WeaponUI.BombUnattached();
        }
    }
    private void ShootEndedInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            return;
        }
        
        Invoke(nameof(StopShootingAnimation), 1f);
        if (_shooting != null) StopCoroutine(_shooting);
    }
    
    private void ReloadInput(InputAction.CallbackContext context)
    {
        Reload();
    }
    private void CycleWeapons(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        StopCoroutine(WeaponUI.FlashMag());
        StopCoroutine(WeaponUI.FlashReserve());
        CurrentWeapon.SetActive(false);
        _currrentWeaponIndex++;
        if (_currrentWeaponIndex >= Weapons.Length)
        {
            _currrentWeaponIndex = 0;
            CurrentWeapon.SetActive(false);
            CurrentWeapon = Weapons[_currrentWeaponIndex];
            CurrentWeapon.SetActive(true);
        }
        CurrentWeapon = Weapons[_currrentWeaponIndex];
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();
        CurrentWeapon.SetActive(true);
        StopCoroutine(_shooting);
        WeaponUI.SetValues();
        if (weaponController.CurrentMag > weaponController.MagSize * 0.3f) WeaponUI.ResetMagColor();
        else WeaponUI.LowAmmo();
        if (weaponController.CurrentAmmoReserve > weaponController.AmmoReserve * 0.3f) WeaponUI.ResetReserveColor();
        else WeaponUI.LowReserve();
        if (weaponController.IsEnhanced && !(weaponController.CurrentMag <= weaponController.MagSize * 0.3f)) WeaponUI.EnhancedWeapon();
        else if (weaponController.IsEnhanced && (weaponController.CurrentMag <= weaponController.MagSize * 0.3f))
        {
            WeaponUI.EnhancedWeapon();
            WeaponUI.LowAmmo();
        }
        if (!weaponController.IsEnhanced)
        {
            WeaponUI.ResetEnhancedWeapon();
            if (weaponController.CurrentMag <= weaponController.MagSize * 0.3f) WeaponUI.LowAmmo();
        }
    }
    private void Interact(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        if (_currentBomb != null)
        {
            GetBomb();
        }
    }
    private void EnhanceWeapon(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();
        if (AvailableBuffs > 0 && weaponController.CurrentMag > 0 && !weaponController.IsEnhanced)
        {
            weaponController.IsEnhanced = true;

            foreach (GameObject bit in weaponController.ColoredBits)
            {
                if(!weaponController.Reloading) bit.GetComponent<MeshRenderer>().material = weaponController.EnhancedMaterial;
            }

            AvailableBuffs--;
            PickupUI.SetValue();
            WeaponUI.EnhancedWeapon();
        }
        else if(weaponController.CurrentMag <= 0)
        {
            Debug.Log("CAN'T ENHANCED WITH AN EMPTY MAG");
            StartCoroutine(WeaponUI.FlashMag());
        }
        else if (AvailableBuffs <= 0)
        {
            Debug.Log("NO BUFFS AVAILABLE");
            StartCoroutine(PickupUI.Flash());
        }
        else if (weaponController.IsEnhanced)
        {
            Debug.Log("THIS WEAPON IS ALREADY ENHANCED");
        }
    }

    private void PerformMeleeAttack(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gamePaused)
        {
            Debug.Log("GAME IS PAUSED");
            return;
        }
        if (_meleeCountdown > 0) return;

        _meleeCountdown = 0.3f;

        MeleeAttack.SetActive(true);
    }

    public IEnumerator DieAndRespawn()
    {
        GameEvents.Instance.playerDied.Ping(null, (1f,TimeToRepair)); //(float noColorTimer, float deadtimer)
        _animator.SetTrigger(OneTime);
        _animator.SetBool(Dead, true);
        dying = true;
        DisableInputSystem();
        AliveUI.SetActive(false);
        DeadUI.SetActive(true);
        EnemiesDisperse();
        yield return new WaitForSeconds(TimeToRepair);
        CheckEnemyTarget();
        dying = false;
        AliveUI.SetActive(true);
        DeadUI.SetActive(false);
        EnableInputSystem();
        _animator.SetBool(Dead, false);
        _playerStats.CurrentHealth = _playerStats.MaxHealth;
        _playerStats.UpdateColor();
        HealthBar.SetHealth();
    }
    
    
    private void EnemiesDisperse()
    {
        foreach (var enemySpawn in GameManager.Instance.currentArena.enemiesSpawners)
        {
            enemySpawn.enemies ??= new List<GameObject>();

            foreach (var enemy in enemySpawn.enemies.ToArray())
            {
                enemy.GetComponent<Hittable>().Disperse();
            }
        }
    }

    private void CheckEnemyTarget()
    {
        foreach (var enemySpawn in GameManager.Instance.currentArena.enemiesSpawners)
        {
            enemySpawn.enemies ??= new List<GameObject>();

            foreach (var enemy in enemySpawn.enemies.ToArray())
            {
                enemy.GetComponent<Hittable>().CheckTarget();
            }
        }
    }

    public void EnableInputSystem()
    {
        _iMove = PlayerControls.Player.Walk;
        _iMove.Enable();
        
        _iLook = PlayerControls.Player.Look;
        _iLook.Enable();
        
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
        _iMelee.performed += PerformMeleeAttack;
        _iMelee.Enable();

        _iShoot = PlayerControls.Player.Shoot;
        _iShoot.performed += Shoot;
        _iShoot.canceled += ShootEndedInput;
        _iShoot.Enable();

        _iCycle = PlayerControls.Player.Cycle;
        _iCycle.performed += CycleWeapons;
        _iCycle.Enable();

        _iHeal = PlayerControls.Player.Healing;
        _iHeal.Enable();

        _iCritical = PlayerControls.Player.CriticalMesures;
        _iCritical.Enable();

        _iReload = PlayerControls.Player.Reload;
        _iReload.performed += ReloadInput;
        _iReload.Enable();

        _iInteract = PlayerControls.Player.Interact;
        _iInteract.performed += Interact;
        _iInteract.Enable();

        _iEnhanceAmmo = PlayerControls.Player.EnhanceAmmo;
        _iEnhanceAmmo.performed += EnhanceWeapon;
        _iEnhanceAmmo.Enable();

    }
    public void DisableInputSystem()
    {
        _iMove.Disable();
        _iLook.Disable();
        _iJump.Disable();
        _iDash.Disable();
        _iSprint.Disable();
        _iMelee.Disable();
        _iShoot.Disable();
        _iCycle.Disable();
        _iHeal.Disable();
        _iCritical.Disable();
        _iReload.Disable();
        _iInteract.Disable();
        _iEnhanceAmmo.Disable();
    }
    
    #endregion


    private void Hacks()
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) { return; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) // go to lungs 
        {
            GameManager.Instance.ChangeArena(Arena.Lungs);
            transform.position = GameManager.Instance.lungsPlayerSpawnPosition.position;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // go to heart 
        {
            GameManager.Instance.ChangeArena(Arena.Heart);
            transform.position = GameManager.Instance.heartPlayerSpawnPosition.position;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // kill phage 
        {
            GameManager.Instance.phage.GotHit(GameManager.Instance.phage.maxHealth + 10, PlayerAttacks.BulletEnhanced);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) // kill phage shield
        {
            if (GameManager.Instance.phage.shieldIsActive)
            {
                GameManager.Instance.phage.GotHit(GameManager.Instance.phage.maxShieldHealth +10, PlayerAttacks.BulletEnhanced);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) // added lung health
        {
            var add = ObjectiveManager.Instance.maxValue * 0.05f;
            
            if (ObjectiveManager.Instance.currentValue + add <= ObjectiveManager.Instance.maxValue)
            {
                ObjectiveManager.Instance.currentValue += add;
            }
            GameEvents.Instance.lungsHealthChanged.Ping(null, null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) // removed lung health
        {
            ObjectiveManager.Instance.currentValue -= ObjectiveManager.Instance.maxValue * 0.05f;
            
            if (ObjectiveManager.Instance.currentValue <= 0 )
            {
                GameManager.Instance.GameEnded(false);
            }
            
            GameEvents.Instance.lungsHealthChanged.Ping(null, null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7)) // get ammo
        {
            WeaponController w1 = Weapons[0].GetComponent<WeaponController>();
            WeaponController w2 = Weapons[1].GetComponent<WeaponController>();

            // Debug.Log((w1.CurrentAmmoReserve == w1.AmmoReserve) && (w2.CurrentAmmoReserve == w2.AmmoReserve));

            if ((w1.CurrentAmmoReserve == w1.AmmoReserve) && (w2.CurrentAmmoReserve == w2.AmmoReserve))
            {
                Debug.Log("ALL AMMO RESERVES FULL");
                return;
            }
            foreach (GameObject weapon in Weapons)
            {
                WeaponController w = weapon.GetComponent<WeaponController>();
                float AmmoRestored = w.AmmoReserve * 0.15f;
                int RoundedAmmo = Mathf.RoundToInt(AmmoRestored);

                if(w.CurrentAmmoReserve == w.AmmoReserve)
                {
                    Debug.Log(w.Name + ": AMMO RESERVES FULL");
                }

                else if (RoundedAmmo + w.CurrentAmmoReserve > w.AmmoReserve)
                {
                    w.CurrentAmmoReserve = w.AmmoReserve;
                }
                else w.CurrentAmmoReserve += RoundedAmmo;
                WeaponUI.SetValues();
                if (w.CurrentAmmoReserve > w.AmmoReserve * 0.3f) WeaponUI.ResetReserveColor();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) // get special ammo
        {
            AvailableBuffs += 2;
            PickupUI.SetValue();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9)) // get bomb
        {
            _currentBomb = Instantiate(GameManager.Instance.bomb);
            _currentBomb.GetComponent<BombPickupController>().Interact();
            WeaponUI.BombAttached();
        }
    }
    
    private void OnGUI()
    {
        /*GUI.Box(new Rect(0, 0, Screen.width * 0.2f, Screen.height * 0.1f), _rb.velocity.ToString());
        GUI.Box(new Rect(0, 50, Screen.width * 0.2f, Screen.height * 0.1f), "can jump: " + _canJump);
        GUI.Box(new Rect(0, 100, Screen.width * 0.2f, Screen.height * 0.1f), "_jumpWasPressed: " + _jumpWasPressed);*/

        //Weapons
        /*GUI.Box(new Rect(0, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.1f), CurrentWeapon.GetComponent<WeaponController>().Name);
        GUI.Box(new Rect(0, Screen.height * 0.2f, Screen.width * 0.2f, Screen.height * 0.1f), CurrentWeapon.GetComponent<WeaponController>().CurrentMag.ToString());
        GUI.Box(new Rect(0, Screen.height * 0.3f, Screen.width * 0.2f, Screen.height * 0.1f), CurrentWeapon.GetComponent<WeaponController>().CurrentAmmoReserve.ToString());
        GUI.Box(new Rect(0, Screen.height * 0.4f, Screen.width * 0.2f, Screen.height * 0.1f), "Magazine Empty: " +  CurrentWeapon.GetComponent<WeaponController>().MagEmpty);
        GUI.Box(new Rect(0, Screen.height * 0.5f, Screen.width * 0.2f, Screen.height * 0.1f), "Out of Ammo: " + CurrentWeapon.GetComponent<WeaponController>().OutOfAmmo);
        GUI.Box(new Rect(0, Screen.height * 0.6f, Screen.width * 0.2f, Screen.height * 0.1f), "Reloading: " + CurrentWeapon.GetComponent<WeaponController>().Reloading);
        GUI.Box(new Rect(0, Screen.height * 0.7f, Screen.width * 0.2f, Screen.height * 0.1f), "Available Buffs: " + AvailableBuffs);*/
    }

    #region Colliding
    private bool GetAmmo()
    {
        WeaponController w1 = Weapons[0].GetComponent<WeaponController>();
        WeaponController w2 = Weapons[1].GetComponent<WeaponController>();

        if ((w1.CurrentAmmoReserve == w1.AmmoReserve) && (w2.CurrentAmmoReserve == w2.AmmoReserve))
        {
            Debug.Log("ALL AMMO RESERVES FULL");
            return false;
        }

        foreach (GameObject weapon in Weapons)
        {
            WeaponController w = weapon.GetComponent<WeaponController>();
            float AmmoRestored = w.AmmoReserve * 0.15f;
            int RoundedAmmo = Mathf.RoundToInt(AmmoRestored);

            if (w.CurrentAmmoReserve == w.AmmoReserve)
            {
                Debug.Log(w.Name + ": AMMO RESERVES FULL");
            }

            else if (RoundedAmmo + w.CurrentAmmoReserve > w.AmmoReserve)
            {
                w.CurrentAmmoReserve = w.AmmoReserve;
            }
            else w.CurrentAmmoReserve += RoundedAmmo;

            WeaponUI.SetValues();
            if (w.CurrentAmmoReserve > w.AmmoReserve * 0.3f) WeaponUI.ResetReserveColor();

        }
        return true;
    }

    private void GetSpecialAmmo()
    {
        AvailableBuffs += 2;
        PickupUI.SetValue();
    }

    private void GetBomb()
    {
        _currentBomb.GetComponent<BombPickupController>().Interact();
        WeaponUI.BombAttached();
    }

    
    #endregion

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag($"Bomb"))
        {
            _currentBomb = col.gameObject;
        }

        if (col.CompareTag($"BuffPuddle"))
        {
            if (AvailableBuffs < 1)
            {
                AvailableBuffs += 1;
                PickupUI.SetValue();
            }
        }

        if (col.CompareTag($"AmmoBuff"))
        {
            AvailableBuffs += 2;
            PickupUI.SetValue();
            Destroy(col.gameObject);
        }

        if (col.CompareTag($"AmmoPickup"))
        {
            WeaponController w1 = Weapons[0].GetComponent<WeaponController>();
            WeaponController w2 = Weapons[1].GetComponent<WeaponController>();

            if ((w1.CurrentAmmoReserve == w1.AmmoReserve) && (w2.CurrentAmmoReserve == w2.AmmoReserve))
            {
                Debug.Log("ALL AMMO RESERVES FULL");
                return;
            }
            foreach (GameObject weapon in Weapons)
            {
                WeaponController w = weapon.GetComponent<WeaponController>();
                float AmmoRestored = w.AmmoReserve * 0.15f;
                int RoundedAmmo = Mathf.RoundToInt(AmmoRestored);

                if(w.CurrentAmmoReserve == w.AmmoReserve)
                {
                    Debug.Log(w.Name + ": AMMO RESERVES FULL");
                }

                else if (RoundedAmmo + w.CurrentAmmoReserve > w.AmmoReserve)
                {
                    w.CurrentAmmoReserve = w.AmmoReserve;
                }
                else w.CurrentAmmoReserve += RoundedAmmo;
                WeaponUI.SetValues();
                if (w.CurrentAmmoReserve > w.AmmoReserve * 0.3f) WeaponUI.ResetReserveColor();
                Destroy(col.gameObject);
            }
        }

        if (col.CompareTag("ItemTreeTrigger"))
        {
            var controller = col.gameObject.GetComponent<ItemTreeBranchController>();
            var itemtype = controller.itemSpawn.item;
            
            switch (itemtype)
            {
                default:
                    return;
                case ItemType.Ammo:
                    if (GetAmmo())
                    {
                        controller.RemovedItem();
                    }
                    break;
                case ItemType.Explosive:
                    controller.RemovedItem();
                    GetBomb();
                    break;
                case ItemType.SpecialAmmo:
                    controller.RemovedItem();
                    GetSpecialAmmo();
                    break;
            }
        }
        /*if (col.CompareTag($"DangerZone"))
        {
            if (_damaged) return;
            _playerStats.CurrentHealth -= 5;
            HealthBar.SetHealth();
            _damaged = true;
            _playerStats.CurrentHealth -= 5;
            HealthBar.SetHealth();
        }*/                                                                                                                                                                                                                                                                                                                     
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag($"DangerZone"))
        {
            Debug.Log("DAMAGE");
            /*if (_damaged) return;
            _playerStats.CurrentHealth -= 5;
            HealthBar.SetHealth();
            _damaged = true;*/
            _playerStats.DamageTaken(1);
            HealthBar.SetHealth();
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag($"Bomb"))
        {
           _currentBomb = null;
        }

        if (col.CompareTag($"DangerZone"))
        {
            _damaged = false;
        }
    }
}