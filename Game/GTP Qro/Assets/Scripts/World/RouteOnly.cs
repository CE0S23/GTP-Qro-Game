using UnityEngine;

namespace GTP
{
    /// <summary>Activa este objeto solo si la ruta guardada coincide (p. ej., acompañantes en el Spot).</summary>
    public sealed class RouteOnly : MonoBehaviour
    {
        [SerializeField] private Route route = Route.Bien;
        [Tooltip("Ruta usada si se da Play directo en la escena sin haber decidido.")]
        [SerializeField] private Route fallbackRoute = Route.Bien;

        public void Configure(Route requiredRoute) => route = requiredRoute;

        private void Awake()
        {
            Route current = GameManager.Instance != null ? GameManager.Instance.Session.Route : Route.None;
            if (current == Route.None) current = fallbackRoute;
            gameObject.SetActive(current == route);
        }
    }
}
