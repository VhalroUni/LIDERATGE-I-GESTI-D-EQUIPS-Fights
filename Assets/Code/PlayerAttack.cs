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
    //public float powerRegeneration = 0.1f;
    public float meleePowerGain = 0.1f;
    public float areaPowerGain = 0.5f;
    public float distancePowerGain = 0.5f;

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

    //private float powerNeed = 0.05f;

    void Update()
    {
        if (target == null)
        {
            canMeleeAttack = false;
            return;
        }

        distanceToRival = Vector2.Distance(gameObject.transform.position, target.transform.position);

        float l_DistanceToRival = Vector3.Distance(transform.position, target.transform.position);
        canMeleeAttack = l_DistanceToRival < meleeAttackDistance;

        /*if (powerBar.value < 1)
        {
            powerBar.value += powerRegeneration * Time.deltaTime;
        }*/
        //Debug.Log(powerBar.value);

    }

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
            Debug.LogWarning($"{gameObject.name} intento atacar pero está fuera de rango (m_AttackDistance={meleeAttackDistance})");
            return;
        }

        if(isAttacking)
        {
            return;
        }

        isAttacking = true;
        lastAttackTime = Time.time;

        var life = target.GetComponent<LifeController>();

        if (life != null)
        {
            life.LoseHealth(attackDamage);
            powerBar.value += meleePowerGain;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} ataca a {target.name} pero el objetivo no tiene LifeController");
        }

        Debug.Log($"{gameObject.name} ataca a {target.name} con {attackDamage} de daño");

        StartCoroutine(ResetAttack());
    }

    public void AreaAtack()
    {
        Debug.Log("Ataque area");
        if (target == null) return;

        if (isAttacking) return;

        isAttacking = true;

        var life = target.GetComponent<LifeController>();

        if (life != null && distanceToRival < areaAttackDistance)
        {
            life.LoseHealth(areaDamage);
            powerBar.value += areaPowerGain;
        }

        StartCoroutine(ResetAttack());
    }

    public void Ultimate()
    {
        Debug.Log("Ultimate");
    }

    public void DistanceAttack()
    {
        Debug.Log("Ataque a distancia");
        if (target == null) return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        if (isAttacking)
            return;
        if (powerBar.value < distancePowerCost)
            return;
        powerBar.value -= distancePowerCost;
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

        Destroy(projectile, 3);

        StartCoroutine(ResetAttack());
    }

    public void Teleport()
    {
        Debug.Log("Teletransporte");
        float distanceBehind = 1.0f;
        if (target == null) return;
        if (powerBar.value < teleportPowerCost)
            return;
        powerBar.value -= teleportPowerCost;
        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        Vector3 behindDirection = directionToTarget;
        Vector3 newPosition = target.transform.position + behindDirection * distanceBehind;
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
        Gizmos.DrawWireSphere(gameObject.transform.position, areaAttackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gameObject.transform.position, meleeAttackDistance);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var projectileId = collision.gameObject.GetComponent<Projectile>();
        if(collision.CompareTag("Projectile") && projectileId.GetOwnerPlayerID() != playerIndex)
        {
            var targetMana = target.GetComponent<PlayerAttack>();

            powerBar.value += distancePowerGain;
            targetMana.powerBar.value += distancePowerGain;
        }
    }
}
