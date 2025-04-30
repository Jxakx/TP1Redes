using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    public void PlayerJoined(PlayerRef player)
    {
        //Pregunta si el cliente que entro es el mismo que esta en esta computadora
        if (player == Runner.LocalPlayer)
        {
            //Crea el player de cada jugador en la red
            Runner.Spawn(_playerPrefab);
        }
    }
}
