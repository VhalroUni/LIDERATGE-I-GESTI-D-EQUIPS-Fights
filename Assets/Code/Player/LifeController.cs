using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class LifeController : MonoBehaviour
{
    public event Action<LifeController> OnDeath;

    [SerializeField] private float MaxHP = 100f;
    [SerializeField] public Slider Life;
    [SerializeField] public bool IsBlocking = false;

    // NUEVO — colores por porcentaje
    public Color color100 = Color.green;
    public Color color50 = Color.yellow;
    public Color color25 = Color.red;

    // NUEVO — color al recibir impacto
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine hitCoroutine;
    private readonly Color hitColor = new Color(1f, 0.85f, 0.85f); // #FFA0A0

    private Animator animator;
    private float CurrentHP;
    private Image fillImage;

    // NUEVO — flag para indicar si está en hit stun
    private bool isInHitStun = false;
    public bool IsInHitStun => isInHitStun;

    void Start()
    {
        animator = GetComponent<Animator>();

        // NUEVO
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        CurrentHP = MaxHP;

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
        ApplyDamage(1f, collision.transform.position, false);
    }

    public float LoseHealth(float damage, Vector3 attackerPosition = default, bool ignoreBlock = false)
    {
        ApplyDamage(damage, attackerPosition, ignoreBlock);
        return CurrentHP;
    }

    private void ApplyDamage(float damage, Vector3 attackerPosition = default, bool ignoreBlock = false)
    {
        bool canBlock = IsBlocking && !ignoreBlock;

        if (canBlock && attackerPosition != default && spriteRenderer != null)
        {
            Vector2 attackDirection = (attackerPosition - transform.position).normalized;

            Vector2 facingDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            float dotProduct = Vector2.Dot(facingDirection, attackDirection);

            if (dotProduct < 0)
            {
                canBlock = false;
                Debug.Log($"[LifeController] ¡Ataque por la espalda! El bloqueo es inefectivo. flipX={spriteRenderer.flipX}, dotProduct={dotProduct}");
            }
            else
            {
                Debug.Log($"[LifeController] Bloqueo exitoso. flipX={spriteRenderer.flipX}, dotProduct={dotProduct}");
            }
        }

        if (ignoreBlock && IsBlocking)
        {
            Debug.Log($"[LifeController] ¡Ataque que ignora bloqueo! Daño recibido a pesar de bloquear.");
        }

        if (canBlock)
        {
            damage = 0f;
            return;
        }

        CurrentHP -= damage;

        CancelAllAnimations();

        if (spriteRenderer != null)
        {
            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);

            hitCoroutine = StartCoroutine(HitFlash());
        }

        if (CurrentHP < 0f) CurrentHP = 0f;

        if (Life != null)
            Life.value = CurrentHP;

        UpdateBarColor();

        CheckDeath();
    }

    // NUEVO — Cancelar todas las animaciones activas
    private void CancelAllAnimations()
    {
        isInHitStun = true;

        if (IsBlocking)
        {
            IsBlocking = false;
            animator.SetBool("IsBlocking", false);
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.CancelAllAttacks();
        }

        animator.SetBool("IsAttacking", false);
        animator.SetFloat("AttackId", 0);
    }

    private IEnumerator HitFlash()
    {
        animator.Play("Zhurong_Hit", -1, 0f);

        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(0.2f);

        isInHitStun = false;
    }

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