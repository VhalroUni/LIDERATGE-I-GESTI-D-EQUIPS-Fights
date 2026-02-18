using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class LifeController : MonoBehaviour
{
    public event Action<LifeController> OnDeath;

    [SerializeField] private float MaxHP = 100f;
    [SerializeField] public Slider Life;

    private float CurrentHP;

    void Start()
    {
        CurrentHP = MaxHP;
        if (Life != null)
        {
            Life.minValue = 0;
            Life.maxValue = MaxHP;
            Life.value = CurrentHP;
        }
    }

    void Update()
    {
        if (Life != null)
            Life.value = CurrentHP;

        CheckDeath();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ApplyDamage(1f);
    }

    public float LoseHealth(float damage)
    {
        ApplyDamage(damage);
        return CurrentHP;
    }

    private void ApplyDamage(float damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0f) CurrentHP = 0f;
        if (Life != null)
            Life.value = CurrentHP;

        CheckDeath();
    }

    private void CheckDeath()
    {
        if (CurrentHP <= 0f)
        {
            Debug.Log($"[LifeController] {name} murió. Lanzando OnDeath.");
            OnDeath?.Invoke(this);
            StartCoroutine(DestroyNextFrame());
        }
    }

    private IEnumerator DestroyNextFrame()
    {
        yield return null;
        if (this != null && gameObject != null)
        {
            Debug.Log($"[LifeController] Destruyendo {name}.");
            Destroy(gameObject);
        }
    }
}