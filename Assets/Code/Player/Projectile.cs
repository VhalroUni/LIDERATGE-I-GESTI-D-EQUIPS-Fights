using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float damage;
    public float gainOnReceive;
    public int ownerPlayerIndex = -1;

    private Vector2 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = ((Vector2)dir).normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public int GetOwnerPlayerID()
    {
        return ownerPlayerIndex;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var targetAttack = other.GetComponent<PlayerAttack>();
        if (targetAttack != null && targetAttack.GetPlayerIndex() == ownerPlayerIndex)
            return;

        var life = other.GetComponent<LifeController>();
        var power = other.GetComponent<PowerBar>();
        if (life != null)
        {
            life.LoseHealth(damage);
            power.ModifyPower(gainOnReceive);
            Destroy(gameObject);
        }

        Destroy(gameObject, 3f);
    }
}