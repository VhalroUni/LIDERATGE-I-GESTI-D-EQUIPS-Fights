using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject m_Rival;
    public GameObject m_Arrow;

    [Header("Movement Settings")]
    public float m_Acceleration = 15.0f;
    public float m_MaxSpeed = 8.0f;
    public float m_Friction = 3.0f;

    [Header("Dash Settings")]
    public float m_DashForce = 15f;
    public float m_DashCooldown = 1f;

    [Header("Dash UI")]
    public Slider m_DashBarLeft;
    public Slider m_DashBarRight;

    [Header("Dash Audio")]
    public AudioClip m_DashSound;
    public AudioClip m_DashRechargeSound;

    [Header("Orientations")]
    public float m_DelayFlipTime = 0.5f;
    public float velocidadRotacion = 5;

    [Header("Others")]
    public int m_PlayerIndex = 0;

    private PlayerAttack m_Attack;
    private SpriteRenderer m_SpriteRender;
    private AudioSource m_AudioSource;

    private Vector2 m_Movement;
    private Vector2 m_Velocity;
    private float currentFlipTime = 0;

    // DASH CHARGES
    private float chargeLeft = 1f;
    private float chargeRight = 1f;

    // detectar cuando termina de cargar
    private bool leftWasFull = true;
    private bool rightWasFull = true;

    private void Awake()
    {
        if (m_Rival != null)
            m_Attack = m_Rival.GetComponent<PlayerAttack>();
        else
            m_Attack = null;

        m_SpriteRender = GetComponent<SpriteRenderer>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        UpdateDashUI();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (m_Rival == null || m_Attack == null) return;

        bool teleport = m_Attack.GetTeleportBool();

        if (teleport)
            currentFlipTime += Time.deltaTime;

        HandleDash();
        UpdateDashRecharge();

        Movement();
        FlipSprite(teleport);
    }

    // ================= DASH =================
    private void HandleDash()
    {
        if (!Input.GetKeyDown(KeyCode.LeftShift))
            return;

        if (m_Movement == Vector2.zero)
            return;

        if (chargeLeft < 1f && chargeRight < 1f)
            return;

        if (chargeRight >= 1f)
            chargeRight = 0f;
        else if (chargeLeft >= 1f)
            chargeLeft = 0f;

        m_Velocity += m_Movement * m_DashForce;

        // SONIDO DASH
        if (m_DashSound != null && m_AudioSource != null)
            m_AudioSource.PlayOneShot(m_DashSound);

        UpdateDashUI();
    }

    private void UpdateDashRecharge()
    {
        if (chargeLeft >= 1f && chargeRight >= 1f)
            return;

        float rechargeAmount = Time.deltaTime / m_DashCooldown;

        if (chargeLeft < 1f)
        {
            chargeLeft += rechargeAmount;
            chargeLeft = Mathf.Clamp01(chargeLeft);
        }
        else if (chargeRight < 1f)
        {
            chargeRight += rechargeAmount;
            chargeRight = Mathf.Clamp01(chargeRight);
        }

        // Detectar final de carga (SONIDO)
        if (!leftWasFull && chargeLeft >= 1f)
            PlayRechargeSound();

        if (!rightWasFull && chargeRight >= 1f)
            PlayRechargeSound();

        leftWasFull = chargeLeft >= 1f;
        rightWasFull = chargeRight >= 1f;

        UpdateDashUI();
    }

    private void PlayRechargeSound()
    {
        if (m_DashRechargeSound != null && m_AudioSource != null)
            m_AudioSource.PlayOneShot(m_DashRechargeSound);
    }
    // ========================================

    // ✅ ESTE MÉTODO FALTABA (lo usa PlayerInputHandler)
    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public void SetInputVector(Vector2 _Direction)
    {
        m_Movement = _Direction.magnitude > 0.1f ? _Direction.normalized : Vector2.zero;
    }

    private void UpdateDashUI()
    {
        if (m_DashBarLeft != null)
            m_DashBarLeft.value = chargeLeft;

        if (m_DashBarRight != null)
            m_DashBarRight.value = chargeRight;
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

        m_Arrow.transform.rotation = Quaternion.Slerp(
            m_Arrow.transform.rotation,
            l_Rotation,
            velocidadRotacion * Time.deltaTime);
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