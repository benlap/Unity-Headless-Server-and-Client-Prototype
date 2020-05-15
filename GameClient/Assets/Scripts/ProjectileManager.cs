using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;
    public GameObject explosionPrefab;

    public void Initialize(int id)
    {
        this.id = id;
    }

    public void Explode(Vector3 position)
    {
        transform.position = position;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        GameManager.projectiles.Remove(id);

        Destroy(gameObject);
    }
}