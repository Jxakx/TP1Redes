using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float _boundsWith, _boundsHeight;
    public Material _alas;
    public Material _cabina;

    [SerializeField] private GameObject _ganar;
    [SerializeField] private GameObject _perder;

    [SerializeField] private List<PlayerRef> _clients;

    [SerializeField] public int index = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _clients = new List<PlayerRef>();
        }
    }

    public void AddToList(Player player)
    {
        var playerRef = player.Object.StateAuthority;
        if (_clients.Contains(playerRef)) return;

        _clients.Add(playerRef);
    }

    public void RemoveFromList(PlayerRef client)
    {
        _clients.Remove(client);
    }

    [Rpc]
    public void RPC_Defeat(PlayerRef client)
    {
        if (client == Runner.LocalPlayer)
        {
            ShowLose();
        }

        _clients.Remove(client);

        if (_clients.Count == 1 && HasStateAuthority)
        {
            RPC_Win(_clients[0]);
        }
    }

    [Rpc]
    void RPC_Win([RpcTarget]PlayerRef client)
    {
        ShowWin();
    }

    [Rpc]
    void RPC_Lose(PlayerRef client)
    {
        ShowLose();
    }

    private void ShowWin()
    {
        _ganar.SetActive(true);
    }

    private void ShowLose()
    {
        _perder.SetActive(true);
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
