using UnityEngine;

namespace GTP
{
    /// <summary>Mantiene un texto 3D orientado a la cámara aunque su padre gire (nombres, alerta "!").</summary>
    public sealed class Billboard : MonoBehaviour
    {
        private Transform cam;

        private void LateUpdate()
        {
            if (cam == null)
            {
                Camera main = Camera.main;
                if (main == null) return;
                cam = main.transform;
            }
            transform.rotation = cam.rotation;
        }
    }
}
