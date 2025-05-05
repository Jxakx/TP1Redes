using Fusion;
using UnityEngine;

public class PowerUpSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private NetworkPrefabRef _powerUpPrefab;
    [SerializeField] private float _minSpawnInterval = 5f;
    [SerializeField] private float _maxSpawnInterval = 15f;
    [SerializeField] private Vector3 _areaSize = new Vector3(10, 0, 10);
    [SerializeField] private bool _debugSpawner = true;

    [Header("Gizmos")]
    [SerializeField] private Color _gizmoColor = Color.green;
    [SerializeField] private bool _drawGizmos = true;

    private TickTimer _spawnTimer;
    private float _nextSpawnInterval;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        SetNextSpawnTime();
        _spawnTimer = TickTimer.CreateFromSeconds(Runner, _nextSpawnInterval);

        if (_debugSpawner)
        {
            Debug.Log($"[PowerUpSpawner] Inicializado. Primer spawn en {_nextSpawnInterval} segundos.", this);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !_spawnTimer.Expired(Runner)) return;

        SpawnPowerUp();
        SetNextSpawnTime();
        _spawnTimer = TickTimer.CreateFromSeconds(Runner, _nextSpawnInterval);

        if (_debugSpawner)
        {
            Debug.Log($"[PowerUpSpawner] Nuevo power-up spawnedo. Próximo en {_nextSpawnInterval} segundos.", this);
        }
    }

    private void SetNextSpawnTime()
    {
        _nextSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
    }

    private void SpawnPowerUp()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-_areaSize.x / 2, _areaSize.x / 2),
            0,
            Random.Range(-_areaSize.z / 2, _areaSize.z / 2)
        );

        var powerUp = Runner.Spawn(_powerUpPrefab, randomPosition, Quaternion.identity);

        if (_debugSpawner)
        {
            Debug.Log($"[PowerUpSpawner] PowerUp creado en posición: {randomPosition}", powerUp);
        }
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = _gizmoColor;
        Gizmos.DrawWireCube(transform.position, _areaSize);
        Gizmos.DrawSphere(transform.position, 0.5f);
    }

    // Método para spawn manual (debug)
    public void DebugSpawnPowerUp()
    {
        if (!HasStateAuthority) return;

        SpawnPowerUp();
        Debug.Log("[PowerUpSpawner] PowerUp generado manualmente");
    }
}