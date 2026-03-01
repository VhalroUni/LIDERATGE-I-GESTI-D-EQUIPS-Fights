using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public enum AttackType
    {
        Area,
        Distance
    }

    [Header("References")]
    public GameObject target;
    public GameObject ball;
    public Vector3 lastPosition;
    private PlayerGains playerGains;
    private PowerBar powerBar;

    [Header("Attack Settings")]
    public int playerIndex;
    public float areaDamage = 19.0f;
    public float distanceDamage = 8.0f;
    public float areaAttackDistance = 2.0f;
    public float meleeAttackDistance = 3.0f;

    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

    private int comboStep = 0;
    private bool attackActive = false;

    [Header("Ultimate Settings")]
    public GameObject ultiLevel1Projectile;
    public GameObject ultiLevel2Projectile;
    public GameObject ultiLevel4Projectile;

    public float ultiLevel1Damage = 15f;
    public float ultiLevel2Damage = 25f;
    public float ultiLevel4Damage = 60f;

    public float ultiLevel1Speed = 8f;
    public float ultiLevel2Speed = 10f;
    public float ultiLevel4Speed = 15f;

    public bool teleport = false;

    [Header("Melee Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(1.5f, 1f);
    public Vector2 hitboxOffset = new Vector2(1f, 0f);

    private readonly int[] damage = { 4, 5, 7 };
    private readonly int[] startup = { 5, 6, 8 };
    private readonly int[] active = { 3, 3, 4 };
    private readonly int[] recovery = { 10, 12, 16 };

    [Header("Block")]
    public Sprite blockSprite;
    LifeController lifeController;

    private Vector3 meleeDirection = Vector3.right;

    [Header("Animations")]
    public float samples = 30.0f;
    public Animator animator;
    private readonly int attackIdHash = Animator.StringToHash("AttackId");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int[] areaInfo = { 14, 6, 22 };
    private readonly int[] distanceInfo = { 10, 5, 18 };
    private readonly int[] ultimateInfo = { 20, 8, 30 };

    void Start()
    {
        lifeController = GetComponent<LifeController>();
        animator = GetComponent<Animator>();
        playerGains = GetComponent<PlayerGains>();
        powerBar = GetComponent<PowerBar>();
    }

    void Update()
    {
        if (animator.GetBool("IsBlocking") == false)
        {
            GetComponent<LifeController>().IsBlocking = false;
        }

        lastPosition = transform.position;
    }

    // ================= BASIC =================
    public void MeleeAttack()
    {
        if (isAttacking) return;
        //if (Time.time - lastAttackTime < attackCooldown) return;

        float cost = comboStep == 0 ? playerGains.basic1Cost :
                     comboStep == 1 ? playerGains.basic2Cost :
                                      playerGains.basic3Cost;

        powerBar.ModifyPower(-cost);

        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetFloat(attackIdHash, 0);
        animator.SetBool(isAttackingHash, true);

        int step = comboStep;

        meleeDirection = target != null ?
            (target.transform.position - transform.position).normalized :
            Vector3.right;

        StartCoroutine(MeleeAttackRoutine(step));

        comboStep++;
        if (comboStep >= 3)
            comboStep = 0;

        CancelInvoke(nameof(ResetComboStep));

        float totalAnimationTime = (startup[step] + active[step] + recovery[step]) / samples;
        float resetDelay = totalAnimationTime - 0.2f;

        Invoke(nameof(ResetComboStep), Mathf.Max(0.1f, resetDelay));
    }

    private IEnumerator MeleeAttackRoutine(int step)
    {
        yield return new WaitForSeconds(startup[step] / samples);

        attackActive = true;

        Vector3 hitboxCenter =
            transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            LifeController life = hit.GetComponent<LifeController>();
            PowerBar targetPowerBar = hit.GetComponent<PowerBar>();
            if (life != null)
            {
                life.LoseHealth(damage[step]);

                float gainOnHit = step == 0 ? playerGains.basic1GainOnHit :
                             step == 1 ? playerGains.basic2GainOnHit :
                                         playerGains.basic3GainOnHit;

                float gainOnReceive = step == 0 ? playerGains.basic1GainOnReceive :
                             step == 1 ? playerGains.basic2GainOnReceive :
                                         playerGains.basic3GainOnReceive;

                powerBar.ModifyPower(gainOnHit);
                targetPowerBar.ModifyPower(gainOnReceive);
            }
        }

        yield return new WaitForSeconds(active[step] / samples);
        attackActive = false;
        yield return new WaitForSeconds(recovery[step] / samples);

        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }

    private void ResetComboStep()
    {
        if(!isAttacking)
            comboStep = 0;
    }

    // ================= AREA =================

    public void AreaAtack()
    {
        if (isAttacking) return;

        powerBar.ModifyPower(-playerGains.strongCost);

        isAttacking = true;

        animator.SetFloat(attackIdHash, 1);
        animator.SetBool(isAttackingHash, true);

        StartCoroutine(ResetAttack(areaDamage, areaInfo[0], areaInfo[1], areaInfo[2], AttackType.Area));
    }

    // ================= DISTANCE =================

    public void DistanceAttack()
    {
        if (isAttacking || target == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (powerBar.totalPower < playerGains.projectileCost / 100.0f) return;
        
        powerBar.ModifyPower(-playerGains.projectileCost);

        animator.SetFloat(attackIdHash, 2);
        animator.SetBool(isAttackingHash, true);

        isAttacking = true;
        lastAttackTime = Time.time;

        StartCoroutine(ResetAttack(0, distanceInfo[0], distanceInfo[1], distanceInfo[2], AttackType.Distance));
    }

    // ================= TELEPORT =================

    public void Teleport()
    {
        if (target == null) return;
        if (powerBar.totalPower < playerGains.teleportCost / 100.0f) return;

        powerBar.ModifyPower(-playerGains.teleportCost);

        Vector3 direction =
            (target.transform.position - transform.position).normalized;

        transform.position =
            target.transform.position + direction * 1f;

        teleport = true;
    }

    public bool GetTeleportBool() => teleport;
    public void SetTeleportBool(bool value) => teleport = value;
    public int GetPlayerIndex() => playerIndex;

    // ================= BLOCK =================

    public void Block()
    {
        GetComponent<LifeController>().IsBlocking = true;
        animator.SetBool("IsBlocking", true);
    }

    // ================= ULTIMATE =================

    public void Ultimate()
    {
        if (isAttacking || target == null) return;

        int level = Mathf.FloorToInt(powerBar.totalPower);

        if (level != 1 && level != 2 && level != 4)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetFloat(attackIdHash, 3);
        animator.SetBool(isAttackingHash, true);

        StartCoroutine(UltimateRoutine(level));
    }

    private IEnumerator UltimateRoutine(int level)
    {
        yield return new WaitForSeconds(ultimateInfo[0] / samples);

        GameObject prefab = null;
        float damage = 0f;
        float speed = 0f;
        float cost = 0f;

        if (level == 1)
        {
            prefab = ultiLevel1Projectile ?? ball;
            damage = ultiLevel1Damage;
            speed = ultiLevel1Speed;
            cost = playerGains.ultimate100Cost;
        }
        else if (level == 2)
        {
            prefab = ultiLevel2Projectile ?? ball;
            damage = ultiLevel2Damage;
            speed = ultiLevel2Speed;
            cost = playerGains.ultimate200Cost;
        }
        else if (level == 4)
        {
            prefab = ultiLevel4Projectile ?? ball;
            damage = ultiLevel4Damage;
            speed = ultiLevel4Speed;
            cost = playerGains.ultimate400Cost;
        }

        powerBar.ModifyPower(-cost);

        if (prefab != null)
        {
            Vector3 direction =
                (target.transform.position - transform.position).normalized;

            GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.SetDirection(direction);
                projectile.damage = damage;
                projectile.speed = speed;
                projectile.ownerPlayerIndex = playerIndex;
            }
        }

        yield return new WaitForSeconds(ultimateInfo[2] / samples);

        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }

    private IEnumerator ResetAttack(float attackDamage, int startupFrames, int activeFrames, int recoveryFrames, AttackType type)
    {
        yield return new WaitForSeconds(startupFrames / samples);

        attackActive = true;
        yield return new WaitForSeconds(activeFrames / samples);

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
                projectile.gainOnReceive = playerGains.projectileGainOnReceive;
                projectile.ownerPlayerIndex = playerIndex;
            }
        }
        else
        {
            Vector3 hitboxCenter =
                transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                LifeController life = hit.GetComponent<LifeController>();
                PowerBar targetPowerBar = hit.GetComponent<PowerBar>();
                if (life != null)
                {
                    life.LoseHealth(attackDamage);
                    powerBar.ModifyPower(playerGains.areaGainOnHit);
                    targetPowerBar.ModifyPower(playerGains.areaGainOnReceive);
                }
            }
        }

        attackActive = false;
        yield return new WaitForSeconds(recoveryFrames / samples);

        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }
}