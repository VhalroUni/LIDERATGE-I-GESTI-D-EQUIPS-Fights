using UnityEngine;

public class PlayerGains : MonoBehaviour
{
    [Header("=== COST ===")]
    public float basic1Cost;
    public float basic2Cost;
    public float basic3Cost;
    public float strongCost;
    public float projectileCost = 10.0f;
    public float teleportCost = 25.0f;
    public float ultimate100Cost = 0.7f;
    public float ultimate200Cost = 1.6f;
    public float ultimate400Cost = 3.5f;

    [Header("=== GAIN ON HIT ===")]
    public float basic1GainOnHit = 20.0f;
    public float basic2GainOnHit = 20.0f;
    public float basic3GainOnHit = 20.0f;
    public float areaGainOnHit = 10.0f;

    [Header("=== GAIN ON RECEIVE ===")]
    public float basic1GainOnReceive = 10.0f;
    public float basic2GainOnReceive = 10.0f;
    public float basic3GainOnReceive = 20.0f;
    public float areaGainOnReceive = 15.0f;
    public float projectileGainOnReceive = 5.0f;
}
