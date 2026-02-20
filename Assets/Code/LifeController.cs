using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class LifeController : MonoBehaviour
{
    public event Action<LifeController> OnDeath;

    [SerializeField] private float MaxHP = 100f;
    [SerializeField] public Slider Life;

    // NUEVO — colores por porcentaje
    public Color color100 = Color.green;
    public Color color50 = Color.yellow;
    public Color color25 = Color.red;

    private Animator animator;
    private float CurrentHP;
    private Image fillImage;

    void Start()
    {
        animator = GetComponent<Animator>();

        CurrentHP = MaxHP;

        // Obtener la imagen del fill del slider
        if (Life != null)
            fillImage = Life.fillRect.GetComponent<Image>();

        if (Life != null)
        {
            Life.minValue = 0;
            Life.maxValue = MaxHP;
            Life.value = CurrentHP;
        }

        UpdateBarColor();
    }

    void Update()
    {
        if (Life != null)
            Life.value = CurrentHP;

        UpdateBarColor();

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
        animator.Play("Zhurong_Hit");
        if (CurrentHP < 0f) CurrentHP = 0f;

        if (Life != null)
            Life.value = CurrentHP;

        UpdateBarColor();

        CheckDeath();
    }

    // NUEVO — cambio de color por tramos
    private void UpdateBarColor()
    {
        if (fillImage == null || MaxHP <= 0f) return;

        float percent = CurrentHP / MaxHP;

        if (percent > 0.4f)
            fillImage.color = color100;
        else if (percent > 0.15f)
            fillImage.color = color50;
        else
            fillImage.color = color25;
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