using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerControllerType { WASD, ArrowKeys, NONE }
    public PlayerControllerType m_ControllerType = PlayerControllerType.NONE;

    public float m_Acceleration = 15.0f;
    public float m_MaxSpeed = 8.0f;
    public float m_Friction = 3.0f;

    private KeyCode m_PlayerUp;
    private KeyCode m_PlayerDown;
    private KeyCode m_PlayerLeft;
    private KeyCode m_PlayerRight;
    private Vector2 m_Velocity;

    private void Awake()
    {
        switch (m_ControllerType)
        {
            case PlayerControllerType.WASD:
                m_PlayerUp = KeyCode.W;
                m_PlayerDown = KeyCode.S;
                m_PlayerLeft = KeyCode.A;
                m_PlayerRight = KeyCode.D;
                break;

            case PlayerControllerType.ArrowKeys:
                m_PlayerUp = KeyCode.UpArrow;
                m_PlayerDown = KeyCode.DownArrow;
                m_PlayerLeft = KeyCode.LeftArrow;
                m_PlayerRight = KeyCode.RightArrow;
                break;
        }
    }

    void Update()
    {
        Vector2 l_Input = Vector2.zero;

        if (Input.GetKey(m_PlayerUp)) l_Input.y += 1;
        if (Input.GetKey(m_PlayerDown)) l_Input.y -= 1;
        if (Input.GetKey(m_PlayerLeft)) l_Input.x -= 1;
        if (Input.GetKey(m_PlayerRight)) l_Input.x += 1;

        l_Input = l_Input.normalized;

        m_Velocity += l_Input * m_Acceleration * Time.deltaTime;
        m_Velocity = Vector2.ClampMagnitude(m_Velocity, m_MaxSpeed);
        m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, m_Friction * Time.deltaTime);

        transform.position += (Vector3)(m_Velocity * Time.deltaTime);
    }
}
