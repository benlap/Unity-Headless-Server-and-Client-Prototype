using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth;
    public int itemCount = 0;
    public MeshRenderer model;

    public void Initialize(int id, string username)
    {
        this.id = id;
        this.username = username;
        health = maxHealth;
    }

    public void SetHealth(float health)
    {
        this.health = health;

        if (this.health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }
}