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
    public float areaAttackDistance = 6.0f;
    public float meleeAttackDistance = 3.0f;

    public bool teleport = false;

    private bool canMeleeAttack = false;
    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;
    private float distanceToRival;

    // =========================
    // NUEVA LÓGICA DE PODER
    // =========================

    private float totalPower = 0f; // 0 → 400
    private const float maxTotalPower = 4f; // equivalente a 400%
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

        distanceToRival = Vector2.Distance(transform.position, target.transform.position);

        float l_DistanceToRival = Vector3.Distance(transform.position, target.transform.position);
        canMeleeAttack = l_DistanceToRival < meleeAttackDistance;
    }

    // =========================
    // CONTROL CENTRALIZADO POWER BAR
    // =========================
    void ModifyPower(float amount)
    {
        totalPower += amount;

        // Clamp 0 → 400%
        totalPower = Mathf.Clamp(totalPower, 0f, maxTotalPower);

        UpdatePowerVisual();
    }

    void UpdatePowerVisual()
    {
        if (powerBar == null) return;

        // Nivel actual (0,1,2,3)
        int level = Mathf.FloorToInt(totalPower);

        // Parte decimal dentro del nivel (0→1)
        float localValue = totalPower - level;

        powerBar.value = localValue;

        if (powerFillImage == null) return;

        switch (level)
        {
            case 0:
                powerFillImage.color = level1Color;
                break;
            case 1:
                powerFillImage.color = level2Color;
                break;
            case 2:
                powerFillImage.color = level3Color;
                break;
            default:
                powerFillImage.color = level4Color;
                break;
        }
    }

    // =========================
    // ATAQUES (solo cambiado el manejo de power)
    // =========================

    public void MeleeAttack()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name} intento atacar pero m_Target es null");
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        if (!canMeleeAttack)
        {
            Debug.LogWarning($"{gameObject.name} intento atacar pero está fuera de rango");
            return;
        }

        if (isAttacking)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;

        var life = target.GetComponent<LifeController>();

        if (life != null)
        {
            life.LoseHealth(attackDamage);
            ModifyPower(meleePowerGain);
        }

        StartCoroutine(ResetAttack());
    }

    public void AreaAtack()
    {
        if (target == null) return;
        if (isAttacking) return;

        isAttacking = true;

        var life = target.GetComponent<LifeController>();

        if (life != null && distanceToRival < areaAttackDistance)
        {
            life.LoseHealth(areaDamage);
            ModifyPower(areaPowerGain);
        }

        StartCoroutine(ResetAttack());
    }

    public void Ultimate()
    {
        Debug.Log("Ultimate");
    }

    public void DistanceAttack()
    {
        if (target == null) return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        if (isAttacking)
            return;

        if (totalPower < distancePowerCost)
            return;

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
        float distanceBehind = 1.0f;
        if (target == null) return;

        if (totalPower < teleportPowerCost)
            return;

        ModifyPower(-teleportPowerCost);

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        Vector3 newPosition = target.transform.position + directionToTarget * distanceBehind;

        transform.position = newPosition;
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

    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    public bool GetTeleportBool()
    {
        return teleport;
    }

    public void SetTeleportBool(bool value)
    {
        teleport = value;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaAttackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meleeAttackDistance);
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