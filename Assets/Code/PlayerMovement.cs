using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject m_Rival;
    public GameObject m_Arrow;

    [Header("Movement Settings")]
    public float m_Acceleration = 15.0f;
    public float m_MaxSpeed = 8.0f;
    public float m_Friction = 3.0f;

    [Header("Orientations")]
    public float m_DelayFlipTime = 0.5f;
    public float velocidadRotacion = 5;

    [Header("Others")]
    public int m_PlayerIndex = 0;

    private PlayerAttack m_Attack;
    private SpriteRenderer m_SpriteRender;
    private Vector2 m_Movement;
    private Vector2 m_Velocity;
    private float currentFlipTime = 0;

    private void Awake()
    {
        if (m_Rival != null)
            m_Attack = m_Rival.GetComponent<PlayerAttack>();
        else
            m_Attack = null;

        m_SpriteRender = GetComponent<SpriteRenderer>();
    }

    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public void SetInputVector(Vector2 _Direction)
    {
        m_Movement = _Direction.magnitude > 0.1f ? _Direction.normalized : Vector2.zero;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (m_Rival == null || m_Attack == null) return;

        bool teleport = m_Attack.GetTeleportBool();

        if (teleport)
            currentFlipTime += Time.deltaTime;

        Movement();
        FlipSprite(teleport);
    }

    private void Movement()
    {
        m_Velocity += m_Movement * m_Acceleration * Time.deltaTime;
        m_Velocity = Vector2.ClampMagnitude(m_Velocity, m_MaxSpeed);
        m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, m_Friction * Time.deltaTime);

        transform.position += (Vector3)(m_Velocity * Time.deltaTime);
    }

    private void TargetOrientation(GameObject target)
    {
        if (target == null || m_Arrow == null) return;

        Vector3 l_Direction = target.transform.position - m_Arrow.transform.position;
        float l_Angle = Mathf.Atan2(l_Direction.y, l_Direction.x) * Mathf.Rad2Deg;
        Quaternion l_Rotation = Quaternion.AngleAxis(l_Angle, Vector3.forward);

        m_Arrow.transform.rotation = Quaternion.Slerp(m_Arrow.transform.rotation, l_Rotation, velocidadRotacion * Time.deltaTime);
    }

    private void FlipSprite(bool teleport)
    {
        if (m_Rival == null) return;

        if (!teleport)
        {
            if (m_Rival.transform.position.x < transform.position.x)
                m_SpriteRender.flipX = true;
            else if (m_Rival.transform.position.x > transform.position.x)
                m_SpriteRender.flipX = false;

            TargetOrientation(m_Rival);
        }
        else if (currentFlipTime >= m_DelayFlipTime)
        {
            if (m_Rival.transform.position.x < transform.position.x)
                m_SpriteRender.flipX = true;
            else if (m_Rival.transform.position.x > transform.position.x)
                m_SpriteRender.flipX = false;

            TargetOrientation(m_Rival);
            currentFlipTime = 0;
            m_Attack?.SetTeleportBool(false);
        }
    }

    public void EndMatch()
    {
        enabled = false;
    }
}