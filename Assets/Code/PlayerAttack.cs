using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject m_Target;
    public GameObject m_Ball;
    public Slider PowerBar;

    [Header("PowerBar Settings")]
    public float TeleportPower = 0.4f;
    public float DistancePower = 0.2f;
    public float PowerRegeneration = 0.1f;
    [Header("Attack Settings")]
    public int m_PlayerIndex;
    public float attackDamage = 10f;
    public float m_AttackDistance = 5.0f;

    public bool teleport = false;
    private bool m_CanAttack = false;
    private bool m_IsAttacking = false;
    private float m_AttackCooldown = 0.5f;
    private float m_LastAttackTime = 0f;

    private float m_PowerNeed = 0.05f;

    void Update()
    {
        if (m_Target == null)
        {
            m_CanAttack = false;
            return;
        }

        float l_DistanceToRival = Vector3.Distance(transform.position, m_Target.transform.position);
        m_CanAttack = l_DistanceToRival < m_AttackDistance;

        if (PowerBar.value < 1)
        {
            PowerBar.value += PowerRegeneration * Time.deltaTime;
        }
            
    }

    public void MeleeAttack()
    {
        if (m_Target == null)
        {
            Debug.LogWarning($"{gameObject.name} intento atacar pero m_Target es null");
            return;
        }

        if (Time.time - m_LastAttackTime < m_AttackCooldown)
            return;

        if (!m_CanAttack)
        {
            Debug.LogWarning($"{gameObject.name} intento atacar pero está fuera de rango (m_AttackDistance={m_AttackDistance})");
            return;
        }

        if(m_IsAttacking)
        {
            return;
        }

        m_IsAttacking = true;
        m_LastAttackTime = Time.time;

        var life = m_Target.GetComponent<LifeController>();

        if (life != null)
        {
            life.LoseHealth(attackDamage);
            PowerBar.value -= m_PowerNeed; 
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} ataca a {m_Target.name} pero el objetivo no tiene LifeController");
        }

        Debug.Log($"{gameObject.name} ataca a {m_Target.name} con {attackDamage} de daño");

        StartCoroutine(ResetAttack());
    }

    public void AreaAtack()
    {
        Debug.Log("Ataque area");
    }

    public void Ultimate()
    {
        Debug.Log("Ultimate");
    }

    public void DistanceAttack()
    {
        Debug.Log("Ataque a distancia");
        if (m_Target == null) return;

        if (Time.time - m_LastAttackTime < m_AttackCooldown)
            return;

        if (m_IsAttacking)
            return;
        if (PowerBar.value < DistancePower)
            return;
        PowerBar.value -= DistancePower;
        m_IsAttacking = true;
        m_LastAttackTime = Time.time;

        GameObject projectileGO = Instantiate(m_Ball, transform.position, Quaternion.identity);

        Vector3 direction = (m_Target.transform.position - transform.position).normalized;

        var projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.damage = attackDamage;
            projectile.ownerPlayerIndex = m_PlayerIndex;
        }

        Destroy(projectile, 3);

        StartCoroutine(ResetAttack());
    }

    public void Teleport()
    {
        Debug.Log("Teletransporte");
        float distanceBehind = 1.0f;
        if (m_Target == null) return;
        if (PowerBar.value < TeleportPower)
            return;
        PowerBar.value -= TeleportPower;
        Vector3 directionToTarget = (m_Target.transform.position - transform.position).normalized;
        Vector3 behindDirection = directionToTarget;
        Vector3 newPosition = m_Target.transform.position + behindDirection * distanceBehind;
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
        m_IsAttacking = false;
    }

    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public bool GetTeleportBool()
    {
        return teleport;
    }

    public void SetTeleportBool(bool value)
    {
        teleport = value;
    }
}
