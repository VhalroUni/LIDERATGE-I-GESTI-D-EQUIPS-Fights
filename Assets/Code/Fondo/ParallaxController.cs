using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [Header("Referencia")]
    public Transform cameraTransform;

    [Header("Capas del fondo")]
    public Transform[] layers;

    [Header("Intensidad general")]
    [Range(0f, 1f)]
    public float intensity = 0.5f;

    // Posición anterior de la cámara
    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Movemos cada capa según la intensidad
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null) continue;

            float layerFactor = (i + 1f) / layers.Length;
            float parallaxAmount = layerFactor * intensity;

            Vector3 movement = new Vector3(
                deltaMovement.x * parallaxAmount,
                deltaMovement.y * parallaxAmount,
                0f
            );

            layers[i].position += movement;
        }

        lastCameraPosition = cameraTransform.position;
    }
}