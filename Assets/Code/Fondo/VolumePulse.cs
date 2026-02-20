using UnityEngine;
using UnityEngine.Rendering;

public class VolumePulse : MonoBehaviour
{
    [Header("Referencia")]
    public GameObject volumeObject;

    private Volume volume;

    [Header("Rango de intensidad")]
    [Range(0f, 1f)] public float minWeight = 0.5f;
    [Range(0f, 1f)] public float maxWeight = 1f;

    [Header("Ritmo")]
    [Tooltip("Ticks por segundo (150 BPM ≈ 2.5)")]
    public float ticksPerSecond = 2.5f;

    [Header("Suavizado")]
    public float lerpSpeed = 3f;

    float targetWeight;
    float tickTimer;

    void Start()
    {
        if (volumeObject != null)
            volume = volumeObject.GetComponent<Volume>();

        if (volume == null)
        {
            Debug.LogWarning("No se encontró un Volume en el objeto asignado.");
            enabled = false;
            return;
        }

        targetWeight = volume.weight;
    }

    void Update()
    {
        // --- Temporizador tipo metrónomo ---
        tickTimer += Time.deltaTime;

        if (tickTimer >= 1f / ticksPerSecond)
        {
            tickTimer = 0f;

            // Nuevo objetivo aleatorio
            targetWeight = Random.Range(minWeight, maxWeight);
        }

        // --- Interpolación continua ---
        volume.weight = Mathf.Lerp(
            volume.weight,
            targetWeight,
            Time.deltaTime * lerpSpeed
        );
    }
}