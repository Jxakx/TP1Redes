using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
    [SerializeField] private float _speed;
    private float _xAxis;
    private float _zAxis;
    [SerializeField] private Vector3 _dir;
    void Update()
    {
        if (!HasStateAuthority) return;
        _xAxis = Input.GetAxis("Horizontal");
        _zAxis = Input.GetAxis("Vertical");
        _dir = new Vector3(_xAxis, 0, _zAxis);
    }
     
    public override void FixedUpdateNetwork()
    {
        transform.position += transform.right * _xAxis * _speed * Runner.DeltaTime;
        transform.position += transform.forward * _zAxis * _speed * Runner.DeltaTime;
    }
}
