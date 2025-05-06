using Fusion;
using Fusion.Addons.Physics;
using System;
using UnityEngine;

public class Asteroid : NetworkBehaviour, IDamageable
{
    [Header("Asteroid")]
    [SerializeField] private float _force;
    [SerializeField] private float _speedRotationX;
    [SerializeField] private float _speedRotationY;
    [SerializeField] private float _speedRotationZ;
    [SerializeField] public float _damage;
    [SerializeField] private float _health;
    [SerializeField] private Vector3 _direction;

    NetworkRigidbody3D _netRB;

    public override void Spawned()
    {
        _netRB = GetComponent<NetworkRigidbody3D>();
        _direction = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
        _force = UnityEngine.Random.Range(0.1f, 15);
        _speedRotationX = UnityEngine.Random.Range(20, 100);
        _speedRotationY = UnityEngine.Random.Range(20, 100);
        _speedRotationZ = UnityEngine.Random.Range(20, 100);
        _netRB.Rigidbody.AddForce(_direction.normalized * _force, ForceMode.VelocityChange);
    }

    public override void FixedUpdateNetwork()
    {
        RotationAsteroid();
        transform.position = GameManager.Instance.AjustPositionToBounds(transform.position);
    }

    private void RotationAsteroid()
    {
        float rotationAmountX = _speedRotationX * Runner.DeltaTime;
        float rotationAmountY = _speedRotationY * Runner.DeltaTime;
        float rotationAmountZ = _speedRotationZ * Runner.DeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(rotationAmountX, rotationAmountY, rotationAmountZ);
        _netRB.Rigidbody.MoveRotation(_netRB.Rigidbody.rotation * deltaRotation);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (!HasStateAuthority) return;

    //    if (collision.gameObject.TryGetComponent(out Player enemy))
    //    {
    //        enemy.RPC_TakeDamage(_damage);
    //    }
    //}

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float damage)
    {
        if (!Object.HasStateAuthority) return;

        _health -= damage;

        if (_health <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        Destroy(gameObject);
    }
}
