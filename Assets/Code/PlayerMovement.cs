using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject m_Rival;

    [Header ("Movement Settings")]
    public float m_Acceleration = 15.0f;
    public float m_MaxSpeed = 8.0f;
    public float m_Friction = 3.0f;
    public int m_PlayerIndex = 0;

    private SpriteRenderer m_SpriteRender;
    private Vector2 m_Movement;
    private Vector2 m_Velocity;

    private void Awake()
    {
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
        Movement();
        FlipSprite();
    }

    private void Movement()
    {
        m_Velocity += m_Movement * m_Acceleration * Time.deltaTime;
        m_Velocity = Vector2.ClampMagnitude(m_Velocity, m_MaxSpeed);
        m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, m_Friction * Time.deltaTime);

        transform.position += (Vector3)(m_Velocity * Time.deltaTime);
    }

    private void FlipSprite()
    {
        if (m_Rival.transform.position.x < transform.position.x)
            m_SpriteRender.flipX = true;
        else if (m_Rival.transform.position.x > transform.position.x)
            m_SpriteRender.flipX = false;
    }
}
