using Fusion;
using UnityEngine;
using Fusion.Addons.Physics;

[RequireComponent(typeof(Collider), typeof(NetworkRigidbody3D))]
public class PowerUp : NetworkBehaviour
{
    [Header("Configuración PowerUp")]
    [SerializeField] private float _powerUpDuration = 5f;
    [SerializeField] private float _lifeTime = 10f;
    [SerializeField] private float _blinkStartTime = 3f;
    [SerializeField] private float _blinkSpeed = 5f;

    [Header("Referencias")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;

    private NetworkRigidbody3D _netRB;
    private Rigidbody _rb;
    private TickTimer _lifeTimer;
    private bool _isBlinking = false;

    public override void Spawned()
    {
        // Obtener referencias
        if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
        if (_collider == null) _collider = GetComponent<Collider>();
        _netRB = GetComponent<NetworkRigidbody3D>();
        _rb = _netRB.Rigidbody;

        // Desactivar gravedad y físicas externas
        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.isKinematic = true;
        }

        if (HasStateAuthority)
        {
            _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (_lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void Update()
    {
        if (!HasStateAuthority || _renderer == null)
            return;

        float remaining = _lifeTimer.RemainingTime(Runner) ?? 0f;
        if (remaining <= _blinkStartTime && !_isBlinking)
            _isBlinking = true;

        if (_isBlinking)
        {
            float val = Mathf.Sin(Time.time * _blinkSpeed);
            _renderer.enabled = val > 0;
            if (_collider != null)
                _collider.enabled = val > 0.5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        if (other.TryGetComponent<Player>(out var player))
        {
            player.RPC_ActivateDoubleShot(_powerUpDuration);
            Runner.Despawn(Object);
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.7f);
        if (_collider is SphereCollider sphere)
            Gizmos.DrawSphere(transform.position, sphere.radius);
        else if (_collider is BoxCollider box)
            Gizmos.DrawCube(transform.position + box.center, box.size);
    }

    private void Reset()
    {
        _powerUpDuration = 5f;
        _lifeTime = 10f;
        _blinkStartTime = 3f;
        _blinkSpeed = 5f;
    }
}