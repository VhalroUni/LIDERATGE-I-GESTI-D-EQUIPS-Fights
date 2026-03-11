using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public float maxBlockDuration = 2f;
    public float blockCooldown = 1.5f;
    public Slider blockBar;

    private float currentBlockTime = 0f;
    private float blockCooldownTimer = 0f;
    private bool isBlockOnCooldown = false;
    private bool isPressingBlock = false;
    private Image blockBarFillImage;

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

    [Header("Particles")]
    public GameObject meleeParticle;
    public GameObject areaParticle;
    public GameObject teleportParticle;
    public GameObject dashParticle;

    [Header("Effects")]
    public AudioSource sounds;
    public AudioClip melee1;
    public AudioClip melee2;
    public AudioClip area;
    public AudioClip distance;
    private bool useMelee1 = true;

    void Start()
    {
        lifeController = GetComponent<LifeController>();
        animator = GetComponent<Animator>();
        playerGains = GetComponent<PlayerGains>();
        powerBar = GetComponent<PowerBar>();
        sounds = GetComponent<AudioSource>();

        currentBlockTime = maxBlockDuration;

        if (blockBar != null)
        {
            blockBar.maxValue = maxBlockDuration;
            blockBar.value = currentBlockTime;
            blockBarFillImage = blockBar.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (animator.GetBool("IsBlocking") == false)
        {
            GetComponent<LifeController>().IsBlocking = false;
        }

        lastPosition = transform.position;

        UpdateBlockSystem();
    }

    private void UpdateBlockSystem()
    {
        if (isBlockOnCooldown)
        {
            blockCooldownTimer += Time.deltaTime;

            if (blockCooldownTimer >= blockCooldown)
            {
                isBlockOnCooldown = false;
                blockCooldownTimer = 0f;
                currentBlockTime = maxBlockDuration;
            }

            UpdateBlockUI();
            return;
        }

        if (isPressingBlock && currentBlockTime > 0f)
        {
            currentBlockTime -= Time.deltaTime;

            if (currentBlockTime <= 0f)
            {
                currentBlockTime = 0f;
                StopBlock();
                isBlockOnCooldown = true;
            }
        }
        else if (!isPressingBlock && currentBlockTime < maxBlockDuration)
        {
            currentBlockTime += Time.deltaTime * 0.5f; 
            currentBlockTime = Mathf.Min(currentBlockTime, maxBlockDuration);
        }

        UpdateBlockUI();
    }

    private void UpdateBlockUI()
    {
        if (blockBar == null || blockBarFillImage == null) return;

        if (isBlockOnCooldown)
        {
            blockBar.value = (blockCooldownTimer / blockCooldown) * maxBlockDuration;
            blockBarFillImage.color = Color.red;
        }
        else
        {
            blockBar.value = currentBlockTime;

            float percent = currentBlockTime / maxBlockDuration;
            if (percent > 0.5f)
                blockBarFillImage.color = Color.cyan;
            else if (percent > 0.25f)
                blockBarFillImage.color = Color.yellow;
            else
                blockBarFillImage.color = Color.red;
        }
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
            GameObject particle = Instantiate(meleeParticle, hit.transform);
            float duration = particle.GetComponent<ParticleSystem>().main.duration;
            Destroy(particle, duration);
            if (life != null)
            {
                if (useMelee1 && melee1 != null)
                {
                    sounds.PlayOneShot(melee1);
                    useMelee1 = false;  // Cambiar para el próximo golpe
                }
                else if (!useMelee1 && melee2 != null)
                {
                    sounds.PlayOneShot(melee2);
                    useMelee1 = true;   // Cambiar para el próximo golpe
                }
                life.LoseHealth(damage[step], transform.position);

                float gainOnHit = step == 0 ? playerGains.basic1GainOnHit :
                             step == 1 ? playerGains.basic2GainOnHit :
                                         playerGains.basic3GainOnHit;

                float gainOnReceive = step == 0 ? playerGains.basic1GainOnReceive :
                             step == 1 ? playerGains.basic2GainOnReceive :
                                         playerGains.basic3GainOnReceive;

                if (life.IsBlocking)
                {
                    gainOnHit *= 0.1f;
                }

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
        if (!isAttacking)
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

        GameObject particle = Instantiate(teleportParticle, gameObject.transform.position, Quaternion.identity);
        float duration = particle.GetComponent<ParticleSystem>().main.duration;
        Destroy(particle, duration);

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

    public void StartBlock()
    {
        if (isBlockOnCooldown || currentBlockTime <= 0f) return;

        isPressingBlock = true;
        GetComponent<LifeController>().IsBlocking = true;
        animator.SetBool("IsBlocking", true);
    }

    public void StopBlock()
    {
        isPressingBlock = false;
        GetComponent<LifeController>().IsBlocking = false;
        animator.SetBool("IsBlocking", false);
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
                projectile.attackerPosition = transform.position;

                if (level == 4)
                {
                    projectile.ignoreBlock = true;
                }
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
                sounds.PlayOneShot(distance);
                projectile.SetDirection(direction);
                projectile.damage = distanceDamage;
                projectile.gainOnReceive = playerGains.projectileGainOnReceive;
                projectile.ownerPlayerIndex = playerIndex;
                projectile.attackerPosition = transform.position;
            }
        }
        else //AREA
        {
            Vector3 hitboxCenter =
                transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;

            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                LifeController life = hit.GetComponent<LifeController>();
                PowerBar targetPowerBar = hit.GetComponent<PowerBar>();
                GameObject particle = Instantiate(areaParticle, hit.transform);
                float duration = particle.GetComponent<ParticleSystem>().main.duration;
                Destroy(particle, duration);
                if (life != null)
                {
                    sounds.PlayOneShot(area);
                    life.LoseHealth(attackDamage, transform.position);
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