using UnityEngine;
using System.Collections;

public class SpawnTeleportParticles : MonoBehaviour
{
    public GameObject particlesPrefab;
    public Transform player;

    public int framesAntesDeSpawn = 10; // Espera antes de aparecer
    public int framesActivos = 10;      // Cuánto duran las partículas

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (particlesPrefab != null && player != null)
            {
                // ✅ Guardar posición EXACTA en el momento del input
                Vector3 posicionGuardada = player.position;

                // Nunca volvemos a mirar la posición del player
                StartCoroutine(SpawnTrasFrames(posicionGuardada));
            }
        }
    }

    IEnumerator SpawnTrasFrames(Vector3 posicionFija)
    {
        // Esperar N frames
        for (int i = 0; i < framesAntesDeSpawn; i++)
            yield return null;

        // ✅ Spawn usando SOLO la posición guardada
        GameObject particles = Instantiate(
            particlesPrefab,
            posicionFija,
            Quaternion.identity
        );

        // Destruir tras N frames
        StartCoroutine(DestruirTrasFrames(particles));
    }

    IEnumerator DestruirTrasFrames(GameObject obj)
    {
        for (int i = 0; i < framesActivos; i++)
            yield return null;

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Destroy(obj);
    }
}