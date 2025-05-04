using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Intance;

    [SerializeField] private float _boundsWith, _boundsHeight;

    private void Awake()
    {
        if (Intance == null)
        {
            Intance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public Vector3 AjustPositionToBounds(Vector3 position)
    {
        float with = _boundsWith / 2;
        float height = _boundsHeight / 2;

        if (position.x > with) position.x = -with;
        if (position.x < -with) position.x = with;

        if (position.z > height) position.z = -height;
        if (position.z < -height) position.z = height;

        position.y = 0;

        return position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(_boundsWith, 0, _boundsHeight));
    }
}
