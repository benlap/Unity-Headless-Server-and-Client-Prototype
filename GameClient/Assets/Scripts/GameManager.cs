using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();
    public static Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject itemSpawnerPrefab;
    public GameObject projectilePrefab;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        if (id == Client.instance.myId)
        {
            player = Instantiate(localPlayerPrefab, position, rotation);
        }
        else
        {
            player = Instantiate(playerPrefab, position, rotation);
        }

        player.GetComponent<PlayerManager>().Initialize(id, username);

        players.Add(id, player.GetComponent<PlayerManager>());
    }

    public void CreateItemSpawner(int spawnerId, Vector3 position, bool hasItem)
    {
        GameObject spawner = Instantiate(itemSpawnerPrefab, position, itemSpawnerPrefab.transform.rotation);
        spawner.GetComponent<ItemSpawner>().Initialize(spawnerId, hasItem);
        itemSpawners.Add(spawnerId, spawner.GetComponent<ItemSpawner>());
    }

    public void SpawnProjectile(int id, Vector3 position)
    {
        GameObject projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        projectile.GetComponent<ProjectileManager>().Initialize(id);
        projectiles.Add(id, projectile.GetComponent<ProjectileManager>());
    }
}