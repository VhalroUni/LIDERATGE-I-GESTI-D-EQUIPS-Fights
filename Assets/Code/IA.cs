using UnityEngine;

public class IA : MonoBehaviour
{
    public GameObject rival;

    [Header("Activate Settings")]
    public bool iaControl = false;

    [Header("IA Settings")]
    public float meleeAttackDistance = 2f;
    public float areaAttackDistance = 1.5f;
    public float projectileDistance = 5f;

    public float decisionTime = 0.4f;
    public float reactionDelay = 0.15f;

    public float chaseSpeedMultiplier = 1.4f;

    [Header("Probabilidades")]
    [Range(0, 100)] public int simpleAttackChance = 50;
    [Range(0, 100)] public int areaAttackChance = 25;
    [Range(0, 100)] public int projectileChance = 20;
    [Range(0, 100)] public int ultimateChance = 10;
    [Range(0, 100)] public int teleportChance = 20;
    [Range(0, 100)] public int blockChance = 30;

    private PlayerAttack attack;
    private PlayerMovement movement;
    private PlayerController controller;
    private PowerBar powerBar;

    float distanceToRival;
    float nextDecisionTime;
    float nextActionTime;
    float nextMoveTime;

    bool isBlocking = false;

    void Start()
    {
        attack = GetComponent<PlayerAttack>();
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>();
        powerBar = GetComponent<PowerBar>();

        nextDecisionTime = Time.time + Random.Range(0.1f, 0.3f);
    }

    void Update()
    {
        if (!iaControl || rival == null) return;

        distanceToRival = Vector2.Distance(transform.position, rival.transform.position);

        if (Time.time >= nextDecisionTime)
        {
            MakeDecision();
            nextDecisionTime = Time.time + decisionTime;
        }

        UpdateMovement();
    }

    void UpdateMovement()
    {
        if (Time.time < nextMoveTime) return;

        if (attack != null && attack.animator.GetBool("IsAttacking"))
        {
            movement.SetInputVector(Vector2.zero);
            return;
        }

        if (distanceToRival > meleeAttackDistance)
        {
            Vector2 dir = (rival.transform.position - transform.position).normalized;

            if (distanceToRival > projectileDistance)
                dir *= chaseSpeedMultiplier;

            movement.SetInputVector(dir);
        }
        else
        {
            movement.SetInputVector(Vector2.zero);
        }
    }

    void MakeDecision()
    {
        if (Time.time < nextActionTime) return;

        if (attack != null && attack.animator.GetBool("IsAttacking"))
            return;

        int random = Random.Range(0, 100);

        if (distanceToRival <= meleeAttackDistance)
        {
            if (random < simpleAttackChance)
                MeleeAttack();

            else if (random < simpleAttackChance + areaAttackChance && distanceToRival <= areaAttackDistance)
                AreaAttack();

            else if (random < simpleAttackChance + areaAttackChance + blockChance)
                Block();
        }

        else if (distanceToRival <= projectileDistance)
        {
            if (random < projectileChance)
                DistanceAttack();

            else if (random < projectileChance + ultimateChance && CanUseUltimate())
                Ultimate();

            else if (random < projectileChance + ultimateChance + teleportChance && CanUseTeleport())
                Teleport();
        }
        else
        {
            if (Random.Range(0, 100) < teleportChance && CanUseTeleport())
                Teleport();
        }
    }

    void MeleeAttack()
    {
        attack.MeleeAttack();
        nextActionTime = Time.time + 0.9f;
        nextMoveTime = Time.time + reactionDelay;
        isBlocking = false;
    }

    void AreaAttack()
    {
        attack.AreaAtack();
        nextActionTime = Time.time + 1.1f;
        nextMoveTime = Time.time + reactionDelay;
        isBlocking = false;
    }

    void DistanceAttack()
    {
        if (!CanUseProjectile()) return;

        attack.DistanceAttack();
        nextActionTime = Time.time + 1.2f;
        nextMoveTime = Time.time + reactionDelay;
        isBlocking = false;
    }

    void Ultimate()
    {
        attack.Ultimate();
        nextActionTime = Time.time + 2f;
        nextMoveTime = Time.time + reactionDelay;
        isBlocking = false;
    }

    void Teleport()
    {
        attack.Teleport();
        nextActionTime = Time.time + 1f;
        nextMoveTime = Time.time + reactionDelay;
        isBlocking = false;
    }

    void Block()
    {
        if (!isBlocking)
        {
            attack.StartBlock();
            isBlocking = true;
            nextActionTime = Time.time + 1.3f;
        }
    }

    bool CanUseUltimate()
    {
        if (powerBar == null) return false;
        int level = Mathf.FloorToInt(powerBar.totalPower);
        return level == 1 || level == 2 || level == 4;
    }

    bool CanUseTeleport()
    {
        return powerBar != null && powerBar.totalPower >= 25;
    }

    bool CanUseProjectile()
    {
        return powerBar != null && powerBar.totalPower >= 10;
    }

    public void ChangeBehaviour()
    {
        iaControl = !iaControl;
        controller.enabled = !iaControl;
    }
}