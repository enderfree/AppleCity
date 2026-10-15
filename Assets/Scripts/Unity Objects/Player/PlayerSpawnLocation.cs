using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// This class contains where and how to spawn the player
/// </summary>
public class NetworkPlayerSpawn : NetworkBehaviour
{
    // where to spawn the players
    [SerializeField] private Transform _position;

    // distance to the above position at which the players may spawn
    [SerializeField] private float _spawnRange = 5;

    /// <summary>
    /// When the player spawns on the network, teleport them to a proper spawn point
    /// </summary>
    public override void OnNetworkSpawn()
    {
        //if(!IsServer) return;
        //TeleportServerRpc();
        Teleport();
    }

    /// <summary>
    /// teleport them physically
    /// </summary>
    private void FixedUpdate()
    {
        Warp();
    }

    /// <summary>
    /// I don't think this is used anymore. I think this was replaced by OnNetworkSpawn, 
    /// but it might simply be because it was causing issues or was untested when Josh took the server down, 
    /// so I'm not removing it in case we get a chance to reestablish a server eventually
    /// </summary>
    [ServerRpc]
    private void TeleportServerRpc()
    {
        Teleport();
    
    }

    /// <summary>
    /// setup the teleportation
    /// </summary>
    private void Teleport()
    {
        _position = GameObject.FindWithTag("SpawnPoint").transform;

        Warp();



    }

    /// <summary>
    /// Physically handles the teleportation
    /// </summary>
    private async void Warp()
    {
        if (_position != null)
        {
            Vector3 random = new Vector3(Random.Range(-_spawnRange, _spawnRange), 0.5f, Random.Range(-_spawnRange, _spawnRange));

            Vector3 thePoint = _position.position + random;
            transform.position = _position.position + random;

            if (transform.position == thePoint)
            {
                await Delay(0.2f);

                this.enabled = false;
            }

        }
    }

    /// <summary>
    /// wait
    /// </summary>
    /// <param name="sec">how long do you wait in seconds</param>
    /// <returns></returns>
    private async Task Delay(float sec)
    {
        await Awaitable.WaitForSecondsAsync(sec);
    }
}
