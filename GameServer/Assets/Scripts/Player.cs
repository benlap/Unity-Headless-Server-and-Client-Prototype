using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float throwForce = 600f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int id, string username)
    {
        this.id = id;
        this.username = username;
        health = maxHealth;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 inputDirection = Vector2.zero;

        if (inputs[0])
            inputDirection.y += 1;

        if (inputs[1])
            inputDirection.y -= 1;

        if (inputs[2])
            inputDirection.x -= 1;

        if (inputs[3])
            inputDirection.x += 1;

        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {        
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4]) //Spacebar pressed
            {
                yVelocity = jumpSpeed;
            }
        }

        yVelocity += gravity;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] inputs, Quaternion rotation)
    {
        this.inputs = inputs;
        transform.rotation = rotation;
    }

    public void Shoot(Vector3 viewDirection)
    {
        if (health <= 0)
            return;

        if (Physics.Raycast(shootOrigin.position, viewDirection, out RaycastHit hit, 25f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }

    public void ThrowItem(Vector3 viewDirection)
    {
        if (health <= 0f)
            return;

        if (itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(viewDirection, throwForce, id);
        }
    }

    public void TakeDamage(float damage)
    {
        if (health <= 0f)
            return;

        health -= damage;
        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;

        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        Debug.Log("Attempt item pickup.");
        
        if (itemAmount >= maxItemAmount)
            return false;

        itemAmount++;
        return true;
    }
}