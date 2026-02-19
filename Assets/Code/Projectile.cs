using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
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
        if (life != null)
        {
            life.LoseHealth(damage);
            Destroy(gameObject);
        }
    }
}