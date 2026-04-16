using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrePooling : MonoBehaviour
{
    public static OrePooling instance;
    [SerializeField] private GameObject orePrefab;
    [SerializeField] private float respawnTime = 1.5f;
    [SerializeField] private int preloadCount = 20;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    [SerializeField] private float spacing = 2f;
    [SerializeField] private BoxCollider spawnRange;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        Preload();
    }
    private void Preload()
    {
        for (int i = 0; i < preloadCount; i++)
        {
            var ore = Instantiate(orePrefab, transform);
            ore.SetActive(false);
            pool.Enqueue(ore);
        }

        Vector3 size = spawnRange.size;
        Vector3 center = spawnRange.center;

        for (float x = -size.x / 2; x < size.x / 2; x += spacing)
        {
            for (float z = (-size.z / 2) + 2f; z < size.z / 2; z += spacing)
            {
                Vector3 localPos = new Vector3(x, 0, z) + center;
                Vector3 worldPos = spawnRange.transform.TransformPoint(localPos);

                var ore = Get(worldPos,orePrefab.transform.rotation);    

            }
        }

       
    }
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        var ore = pool.Count > 0 ? pool.Dequeue() : Instantiate(orePrefab, transform);
        ore.transform.SetPositionAndRotation(position, rotation);
        ore.gameObject.SetActive(true);
        return ore;
    }
    public void Release(GameObject ore)
    {
        if (ore == null) return;
        ore.gameObject.SetActive(false);
        pool.Enqueue(ore);
    }

    public void DespawnAndScheduleRespawn(GameObject ore)
    {
        if (ore == null) return;
        // “그 자리”를 저장해두고
        Vector3 pos = ore.transform.position;
        Quaternion rot = ore.transform.rotation;
        // 즉시 디스폰(비활성화) + 풀로 반환
        Release(ore);
        // N초 후 다시 스폰
        StartCoroutine(RespawnRoutine(pos, rot));


    }
    private IEnumerator RespawnRoutine(Vector3 pos, Quaternion rot)
    {
        yield return new WaitForSeconds(respawnTime);
        Get(pos, rot);
    }
}
