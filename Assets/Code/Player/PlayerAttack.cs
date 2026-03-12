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

    public Transform projectileSpawnPoint;

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
    private readonly int attackAngleHash = Animator.StringToHash("AttackAngle");
    private readonly int[] areaInfo = { 14, 6, 22 };
    private readonly int[] distanceInfo = { 10, 5, 18 };
    private readonly int[] ultimateInfo = { 20, 8, 30 };

    [Header("Particles")]
    public GameObject meleeParticle;
    public GameObject areaParticle;
    public GameObject teleportParticle;
    public GameObject dashParticle;

    [Header("SFX")]
    public AudioSource sounds;
    public AudioClip melee1;
    public AudioClip melee2;
    public AudioClip area;
    public AudioClip distance;
    public AudioClip block;
    private bool useMelee1 = true;

    [Header("Hit Animation Timing")]
    public float meleeHitDelay = 0.13f;
    public float areaHitDelay = 0.22f;
    public float distanceHitDelay = 0.18f;

    [Header("Debug")]
    public bool showDebug = true;

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

    public void CancelAllAttacks()
    {
        StopAllCoroutines();

        isAttacking = false;
        attackActive = false;
        comboStep = 0;

        isPressingBlock = false;
        GetComponent<LifeController>().IsBlocking = false;

        animator.SetBool(isAttackingHash, false);
        animator.SetBool("IsBlocking", false);
        animator.SetFloat(attackIdHash, 0);

        CancelInvoke(nameof(ResetComboStep));
    }

    private float CalculateAttackAngle()
    {
        if (target == null) return 90f; 

        Vector3 direction = target.transform.position - transform.position;

        float horizontalDistance = Mathf.Abs(direction.x);

        if (horizontalDistance < 0.1f)
        {
            return direction.y > 0 ? 180f : 0f;
        }

        float angleRad = Mathf.Atan2(direction.y, horizontalDistance);
        float angle = angleRad * Mathf.Rad2Deg;

        angle = angle + 90f;

        angle = Mathf.Clamp(angle, 0f, 180f);

        if (showDebug)
        {
            Debug.Log($"Target Y difference: {direction.y:F2}, Horizontal Distance: {horizontalDistance:F2}, Angle: {angle:F1}�");
        }

        return angle;
    }

    private float GetBlendTreeValue(float angle)
    {
        float blendValue;

        if (angle >= 0f && angle < 10f)
        {
            blendValue = -1.0f; // Ataque recto abajo
        }
        else if (angle >= 10f && angle < 70f)
        {
            blendValue = -0.5f; // Ataque hacia abajo
        }
        else if (angle >= 70f && angle < 110f)
        {
            blendValue = 0.0f; // Ataque horizontal
        }
        else if (angle >= 110f && angle < 170f)
        {
            blendValue = 0.5f; // Ataque hacia arriba
        }
        else 
        {
            blendValue = 1.0f; // Ataque recto arriba
        }

        if (showDebug)
        {
            string animationName = blendValue == -1.0f ? "Recto Abajo" :
                                   blendValue == -0.5f ? "Hacia Abajo" :
                                   blendValue == 0.0f ? "Horizontal" :
                                   blendValue == 0.5f ? "Hacia Arriba" : "Recto Arriba";
            Debug.Log($"Angle {angle:F1}� -> Blend Value: {blendValue} ({animationName})");
        }

        return blendValue;
    }
    private Vector3 GetProjectileSpawnPosition()
    {
        return projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
    }

    // ================= BASIC =================
    public void MeleeAttack()
    {
        if (isAttacking) return;
        if (lifeController != null && lifeController.IsInHitStun) return;

        float cost = comboStep == 0 ? playerGains.basic1Cost :
                     comboStep == 1 ? playerGains.basic2Cost :
                                      playerGains.basic3Cost;

        powerBar.ModifyPower(-cost);

        isAttacking = true;
        lastAttackTime = Time.time;

        float attackAngle = CalculateAttackAngle();
        float blendTreeValue = GetBlendTreeValue(attackAngle);

        animator.SetFloat(attackIdHash, 0);
        animator.SetFloat(attackAngleHash, blendTreeValue);
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
        yield return new WaitForSeconds(meleeHitDelay);

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
                    useMelee1 = false;
                }
                else if (!useMelee1 && melee2 != null)
                {
                    sounds.PlayOneShot(melee2);
                    useMelee1 = true;
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

        float remainingTime = (startup[step] + active[step]) / samples - meleeHitDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

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
        if (lifeController != null && lifeController.IsInHitStun) return;

        powerBar.ModifyPower(-playerGains.strongCost);

        isAttacking = true;

        animator.SetFloat(attackIdHash, 0.5f);
        animator.SetBool(isAttackingHash, true);

        StartCoroutine(ResetAttack(areaDamage, areaInfo[0], areaInfo[1], areaInfo[2], AttackType.Area));
    }

    // ================= DISTANCE =================

    public void DistanceAttack()
    {
        if (isAttacking || target == null) return;
        if (lifeController != null && lifeController.IsInHitStun) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (powerBar.totalPower < playerGains.projectileCost / 100.0f) return;

        powerBar.ModifyPower(-playerGains.projectileCost);

        animator.SetFloat(attackIdHash, 1);
        animator.SetBool(isAttackingHash, true);

        isAttacking = true;
        lastAttackTime = Time.time;

        StartCoroutine(ResetAttack(0, distanceInfo[0], distanceInfo[1], distanceInfo[2], AttackType.Distance));
    }

    // ================= TELEPORT =================

    public void Teleport()
    {
        if (target == null) return;
        if (lifeController != null && lifeController.IsInHitStun) return;
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
        if (lifeController != null && lifeController.IsInHitStun) return;

        isPressingBlock = true;
        sounds.PlayOneShot(block);
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
        if (lifeController != null && lifeController.IsInHitStun) return;

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
            Vector3 spawnPosition = GetProjectileSpawnPosition();
            Vector3 direction =
                (target.transform.position - spawnPosition).normalized;

            GameObject go = Instantiate(prefab, spawnPosition, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.SetDirection(direction);
                projectile.damage = damage;
                projectile.speed = speed;
                projectile.ownerPlayerIndex = playerIndex;
                projectile.attackerPosition = spawnPosition;

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
        if (type == AttackType.Distance)
        {
            Vector3 spawnPosition = GetProjectileSpawnPosition();

            GameObject projectileGO = Instantiate(ball, transform.position, Quaternion.identity);
            Vector3 direction = (target.transform.position - transform.position).normalized;

            var projectile = projectileGO.GetComponent<Projectile>();

            if (projectile != null)
            {
                sounds.PlayOneShot(distance);
                projectile.SetDirection(direction);
                projectile.damage = distanceDamage;
                projectile.gainOnReceive = playerGains.projectileGainOnReceive;
                projectile.ownerPlayerIndex = playerIndex;
                projectile.attackerPosition = spawnPosition;
            }
        }

        yield return new WaitForSeconds(startupFrames / samples);

        attackActive = true;

        yield return new WaitForSeconds(activeFrames / samples);

        if(type == AttackType.Area)
        {
            Vector3 hitboxCenter = transform.position + meleeDirection * hitboxOffset.x + Vector3.up * hitboxOffset.y;
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f);

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                LifeController life = hit.GetComponent<LifeController>();
                PowerBar targetPowerBar = hit.GetComponent<PowerBar>();

                if (life != null)
                {
                    life.LoseHealth(attackDamage, transform.position);
                    powerBar.ModifyPower(playerGains.areaGainOnHit);

                    if (targetPowerBar != null)
                        powerBar.ModifyPower(playerGains.areaGainOnReceive);
                }
            }

            sounds.PlayOneShot(area);
            GameObject particle = Instantiate(areaParticle, transform.position, Quaternion.identity);
            float duration = particle.GetComponent<ParticleSystem>().main.duration;
            Destroy(particle, duration);
        }

        attackActive = false;

        yield return new WaitForSeconds(recoveryFrames / samples);

        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }
}