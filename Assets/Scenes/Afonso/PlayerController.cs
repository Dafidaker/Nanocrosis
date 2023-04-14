using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
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
    
    
    [Header("Aim"), Space(10)]
    [SerializeField] private float mouseYSentivity;
    [SerializeField] private float mouseXSentivity;
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

    private float _meleeCountdown;

    private Vector3 _delayedForceToApply;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private Vector3 _moveDirection;
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
        Settings.GameStart(); //todo change this to a game controller or something :)
        CurrentWeapon = Weapons[_currrentWeaponIndex];
        FirePoint = CurrentWeapon.GetComponent<WeaponController>().FirePoint;
        FakeBomb.SetActive(false);
    }
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        CurrentWeapon.SetActive(true);

        _fireCountdown = CurrentWeapon.GetComponent<WeaponController>().FireRate;
        
        Cursor.lockState = CursorLockMode.Locked;
        //debugGameObject = Instantiate(debugGameObject);

        _currentJumps = 0;

        _playerStats = GetComponent<PlayerStats>();

        
        /*_camXSpeed = CinemachineVirtual.GetComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed;
        _camYSpeed = CinemachineVirtual.GetComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed;*/

        _camXSpeed = 0.05f;
        _camYSpeed =  0.05f;

        UpdateCamSentivity();
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
            StopCoroutine(_shooting);
            return;
        }

        weaponController.CurrentMag--;

        RaycastHit hit = default;

        GameObject bullet;

        if (weaponController.FullAuto)
        {
            bullet = Instantiate(weaponController.CurrentBulletPrefab, FirePoint.position, Quaternion.LookRotation(cam.forward)); // BulletParent
            BulletController bulletController = bullet.GetComponent<BulletController>();

            GetTargetBullet(hit, bulletController, bullet);

        }

        else if (weaponController.SpreadShot) //bullet = GameObject.Instantiate(weaponController.BulletPrefab, FirePoint.position, Quaternion.LookRotation(cam.forward), BulletParent);
        {
            int i = 0;
            foreach (Quaternion q in weaponController.Pellets.ToArray())
            {
                weaponController.Pellets[i] = Random.rotation;
                bullet = Instantiate(weaponController.CurrentBulletPrefab, FirePoint.position, Quaternion.LookRotation(cam.forward), BulletParent);
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, weaponController.Pellets[i], weaponController.SpreadAngle);

                BulletController b = bullet.GetComponent<BulletController>();

                GetTargetBullet(hit, b, bullet);
                i++;
            }
        }
    }
    
    private IEnumerator ReloadCountdown(WeaponController w)
    {
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
    
    #endregion

    #region other

    private void RotatePlayer()
        {
            //var targetAngle = MathF.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
             
            
            if (!_iLook.triggered) return;
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
        _jumpWasPressed = true;
        _jumpInputTimer = jumpBufferingDuration;
        
        //if (!CanJump()) return;
        
        if (!_canJump) return;
        
        Jump();
        
    }
    private void JumpEndedInput(InputAction.CallbackContext context)
    {
        isJumpOver = true;
    }
    
    private void DashInput(InputAction.CallbackContext context)
    {
        Dash();
    }
    
    private void Shoot(InputAction.CallbackContext context)
    {
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();

        RaycastHit hit = default;

        GameObject bomb;
        
        if (!BombAttached)
        {
            if (weaponController.CurrentMag <= 0 && !weaponController.Reloading)
            {
                //Changes colour
                foreach (GameObject bit in weaponController.ColoredBits)
                {
                    bit.GetComponent<MeshRenderer>().material = weaponController.EmptyMagMaterial;
                }
                StartCoroutine(WeaponUI.FlashMag());
                return;
            }

            if (weaponController.Reloading) return;

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
         if(_shooting != null) StopCoroutine(_shooting);
    }
    
    private void Reload(InputAction.CallbackContext context)
    {
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();

        if (weaponController.CurrentMag < weaponController.MagSize && weaponController.CurrentAmmoReserve > 0 && !weaponController.Reloading)
        {
            StartCoroutine(ReloadCountdown(weaponController));
        }
        else if (weaponController.CurrentMag == weaponController.MagSize) Debug.Log("MAG FULL");
        else if (weaponController.CurrentAmmoReserve <= 0)
        {
            Debug.Log("OUT OF AMMO");
            StartCoroutine(WeaponUI.FlashReserve());
        }

        foreach (GameObject bit in weaponController.ColoredBits)
        {
            if (weaponController.Reloading) bit.GetComponent<MeshRenderer>().material = weaponController.ReloadMaterial;
            else bit.GetComponent<MeshRenderer>().material = weaponController.NormalMaterial;
        }

        weaponController.IsEnhanced = false;
    }
    private void CycleWeapons(InputAction.CallbackContext context)
    {
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
    }
    private void Interact(InputAction.CallbackContext context)
    {
        if(_currentBomb != null)
        {
            _currentBomb.GetComponent<BombPickupController>().Interact();
            WeaponUI.BombAttached();
        }
    }
    private void EnhanceWeapon(InputAction.CallbackContext context)
    {
        WeaponController weaponController = CurrentWeapon.GetComponent<WeaponController>();
        if (AvailableBuffs > 0)
        {
            weaponController.IsEnhanced = true;

            foreach (GameObject bit in weaponController.ColoredBits)
            {
                if(!weaponController.Reloading) bit.GetComponent<MeshRenderer>().material = weaponController.EnhancedMaterial;
            }

            AvailableBuffs--;
            PickupUI.SetValue();
        }
        else
        {
            Debug.Log("NO BUFFS AVAILABLE");
            StartCoroutine(PickupUI.Flash());
        }
    }

    private void PerformMeleeAttack(InputAction.CallbackContext context)
    {
        if (_meleeCountdown > 0) return;

        _meleeCountdown = 0.3f;

        MeleeAttack.SetActive(true);
    }

    public IEnumerator DieAndRespawn()
    {
        dying = true;
        DisableInputSystem();
        AliveUI.SetActive(false);
        DeadUI.SetActive(true);
        yield return new WaitForSeconds(TimeToRepair);
        dying = false;
        AliveUI.SetActive(true);
        DeadUI.SetActive(false);
        EnableInputSystem();
        _playerStats.CurrentHealth = _playerStats.MaxHealth;
        HealthBar.SetHealth();
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
        _iReload.performed += Reload;
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
            else Debug.Log("Collect Pickup to go above 1");
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

            Debug.Log((w1.CurrentAmmoReserve == w1.AmmoReserve) && (w2.CurrentAmmoReserve == w2.AmmoReserve));

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