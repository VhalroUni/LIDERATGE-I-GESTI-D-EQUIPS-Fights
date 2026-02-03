using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Collider2D m_AttackCollider;
    public GameObject m_Target;

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
        LookAtTarget(m_Target);

        float l_DistanceToRival = Vector3.Distance(transform.position, m_Target.transform.position);
        if (l_DistanceToRival < m_AttackDistance)
            m_CanAttack = true;
        else
            m_CanAttack = false;

        Debug.Log(l_DistanceToRival < m_AttackDistance);
    }

    private void LookAtTarget(GameObject _Target)
    {
        Vector3 l_Direction = _Target.transform.position - transform.position;
        float l_Angle = Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg;
        Quaternion l_Rotation = Quaternion.AngleAxis(l_Angle, Vector3.forward);

        //MIRAR AL ENEMIGO
        //transform.rotation = Quaternion.Slerp(transform.rotation, l_Rotation, velocidadRotacion * Time.deltaTime);
    }

    public void Attack()
    {
        if (Time.time - m_LastAttackTime < m_AttackCooldown)
            return;

        if (m_CanAttack && m_Target != null && !m_IsAttacking)
        {
            m_IsAttacking = true;
            m_LastAttackTime = Time.time;

            Debug.Log($"{gameObject.name} ataca a {m_Target.name} con {attackDamage} de daño");

            StartCoroutine(ResetAttack());
        }
        else if (!m_CanAttack)
        {
            Debug.Log("No hay objetivo para atacar");
        }
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
}
