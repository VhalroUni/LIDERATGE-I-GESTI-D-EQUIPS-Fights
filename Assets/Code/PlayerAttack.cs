using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
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
    public float areaAttackDistance = 2.0f;
    public float meleeAttackDistance = 3.0f;

    public bool teleport = false;

    private bool canMeleeAttack = false;
    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

    private int comboStep = 0;
    private bool attackActive = false;

    [Header("Melee Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(1.5f, 1f);
    public Vector2 hitboxOffset = new Vector2(1f, 0f);

    private readonly int[] damage = { 4, 5, 7 };
    private readonly int[] startup = { 5, 6, 8 };
    private readonly int[] active = { 3, 3, 4 };
    private readonly int[] recovery = { 10, 12, 16 };
    private readonly int[] powerGain = { 1, 1, 1 };

    private float totalPower = 0f;
    private const float maxTotalPower = 4f;
    private Image powerFillImage;

    // Nueva variable para almacenar dirección del melee actual
    private Vector3 meleeDirection = Vector3.right;

    void Start()
    {
        if (powerBar != null)
            powerFillImage = powerBar.fillRect.GetComponent<Image>();
    }

    void Update()
    {
        if (target == null)
        {
            canMeleeAttack = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        canMeleeAttack = distance < meleeAttackDistance;
    }

    public void ModifyPower(float amount)
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
    // MELEE (RECUADRO DIRECCIONAL)
    // =========================

    public void MeleeAttack()
    {
        if (isAttacking) return;               // Ya no chequeamos canMeleeAttack
        if (Time.time - lastAttackTime < attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        int currentStep = comboStep;

        // Calculamos la dirección hacia el target, si hay target, si no pegamos hacia la derecha por defecto
        meleeDirection = target != null ?
                         (target.transform.position - transform.position).normalized :
                         Vector3.right;

        StartCoroutine(MeleeAttackRoutine(
            damage[currentStep],
            startup[currentStep],
            active[currentStep],
            recovery[currentStep],
            powerGain[currentStep]
        ));

        comboStep++;
        if (comboStep >= damage.Length)
            comboStep = 0;

        CancelInvoke(nameof(ResetComboStep));
        Invoke(nameof(ResetComboStep), 2f);
    }

    private System.Collections.IEnumerator MeleeAttackRoutine(
        int attackDamage,
        int startupFrames,
        int activeFrames,
        int recoveryFrames,
        int powerGainValue)
    {
        yield return new WaitForSeconds(startupFrames / 60f);

        attackActive = true;

        Vector3 hitboxCenter =
            transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            LifeController life = hit.GetComponent<LifeController>();
            if (life != null)
            {
                life.LoseHealth(attackDamage);
                ModifyPower(powerGainValue / 10f);
            }
        }

        yield return new WaitForSeconds(activeFrames / 60f);

        attackActive = false;

        yield return new WaitForSeconds(recoveryFrames / 60f);

        isAttacking = false;
    }

    private void ResetComboStep()
    {
        comboStep = 0;
    }

    // =========================
    // AREA (CÍRCULO ORIGINAL)
    // =========================

    public void AreaAtack()
    {
        if (isAttacking) return;

        isAttacking = true;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, areaAttackDistance);

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

    // =========================
    // DISTANCE
    // =========================

    public void DistanceAttack()
    {
        if (isAttacking || target == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (totalPower < distancePowerCost) return;

        ModifyPower(-distancePowerCost);

        isAttacking = true;
        lastAttackTime = Time.time;

        GameObject projectileGO =
            Instantiate(ball, transform.position, Quaternion.identity);

        Vector3 direction =
            (target.transform.position - transform.position).normalized;

        var projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.damage = distanceDamage;
            projectile.ownerPlayerIndex = playerIndex;
        }

        Destroy(projectileGO, 3f);

        StartCoroutine(ResetAttack());
    }

    // =========================
    // TELEPORT
    // =========================

    public void Teleport()
    {
        if (target == null) return;
        if (totalPower < teleportPowerCost) return;

        ModifyPower(-teleportPowerCost);

        Vector3 direction =
            (target.transform.position - transform.position).normalized;

        transform.position =
            target.transform.position + direction * 1f;

        teleport = true;
    }

    public void Ultimate()
    {
        Debug.Log("Ultimate");
    }

    public void Block()
    {
        Debug.Log("Block");
    }

    public bool GetTeleportBool()
    {
        return teleport;
    }

    public void SetTeleportBool(bool value)
    {
        teleport = value;
    }

    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    private System.Collections.IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }

    // =========================
    // GIZMOS DIRECCIONALES MELEE
    // =========================

    private void OnDrawGizmos()
    {
        // Área del ataque circular
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, areaAttackDistance);

        // Hitbox melee direccional
        if (attackActive)
        {
            Gizmos.color = Color.red;

            Vector3 hitboxCenter =
                transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

            Gizmos.DrawWireCube(hitboxCenter, hitboxSize);
        }
    }

    // =========================
    // PROYECTILE MANA GAIN
    // =========================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var projectileId =
            collision.gameObject.GetComponent<Projectile>();

        if (collision.CompareTag("Projectile") &&
            projectileId.GetOwnerPlayerID() != playerIndex)
        {
            var enemy = target.GetComponent<PlayerAttack>();

            ModifyPower(distancePowerGain);
            enemy.ModifyPower(distancePowerGain);
        }
    }
}