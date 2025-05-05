using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private List<Transform> _zone;

    private bool _initialized;
    private int _index;

    //Este método se ejecuta cada vez que un nuevo usuario entra
    public void PlayerJoined(PlayerRef player)
    {
        var playerCount = Runner.SessionInfo.PlayerCount;

        if (_initialized && playerCount >= 2)
        {
            SpawnPlayers(_index);
            return;
        }

        //Pregunta si el cliente que entro es el mismo que esta en esta computadora
        if (player == Runner.LocalPlayer)
        {
            if (playerCount < 2)
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _index = playerCount - 1;
                }
            }
            else
            {
                SpawnPlayers(playerCount - 1);
            }
        }
    }

    private void SpawnPlayers(int index)
    {
        _initialized = false;
        var spawnPoint = _zone[index];
        //Se debe spawnear el player de cada jugador en la red
        NetworkObject myPlayer = Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation);

        if (GameManager.Instance.index >= 1)
        {
            myPlayer.GetComponent<Player>()._myAlas.material = GameManager.Instance._alas;
            myPlayer.GetComponent<Player>()._myCamina.material = GameManager.Instance._cabina;
        }
        GameManager.Instance.index += 1;
    }
}
