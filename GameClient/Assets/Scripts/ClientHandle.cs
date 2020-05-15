using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myId = myId;

        //Send confirmation packet back to the server.
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.players[id].transform.position = position;
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.players[id].transform.rotation = rotation;
    }

    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();

        Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
    }

    public static void PlayerHealth(Packet packet)
    {
        int id = packet.ReadInt();
        float health = packet.ReadFloat();

        GameManager.players[id].SetHealth(health);
    }

    public static void PlayerRespawned(Packet packet)
    {
        int id = packet.ReadInt();

        GameManager.players[id].Respawn();
    }

    public static void CreateItemSpawner(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        Vector3 spawnerPosition = packet.ReadVector3();
        bool hasItem = packet.ReadBool();

        GameManager.instance.CreateItemSpawner(spawnerId, spawnerPosition, hasItem);
    }

    public static void ItemSpawned(Packet packet)
    {
        int spawnerId = packet.ReadInt();

        GameManager.itemSpawners[spawnerId].ItemSpawned();
    }
    public static void ItemPickedUp(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        int byPlayer = packet.ReadInt();

        GameManager.itemSpawners[spawnerId].ItemPickedUp();
        GameManager.players[byPlayer].itemCount++;
    }

    public static void SpawnProjectile(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        int thrownByPlayer = packet.ReadInt();

        GameManager.instance.SpawnProjectile(projectileId, position);
        GameManager.players[thrownByPlayer].itemCount--;
    }

    public static void ProjectilePosition(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.projectiles[projectileId].transform.position = position;
    }

    public static void ProjectileExploded(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.projectiles[projectileId].Explode(position);
    }
}