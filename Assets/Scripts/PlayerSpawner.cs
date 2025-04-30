using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
   
    //Este método se ejecuta cada vez que un nuevo usuario entra
    public void PlayerJoined(PlayerRef player)
    {
        //Pregunta si el cliente que entro es el mismo que esta en esta computadora
        if (player == Runner.LocalPlayer)
        {
            //Se debe spawnear el player de cada jugador en la red
            Runner.Spawn(_playerPrefab);
        }
    }
}
