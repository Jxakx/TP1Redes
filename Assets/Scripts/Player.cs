using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine.UI;

public class Player : NetworkBehaviour, IDamageable
{
    [Header("Player")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _speed;
    [SerializeField] private float _speedRotation;
    NetworkRigidbody3D _netRB;
    [SerializeField] private Animator _animator;


    [SerializeField] private float _xAxis;
    [SerializeField] private float _zAxis;

    [Header ("Bullet")]
    [SerializeField] private bool _ShootBulletActivate;
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] Transform _spawnTransform;

    [Header("UI")]
    [SerializeField] private RawImage _barLife;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;
        _netRB = GetComponent<NetworkRigidbody3D>();
        _currentHealth = _maxHealth;
    }

    void Update()
    {
        if (!HasStateAuthority) return;
        _xAxis = Input.GetAxis("Horizontal");
        _zAxis = Input.GetAxis("Vertical");
        _animator.SetFloat("LastX", _xAxis);
        _animator.SetFloat("LastZ", _zAxis);

        if (Input.GetKeyDown(KeyCode.Space)) _ShootBulletActivate = true;
    }

    public override void FixedUpdateNetwork()
    {
        MovementPlayer();
        RotationPlayer();
        transform.position = GameManager.Intance.AjustPositionToBounds(transform.position);
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

        _currentHealth -= damage;
        BarLife(_currentHealth);

        if (_currentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        Destroy(gameObject);
    }

    private void BarLife(float health)
    {
        _barLife.rectTransform.localScale = new Vector3(health / _maxHealth, 1, 1);
    }
}
