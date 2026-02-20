using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OldScript : MonoBehaviour
{
    [Header("References")]
    public GameObject target;
    public GameObject ball;
    public Slider powerBar;

    [Header("PowerBar Settings")]
    public float teleportPowerCost = 0.25f;
    public float distancePowerCost = 0.1f;
    public float meleePowerGain = 0.1f;
    public float areaPowerGain = 0.5f;
    public float distancePowerGain = 0.5f;

    [Header("Power Levels Colors")]
    public Color level1Color = Color.white;
    public Color level2Color = Color.yellow;
    public Color level3Color = Color.red;
    public Color level4Color = Color.magenta;

    [Header("Attack Settings")]
    public int playerIndex;
    public float attackDamage = 4.0f;
    public float areaDamage = 19.0f;
    public float distanceDamage = 8.0f;
    public float meleeAttackDistance = 3.0f;

    public bool teleport = false;

    private bool canMeleeAttack = false;
    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

    private int comboStep = 0;
    private bool attackActive = false;

    [Header("Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(1.5f, 1f);
    public Vector2 hitboxOffset = new Vector2(1f, 0f);

    private readonly int[] damage = { 4, 5, 7 };
    private readonly int[] startup = { 5, 6, 8 };
    private readonly int[] active = { 3, 3, 4 };
    private readonly int[] recovery = { 10, 12, 16 };
    private readonly int[] powerGain = { 10, 10, 10 };

    // Power System
    private float totalPower = 0f;
    private const float maxTotalPower = 4f;
    private Image powerFillImage;

    void Start()
    {
        if (powerBar != null)
        {
            powerFillImage = powerBar.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (target == null)
        {
            canMeleeAttack = false;
            return;
        }

        // Aproximación para permitir ataque melee
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        canMeleeAttack = distanceToTarget < meleeAttackDistance;
    }

    // =========================
    // PowerBar
    // =========================
    void ModifyPower(float amount)
    {
        totalPower += amount;
        totalPower = Mathf.Clamp(totalPower, 0f, maxTotalPower);
        UpdatePowerVisual();
    }

    void UpdatePowerVisual()
    {
        if (powerBar == null) return;

        int level = Mathf.FloorToInt(totalPower);
        float localValue = totalPower - level;

        powerBar.value = localValue;

        if (powerFillImage == null) return;

        switch (level)
        {
            case 0: powerFillImage.color = level1Color; break;
            case 1: powerFillImage.color = level2Color; break;
            case 2: powerFillImage.color = level3Color; break;
            default: powerFillImage.color = level4Color; break;
        }
    }

    // =========================
    // Melee Attack con Hitbox
    // =========================
    public void MeleeAttack()
    {
        if (!canMeleeAttack || isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        int currentStep = comboStep;
        int currentDamage = damage[currentStep];
        int currentStartup = startup[currentStep];
        int currentActive = active[currentStep];
        int currentRecovery = recovery[currentStep];
        int currentPowerGain = powerGain[currentStep];

        StartCoroutine(MeleeAttackRoutine(currentDamage, currentStartup, currentActive, currentRecovery, currentPowerGain));

        comboStep++;
        if (comboStep >= damage.Length)
            comboStep = 0;

        // 🔹 reiniciar combo tras inactividad
        CancelInvoke(nameof(ResetComboStep));
        Invoke(nameof(ResetComboStep), 2f);



    }
    private void ResetComboStep()
    {
        comboStep = 0;
    }

    private System.Collections.IEnumerator MeleeAttackRoutine(int attackDamage, int startupFrames, int activeFrames, int recoveryFrames, int powerGainValue)
    {
        yield return new WaitForSeconds(startupFrames / 60f);

        attackActive = true;

        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 hitboxCenter = transform.position + Vector3.Scale(direction, hitboxOffset) + Vector3.up * hitboxOffset.y;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            LifeController life = hit.GetComponent<LifeController>();
            if (life != null)
            {
                life.LoseHealth(attackDamage);
                ModifyPower(powerGainValue / 10f);
                Debug.Log($"{gameObject.name} ataca a {hit.name} con {attackDamage} de daño");
            }
        }

        yield return new WaitForSeconds(activeFrames / 60f);

        attackActive = false;

        yield return new WaitForSeconds(recoveryFrames / 60f);

        isAttacking = false;

    }

    public void AreaAtack()
    {
        if (isAttacking) return;
        isAttacking = true;

        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 hitboxCenter = transform.position + Vector3.Scale(direction, hitboxOffset) + Vector3.up * hitboxOffset.y;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            LifeController life = hit.GetComponent<LifeController>();
            if (life != null)
            {
                life.LoseHealth(areaDamage);
                ModifyPower(areaPowerGain);
            }
        }

        StartCoroutine(ResetAttack());
    }

    public void Ultimate()
    {
        Debug.Log("Ultimate");
    }

    public void DistanceAttack()
    {
        if (isAttacking || target == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (totalPower < distancePowerCost) return;

        ModifyPower(-distancePowerCost);
        isAttacking = true;
        lastAttackTime = Time.time;

        GameObject projectileGO = Instantiate(ball, transform.position, Quaternion.identity);
        Vector3 direction = (target.transform.position - transform.position).normalized;

        var projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.damage = distanceDamage;
            projectile.ownerPlayerIndex = playerIndex;
        }

        Destroy(projectileGO, 3);
        StartCoroutine(ResetAttack());
    }

    public void Teleport()
    {
        if (target == null) return;
        if (totalPower < teleportPowerCost) return;

        ModifyPower(-teleportPowerCost);

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        transform.position = target.transform.position + directionToTarget * 1f;
        teleport = true;
    }

    public void Block()
    {
        Debug.Log("Cubrir");
    }

    private System.Collections.IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }

    public int GetPlayerIndex() => playerIndex;
    public bool GetTeleportBool() => teleport;
    public void SetTeleportBool(bool value) => teleport = value;

    private void OnDrawGizmos()
    {
        if (attackActive)
        {
            Gizmos.color = Color.red;
            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            Vector3 hitboxCenter = transform.position + Vector3.Scale(direction, hitboxOffset) + Vector3.up * hitboxOffset.y;
            Gizmos.DrawWireCube(hitboxCenter, hitboxSize);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var projectileId = collision.gameObject.GetComponent<Projectile>();
        if (collision.CompareTag("Projectile") && projectileId.GetOwnerPlayerID() != playerIndex)
        {
            var targetMana = target.GetComponent<PlayerAttack>();
            ModifyPower(distancePowerGain);
            targetMana.ModifyPower(distancePowerGain);
        }
    }
}