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
    public float decisionTime = 0.5f;
    public float reactionTime = 0.2f;

    [Header("Probabilidades de ataque")]
    [Range(0, 100)] public int simpleAttackChance = 60;
    [Range(0, 100)] public int areaAttackChance = 40;
    [Range(0, 100)] public int projectileChance = 25;
    [Range(0, 100)] public int ultimateChance = 10;
    [Range(0, 100)] public int teleportChance = 45;
    [Range(0, 100)] public int blockChance = 50;

    //References
    private PlayerAttack attack;
    private PlayerMovement movement;
    private PlayerController controller;
    private PowerBar powerBar;
    private PlayerGains gains;

    private float distanceToRival;
    private float nextActionTime;
    private float nextDecisionTime;
    private bool isBlocking = false;

    private void Start()
    {
        attack = GetComponent<PlayerAttack>();
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>();
        powerBar = GetComponent<PowerBar>();
        gains = GetComponent<PlayerGains>();

        nextDecisionTime = Time.time + Random.Range(0.1f, 0.3f);
    }

    private void Update()
    {
        if (rival == null) return;

        distanceToRival = Vector2.Distance(transform.position, rival.transform.position);

        if (iaControl)
        {
            if (Time.time >= nextDecisionTime)
            {
                MakeDecision();
                nextDecisionTime = Time.time + decisionTime;
            }

            UpdateMovement();
        }
        else
        {
            movement.SetInputVector(Vector2.zero);
            if (attack != null && isBlocking)
            {
                attack.StopBlock();
                isBlocking = false;
            }
        }
    }

    private void MakeDecision()
    {
        if (attack != null && attack.animator.GetBool("IsAttacking"))
            return;

        int random = Random.Range(0, 100);

        if (distanceToRival <= meleeAttackDistance)
        {
            if (random < simpleAttackChance)
                MeleeAttack();
            else if (random < simpleAttackChance + areaAttackChance)
            {
                if (distanceToRival <= areaAttackDistance)
                    AreaAttack();
                else
                    MeleeAttack();
            }
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
    }

    private void UpdateMovement()
    {
        Vector2 direction;
        float stoppingDistance = meleeAttackDistance * 0.8f;

        if (distanceToRival > stoppingDistance + 0.3f) //PERSEGUIR
            direction = (rival.transform.position - transform.position).normalized;
        else
            direction = (transform.position - rival.transform.position).normalized;

        movement.SetInputVector(direction);
    }

    private void MeleeAttack()
    {
        if (CanAttack())
        {
            attack.MeleeAttack();
            isBlocking = false;
            nextActionTime = Time.time + 1f;
        }
    }

    private void AreaAttack()
    {
        if (CanAttack())
        {
            attack.AreaAtack();
            isBlocking = false;
            nextActionTime = Time.time + 1f;
        }
    }

    private void DistanceAttack()
    {
        if (CanAttack() && CanUseProjectile())
        {
            attack.DistanceAttack();
            isBlocking = false;
            nextActionTime = Time.time + 1f;
        }
    }

    private void Ultimate()
    {
        if (CanAttack() && CanUseUltimate())
        {
            attack.Ultimate();
            isBlocking = false;
            nextActionTime = Time.time + 2f;
        }
    }

    private void Teleport()
    {
        if (CanUseTeleport())
        {
            attack.Teleport();
            isBlocking = false;
            nextActionTime = Time.time + 1f;
        }
    }

    private void Block()
    {
        if (attack != null && !isBlocking)
        {
            attack.StartBlock();
            isBlocking = true;
            nextActionTime = Time.time + 1.5f;
        }
    }

    private bool CanAttack()
    {
        return attack != null &&
               !attack.animator.GetBool("IsAttacking") &&
               Time.time >= nextActionTime;
    }

    private bool CanUseUltimate()
    {
        if (powerBar == null) return false;
        int level = Mathf.FloorToInt(powerBar.totalPower);
        return level == 1 || level == 2 || level == 4;
    }

    private bool CanUseTeleport()
    {
        return powerBar != null && powerBar.totalPower >= 25;
    }

    private bool CanUseProjectile()
    {
        return powerBar != null && powerBar.totalPower >= 10;
    }

    public void ChangeBehaviour()
    {
        iaControl = !iaControl;
        controller.enabled = !iaControl;
    }
}