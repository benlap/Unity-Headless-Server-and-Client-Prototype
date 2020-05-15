using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rb;
    public int thrownByPlayer;
    public Vector3 initialForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, thrownByPlayer);

        rb.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialize(Vector3 initialMovementDirection, float initialforceStrength, int thrownByPlayer)
    {
        initialForce = initialMovementDirection * initialforceStrength;
        this.thrownByPlayer = thrownByPlayer;
    }

    private void Explode()
    {
        ServerSend.ProjectileExploded(this);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach(var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                collider.GetComponent<Player>().TakeDamage(explosionDamage);
            }
            projectiles.Remove(id);
            Destroy(gameObject);
        }
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(10f);

        Explode();
    }
}
