using UnityEngine;

namespace GTP
{
    /// <summary>Batería portátil: recarga el celular y suma puntos. Se recoge por colisión (trigger).</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class BatteryPickup : MonoBehaviour
    {
        [SerializeField] private float spinSpeed = 90f;

        private string Id => gameObject.scene.name + "/" + name;

        private void Start()
        {
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Session.IsCollected(Id))
                gameObject.SetActive(false);
        }

        private void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerStealth>() == null) return;
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Session.TryCollect(Id))
            {
                gm.Session.RegisterPickup();
                gm.Session.AddBattery(gm.Config.batteryPickupAmount);
                gm.Session.AddScore(gm.Config.pickupPoints);
                GameUI.Instance?.Hud.ShowToast("+" + gm.Config.batteryPickupAmount.ToString("0") + "% batería");
            }
            gameObject.SetActive(false);
        }
    }
}
