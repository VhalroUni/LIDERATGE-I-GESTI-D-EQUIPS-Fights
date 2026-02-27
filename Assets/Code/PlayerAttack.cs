using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    public enum AttackType
    {
        Area,
        Distance,
        Ultimate
    }

    [Header("References")]
    public GameObject target;
    public GameObject ball;
    public Slider powerBar;
    public  Vector3 lastPosition;

    [Header("PowerBar Settings")]
    public float teleportPowerCost = 0.25f;
    public float distancePowerCost = 0.1f;
    public int areaPowerGain = 5;
    public int distancePowerGain = 5;

    [Header("Power Levels Colors")]
    public Color level1Color = Color.white;
    public Color level2Color = Color.yellow;
    public Color level3Color = Color.red;
    public Color level4Color = Color.magenta;

    [Header("Bar Smooth Settings")]
    public float fillSpeedUp = 5f;
    public float fillSpeedDown = 9f;

    [Header("Attack Settings")]
    public int playerIndex;
    public float areaDamage = 19.0f;
    public float distanceDamage = 8.0f;
    public float areaAttackDistance = 2.0f;
    public float meleeAttackDistance = 3.0f;

    //Start-up, Active, Recovery
    private readonly int[] areaInfo = { 14, 6, 22 };
    private readonly int[] distanceInfo = { 10, 5, 18 };

    public bool teleport = false;

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
    private float displayedPower = 0f;
    private const float maxTotalPower = 4f;
    private Image powerFillImage;

    private bool rainbowActive = false;

    [Header("Block")]
    public Sprite blockSprite;
    LifeController m_lifeController;

    private Vector3 meleeDirection = Vector3.right;

    [Header("Animations")]
    public float samples = 30.0f;
    public Animator animator;
    private readonly int attackIdHash = Animator.StringToHash("AttackId");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");

    void Start()
    {
        if (powerBar != null)
            powerFillImage = powerBar.fillRect.GetComponent<Image>();
        m_lifeController = GetComponent<LifeController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (powerBar != null)
        {
            float speed = (displayedPower < totalPower) ? fillSpeedUp : fillSpeedDown;
            displayedPower = Mathf.MoveTowards(displayedPower, totalPower, speed * Time.deltaTime);

            int visualLevel = Mathf.FloorToInt(displayedPower);
            float visualLocal = displayedPower - visualLevel;

            powerBar.value = visualLocal;
        }

        if (rainbowActive && powerFillImage != null)
        {
            float hue = Mathf.Repeat(Time.time * 0.5f, 1f);
            powerFillImage.color = Color.HSVToRGB(hue, 1f, 1f);
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        
        if (animator.GetBool("IsBlocking") == false)
        {
            GetComponent<LifeController>().IsBlocking = false;
        }
        lastPosition = transform.position;
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

        if (powerFillImage == null) return;

        switch (level)
        {
            case 0:
                rainbowActive = false;
                powerFillImage.color = level1Color;
                break;
            case 1:
                rainbowActive = false;
                powerFillImage.color = level2Color;
                break;
            case 2:
                rainbowActive = false;
                powerFillImage.color = level3Color;
                break;
            default:
                rainbowActive = true;
                break;
        }
    }

    public void MeleeAttack()
    {
        if (isAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetFloat(attackIdHash, 0);
        animator.SetBool(isAttackingHash, true);

        int currentStep = comboStep;

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
        Invoke(nameof(ResetComboStep), 0.1f);
    }
    
    private System.Collections.IEnumerator MeleeAttackRoutine(
        int attackDamage,
        int startupFrames,
        int activeFrames,
        int recoveryFrames,
        int powerGainValue)
    {
        yield return new WaitForSeconds(startupFrames / samples);

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

        yield return new WaitForSeconds(activeFrames / samples);

        attackActive = false;

        yield return new WaitForSeconds(recoveryFrames / samples);

        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }

    private void ResetComboStep()
    {
        comboStep = 0;
    }

    public void AreaAtack()
    {
        if (isAttacking) return;

        isAttacking = true;

        animator.SetFloat(attackIdHash, 1);
        animator.SetBool(isAttackingHash, true);

        StartCoroutine(ResetAttack(areaDamage, areaInfo[0], areaInfo[1], areaInfo[2], areaPowerGain, AttackType.Area));
    }

    public void DistanceAttack()
    {
        if (isAttacking || target == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (totalPower < distancePowerCost) return;

        animator.SetFloat(attackIdHash, 2);
        animator.SetBool(isAttackingHash, true);

        ModifyPower(-distancePowerCost);

        isAttacking = true;
        lastAttackTime = Time.time;

        StartCoroutine(ResetAttack(0, distanceInfo[0], distanceInfo[1], distanceInfo[2], 0, AttackType.Distance));
    }

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
        GetComponent<LifeController>().IsBlocking = true;
        animator.SetBool("IsBlocking", true);
        //GetComponent<SpriteRenderer>().sprite = blockSprite;
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

    private System.Collections.IEnumerator ResetAttack(float attackDamage, int startupFrames, int activeFrames, int recoveryFrames,
    int powerGainValue, AttackType type)
    {
        yield return new WaitForSeconds(startupFrames / samples);

        attackActive = true;

        yield return new WaitForSeconds(activeFrames / samples);

        Vector3 hitboxCenter = transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);

        if (type == AttackType.Distance)
        {
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
        }
        else
        {
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
        }

        attackActive = false;

        yield return new WaitForSeconds(recoveryFrames / samples);

        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, areaAttackDistance);

        if (attackActive)
        {
            Gizmos.color = Color.red;

            Vector3 hitboxCenter =
                transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

            Gizmos.DrawWireCube(hitboxCenter, hitboxSize);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var projectileId =
            collision.gameObject.GetComponent<Projectile>();

        if (collision.CompareTag("Projectile") &&
            projectileId.GetOwnerPlayerID() != playerIndex)
        {
            ModifyPower(distancePowerGain);
        }
    }
}