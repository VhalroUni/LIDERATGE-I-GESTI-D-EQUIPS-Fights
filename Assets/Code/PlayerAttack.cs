using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject m_Target;
    public GameObject m_Arrow;

    [Header("Attack Settings")]
    public int m_PlayerIndex;
    public float velocidadRotacion = 5f;
    public float attackDamage = 10f;
    public float m_AttackDistance = 5.0f;

    private bool m_CanAttack = false;
    private bool m_IsAttacking = false;
    private float m_AttackCooldown = 0.5f;
    private float m_LastAttackTime = 0f;

    void Update()
    {
        if (m_Target == null)
        {
            m_CanAttack = false;
            return;
        }

        TargetOrientation(m_Target);

        float l_DistanceToRival = Vector3.Distance(transform.position, m_Target.transform.position);
        m_CanAttack = l_DistanceToRival < m_AttackDistance;

    }

    private void TargetOrientation(GameObject _Target)
    {
        if (_Target == null) return;

        Vector3 l_Direction = _Target.transform.position - m_Arrow.transform.position;
        float l_Angle = Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg;
        Quaternion l_Rotation = Quaternion.AngleAxis(l_Angle, Vector3.forward);

        m_Arrow.transform.rotation = Quaternion.Slerp(m_Arrow.transform.rotation, l_Rotation, velocidadRotacion * Time.deltaTime);
    }

    public void Attack()
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
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} ataca a {m_Target.name} pero el objetivo no tiene LifeController");
        }

        Debug.Log($"{gameObject.name} ataca a {m_Target.name} con {attackDamage} de daño");

        StartCoroutine(ResetAttack());
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
    public void SetTarget(GameObject target)
    {
        m_Target = target;
    }
}
