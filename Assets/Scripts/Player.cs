using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine.UI;
using System;

public class Player : NetworkBehaviour, IDamageable
{
    [Header("Player")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _speed;
    [SerializeField] private float _speedRotation;
    NetworkRigidbody3D _netRB;
    [SerializeField] private Animator _animator;
    public MeshRenderer _myAlas;
    public MeshRenderer _myCamina;


    [SerializeField] private float _xAxis;
    [SerializeField] private float _zAxis;

    [Header ("Bullet")]
    [SerializeField] private bool _ShootBulletActivate;
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] Transform _spawnTransform;

    [Header("Force")]
    [SerializeField] private bool _canUseForce = true;
    [SerializeField] private bool _forceActivate;
    [SerializeField] private float _lifeTimeForce;
    [SerializeField] private float _lifeTimeForceCount;
    [SerializeField] private GameObject _force;
    [SerializeField] private Material _material;

    [Header("UI")]
    [SerializeField] private RawImage _barLife;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;
        _netRB = GetComponent<NetworkRigidbody3D>();
        _currentHealth = _maxHealth;
        GameManager.Instance.AddToList(this);
    }


    void Update()
    {
        if (!HasStateAuthority) return;
        _xAxis = Input.GetAxis("Horizontal");
        _zAxis = Input.GetAxis("Vertical");
        _animator.SetFloat("LastX", _xAxis);
        _animator.SetFloat("LastZ", _zAxis);

        if (Input.GetKeyDown(KeyCode.Space)) _ShootBulletActivate = true;

        if (Input.GetKeyDown(KeyCode.Alpha1) && _canUseForce)
        {
            RPC_ForceActivate();
        }

        if (_forceActivate)
        {
            _lifeTimeForceCount += Time.deltaTime;
            if (_lifeTimeForceCount / _lifeTimeForce >= 0.4f)
            {
                _material.SetFloat("_Disolve", _lifeTimeForceCount / _lifeTimeForce);
            }

            if (_lifeTimeForceCount >= _lifeTimeForce * 0.6f)
            {
                RPC_ForceDesactivate();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ForceActivate()
    {
        _material.SetFloat("_Disolve", 0.4f);
        _force.SetActive(true);
        _canUseForce = false;
        _forceActivate = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ForceDesactivate()
    {
        _forceActivate = false;
        _force.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        MovementPlayer();
        RotationPlayer();
        transform.position = GameManager.Instance.AjustPositionToBounds(transform.position);
        if (_ShootBulletActivate) SpawnBullet();
    }

    private void SpawnBullet()
    {
        Runner.Spawn(_bulletPrefab, _spawnTransform.position, transform.rotation);
        _ShootBulletActivate = false;
    }

    private void MovementPlayer()
    {
        if (_zAxis == 0)
        {
            _netRB.Rigidbody.velocity = Vector3.zero;
            return;
        }
        //transform.position += transform.forward * _zAxis * _speed * Runner.DeltaTime;
        _netRB.Rigidbody.velocity = transform.forward * _zAxis * _speed * Runner.DeltaTime;

        if (Mathf.Abs(_netRB.Rigidbody.velocity.z) > _speed)
        {
            var vel = _netRB.Rigidbody.velocity;
            vel = Vector3.ClampMagnitude(vel, _speed);
            _netRB.Rigidbody.velocity = vel;
        }
    }

    private void RotationPlayer()
    {
        if (_xAxis == 0) return;
        float rotationAmount = _xAxis * _speedRotation * Runner.DeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
        _netRB.Rigidbody.MoveRotation(_netRB.Rigidbody.rotation * deltaRotation);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float damage)
    {
        if (!Object.HasStateAuthority) return;
        if (_forceActivate) return;

        _currentHealth -= damage;
        RPC_BarLife(_currentHealth);

        if (_currentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        GameManager.Instance.RPC_Defeat(Runner.LocalPlayer);
        Destroy(gameObject);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BarLife(float health)
    {
        _barLife.rectTransform.localScale = new Vector3(health / _maxHealth, 1, 1);
    }
}
