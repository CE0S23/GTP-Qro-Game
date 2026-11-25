using UnityEngine;

namespace GTP
{
    /// <summary>Guarda el último punto de control y reubica al jugador cuando pierde un intento.</summary>
    [RequireComponent(typeof(PlayerMovement))]
    [DisallowMultipleComponent]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        private PlayerMovement movement;
        private PlayerStealth stealth;

        public Vector3 CheckpointPosition { get; private set; }

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            stealth = GetComponent<PlayerStealth>();
            CheckpointPosition = transform.position;
        }

        private void OnEnable() => GameManager.PlayerRespawned += Respawn;
        private void OnDisable() => GameManager.PlayerRespawned -= Respawn;

        public void SetCheckpoint(Vector3 position)
        {
            CheckpointPosition = new Vector3(position.x, transform.position.y, position.z);
        }

        private void Respawn()
        {
            if (stealth != null && stealth.IsHidden)
                stealth.ExitHiding();
            movement.Teleport(CheckpointPosition);
        }
    }
}
