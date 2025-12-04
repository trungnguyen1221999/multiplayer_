using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class PlayerNetWork : NetworkBehaviour
{
    [SerializeField] private Transform spawnObjectPrefab;
    [SerializeField] private float moveSpeed = 5f;
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData { _int = 56, _bool = true, message = new FixedString128Bytes("Hello") },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private List<Transform> spawnedObjects = new List<Transform>();

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
        public override string ToString() => $"Int: {_int}, Bool: {_bool}, Message: {message}";
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            randomNumber.Value = new MyCustomData {
                _int = Random.Range(1, 101),
                _bool = Random.value > 0.5f,
                message = new FixedString128Bytes("Random msg")
            };
            Debug.Log($"Assigned random number: {randomNumber.Value}");
        }
        randomNumber.OnValueChanged += (previousValue, newValue) =>
        {
            Debug.Log($"{OwnerClientId}, random number: {newValue}");
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        float moveX = 0f;
        float moveZ = 0f;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 spawnPos = transform.position + transform.forward * 2f;
            spawnPos.x = Mathf.Clamp(spawnPos.x, -10f, 10f);
            spawnPos.y = 0.5f;
            spawnPos.z = Mathf.Clamp(spawnPos.z, -10f, 10f);
            SpawnObjectClientRpc(spawnPos);
        }

        if(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            DestroyNearestObjectClientRpc();
        }

        // WASD
        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        // Arrow keys
        if (Input.GetKey(KeyCode.UpArrow)) moveZ += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveX += 1f;

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }

    [ClientRpc]
    private void SpawnObjectClientRpc(Vector3 spawnPos)
    {
        Transform spawnedObject = Instantiate(spawnObjectPrefab, spawnPos, Quaternion.identity);
        var netObj = spawnedObject.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn();
        spawnedObjects.Add(spawnedObject);
    }

    [ClientRpc]
    private void DestroyNearestObjectClientRpc()
    {
        if (spawnedObjects.Count > 0)
        {
            Transform obj = spawnedObjects[spawnedObjects.Count - 1];
            var netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn();
            Destroy(obj.gameObject);
            spawnedObjects.RemoveAt(spawnedObjects.Count - 1);
        }
    }
}
