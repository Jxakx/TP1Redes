using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine.UI;
using System;

public class Player : NetworkBehaviour, IDamageable
{
    [Header("Player Settings")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _speed;
    [SerializeField] private float _speedRotation;
    [SerializeField] private Animator _animator;
    public MeshRenderer _myAlas;
    public MeshRenderer _myCamina;

    [Header("Dash Settings")]
    [SerializeField] private float _dashDistance = 5f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private float _invulnerabilityDuration = 0.3f;
    [SerializeField] private TrailRenderer _dashTrail;
    [Networked] private TickTimer _dashCooldownTimer { get; set; }
    [Networked] private TickTimer _dashDurationTimer { get; set; }
    [Networked] private TickTimer _invulnerabilityTimer { get; set; }
    [Networked] private bool _isDashing { get; set; }
    [Networked] private bool _isInvulnerable { get; set; }

    [Header("Shooting")]
    [SerializeField] private bool _ShootBulletActivate;
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private Transform _spawnTransformLeft;
    [SerializeField] private Transform _spawnTransformRight;
    [SerializeField] private bool _doubleShotActive { get; set; }
    [SerializeField] private TickTimer _doubleShotTimer { get; set; }
    public bool _borrar;

    [Header("Force Field")]
    [SerializeField] private bool _canUseForce = true;
    [SerializeField] private bool _forceActivate;
    [SerializeField] private float _lifeTimeForce;
    [SerializeField] private float _lifeTimeForceCount;
    [SerializeField] private GameObject _force;
    [SerializeField] private Material _material;

    [Header("UI")]
    [SerializeField] private RawImage _barLife;
    [SerializeField] private Image _dashCooldownUI;

    private NetworkRigidbody3D _netRB;
    private float _xAxis;
    private float _zAxis;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        _netRB = GetComponent<NetworkRigidbody3D>();
        _currentHealth = _maxHealth;
        GameManager.Instance.AddToList(this);

        if (_dashTrail != null)
            _dashTrail.emitting = false;
    }

    void Update()
    {
        if (!HasStateAuthority) return;

        HandleInput();
        UpdateDashCooldownUI();
    }

    private void HandleInput()
    {
        _xAxis = Input.GetAxis("Horizontal");
        _zAxis = Input.GetAxis("Vertical");
        _animator.SetFloat("LastX", _xAxis);
        _animator.SetFloat("LastZ", _zAxis);

        // Disparo
        if (Input.GetKeyDown(KeyCode.Space))
            _ShootBulletActivate = true;

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_isDashing && _dashCooldownTimer.ExpiredOrNotRunning(Runner))
            TryDash();

        // Campo de fuerza
        if (Input.GetKeyDown(KeyCode.Alpha1) && _canUseForce)
            RPC_ForceActivate();

        UpdateForceField();
    }

    private void TryDash()
    {
        if (_xAxis != 0 || _zAxis != 0)
        {
            _isDashing = true;
            _isInvulnerable = true;
            _dashDurationTimer = TickTimer.CreateFromSeconds(Runner, _dashDuration);
            _dashCooldownTimer = TickTimer.CreateFromSeconds(Runner, _dashCooldown);
            _invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, _invulnerabilityDuration);

            RPC_PlayDashEffects();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDashEffects()
    {
        if (_dashParticles != null) _dashParticles.Play();
        if (_dashTrail != null) _dashTrail.emitting = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StopDashEffects()
    {
        if (_dashTrail != null) _dashTrail.emitting = false;
    }

    private void UpdateDashCooldownUI()
    {
        if (_dashCooldownUI != null)
        {
            float remaining = _dashCooldownTimer.RemainingTime(Runner) ?? 0f;
            _dashCooldownUI.fillAmount = 1 - (remaining / _dashCooldown);
        }
    }

    private void UpdateForceField()
    {
        if (_forceActivate)
        {
            _lifeTimeForceCount += Time.deltaTime;
            if (_lifeTimeForceCount / _lifeTimeForce >= 0.4f)
                _material.SetFloat("_Disolve", _lifeTimeForceCount / _lifeTimeForce);

            if (_lifeTimeForceCount >= _lifeTimeForce * 0.6f)
                RPC_ForceDesactivate();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_isDashing)
            HandleDash();
        else
        {
            MovementPlayer();
            RotationPlayer();
        }

        HandleShooting();
        CheckTimers();
        ClampPosition();
        _borrar = _doubleShotActive;
    }

    private void HandleDash()
    {
        Vector3 dir = (_xAxis != 0 || _zAxis != 0)
            ? transform.TransformDirection(new Vector3(_xAxis, 0, _zAxis).normalized)
            : transform.forward;

        float dashSpeed = _dashDistance / _dashDuration;
        _netRB.Rigidbody.velocity = dir * dashSpeed;
    }

    private void MovementPlayer()
    {
        if (_zAxis == 0)
        {
            _netRB.Rigidbody.velocity = Vector3.zero;
            return;
        }

        Vector3 vel = transform.forward * _zAxis * _speed * Runner.DeltaTime;
        _netRB.Rigidbody.velocity = Vector3.ClampMagnitude(vel, _speed);
    }

    private void RotationPlayer()
    {
        if (_xAxis == 0) return;
        float angle = _xAxis * _speedRotation * Runner.DeltaTime;
        _netRB.Rigidbody.MoveRotation(_netRB.Rigidbody.rotation * Quaternion.Euler(0, angle, 0));
    }

    private void HandleShooting()
    {
        if (_ShootBulletActivate)
        {
            SpawnBullet();
            _ShootBulletActivate = false;
        }
    }

    private void SpawnBullet()
    {
        // Siempre usar Runner.Spawn para que los proyectiles sean NetworkObjects
        if (_doubleShotActive)
        {
            Runner.Spawn(_bulletPrefab, _spawnTransformLeft.position, transform.rotation);
            Runner.Spawn(_bulletPrefab, _spawnTransformRight.position, transform.rotation);
        }
        else
        {
            Runner.Spawn(_bulletPrefab, _spawnTransform.position, transform.rotation);
        }
    }

    private void CheckTimers()
    {
        if (_isDashing && _dashDurationTimer.Expired(Runner))
        {
            _isDashing = false;
            _netRB.Rigidbody.velocity = Vector3.zero;
            RPC_StopDashEffects();
        }

        if (_isInvulnerable && _invulnerabilityTimer.Expired(Runner))
            _isInvulnerable = false;

        if (_doubleShotTimer.Expired(Runner))
            _doubleShotActive = false;
    }

    private void ClampPosition()
    {
        transform.position = GameManager.Instance.AjustPositionToBounds(transform.position);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ForceActivate()
    {
        _material.SetFloat("_Disolve", 0.4f);
        _force.SetActive(true);
        _canUseForce = false;
        _forceActivate = true;
        _lifeTimeForceCount = 0f;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ForceDesactivate()
    {
        _forceActivate = false;
        _force.SetActive(false);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float damage)
    {
        if (!Object.HasStateAuthority || _isInvulnerable || _forceActivate) return;

        _currentHealth -= damage;
        RPC_BarLife(_currentHealth);

        if (_currentHealth <= 0) Death();
    }

    private void Death()
    {
        GameManager.Instance.RPC_Defeat(Runner.LocalPlayer);
        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BarLife(float health)
    {
        _barLife.rectTransform.localScale = new Vector3(health / _maxHealth, 1, 1);
    }

   
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ActivateDoubleShot(float duration)
    {
        _doubleShotActive = true;
        _doubleShotTimer = TickTimer.CreateFromSeconds(Runner, duration);
    }
}