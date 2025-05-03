using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [Header("Bullet")]
    [SerializeField] private float _force;
    [SerializeField] private float _speedRotation;
    [SerializeField] private float _lifeTime;
    [SerializeField] private TickTimer _lifeTimeCount;
    [SerializeField] private float _damage;

    NetworkRigidbody3D _netRB;

    public override void Spawned()
    {
        _netRB = GetComponent<NetworkRigidbody3D>();
        _netRB.Rigidbody.AddForce(transform.forward * _force, ForceMode.VelocityChange);

        _lifeTimeCount = TickTimer.CreateFromSeconds(Runner, _lifeTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.RPC_TakeDamage(_damage);
        }

        Runner.Despawn(Object);
    }

    public override void FixedUpdateNetwork()
    {
        if (!_lifeTimeCount.Expired(Runner)) return;
        Runner.Despawn(Object);
    }
}
