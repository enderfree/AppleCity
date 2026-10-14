using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

public class NetworkPlayerSpawn : NetworkBehaviour
{
    [SerializeField] private Transform _position;
    [SerializeField] private float _spawnRange = 5;

    public override void OnNetworkSpawn()
    {
        //if(!IsServer) return;
        //TeleportServerRpc();
        Teleport();
    }

    private void FixedUpdate()
    {
        Warp();
    }

    [ServerRpc]

    private void TeleportServerRpc()
    {
        Teleport();
    
    }
    private void Teleport()
    {
        _position = GameObject.FindWithTag("SpawnPoint").transform;

        Warp();



    }

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

    private async Task Delay(float sec)
    {
        await Awaitable.WaitForSecondsAsync(sec);
    }
}
