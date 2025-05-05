using Fusion;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    [Header("Configuración PowerUp")]
    [SerializeField] private float _powerUpDuration = 5f;    // Duración del efecto en el jugador
    [SerializeField] private float _lifeTime = 10f;         // Tiempo total de vida del power-up
    [SerializeField] private float _blinkStartTime = 3f;    // Cuándo empieza a parpadear antes de desaparecer
    [SerializeField] private float _blinkSpeed = 5f;        // Velocidad del parpadeo

    [Header("Referencias")]
    [SerializeField] private Renderer _renderer;            // Componente de renderizado
    [SerializeField] private Collider _collider;           // Collider para desactivar durante parpadeo

    private TickTimer _lifeTimer;
    private bool _isBlinking = false;

    public override void Spawned()
    {
        // Obtener referencias si no están asignadas
        if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
        if (_collider == null) _collider = GetComponent<Collider>();

        if (HasStateAuthority)
        {
            _lifeTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        // Auto-destrucción si no es recolectado a tiempo
        if (_lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void Update()
    {
        // Sistema de parpadeo visual (solo en clientes)
        if (_renderer != null && HasStateAuthority)
        {
            float remainingTime = _lifeTimer.RemainingTime(Runner) ?? 0f;

            // Activar parpadeo cuando queda poco tiempo
            if (remainingTime <= _blinkStartTime && !_isBlinking)
            {
                _isBlinking = true;
            }

            // Efecto de parpadeo
            if (_isBlinking)
            {
                float blinkValue = Mathf.Sin(Time.time * _blinkSpeed);
                _renderer.enabled = blinkValue > 0;

                // Opcional: Desactivar collider durante parpadeo para evitar últimos-momentos
                if (_collider != null)
                    _collider.enabled = blinkValue > 0.5f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            // Aplicar efecto al jugador
            player.RPC_ActivateDoubleShot(_powerUpDuration);

            // Solo la autoridad despawnea el objeto
            if (HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.7f);

        // Mostrar área de influencia
        if (_collider != null)
        {
            if (_collider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position, sphere.radius);
            }
            else if (_collider is BoxCollider box)
            {
                Gizmos.DrawCube(transform.position + box.center, box.size);
            }
        }
    }

    // Reset para el Inspector
    private void Reset()
    {
        // Valores por defecto
        _powerUpDuration = 5f;
        _lifeTime = 10f;
        _blinkStartTime = 3f;
        _blinkSpeed = 5f;
    }
}