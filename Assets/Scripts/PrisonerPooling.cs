using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerPooling : MonoBehaviour
{
    public static PrisonerPooling instance;
    [SerializeField] private GameObject prisonerPrefab;
    [SerializeField] private int preloadCount = 20;
    public Queue<GameObject> pool = new Queue<GameObject>();

    // Spawning/queue positioning is owned by CheckPoint. This pool only provides instances.
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
            var prisoner = Instantiate(prisonerPrefab, transform);
            prisoner.SetActive(false);
            pool.Enqueue(prisoner);
        }
    }
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        var prisoner = pool.Count > 0 ? pool.Dequeue() : Instantiate(prisonerPrefab, transform);
        prisoner.transform.SetPositionAndRotation(position, rotation);
        prisoner.gameObject.SetActive(true);
        return prisoner;
    }

    public GameObject Get(Transform spawnPoint)
    {
        if (spawnPoint == null)
            return Get(Vector3.zero, Quaternion.identity);
        return Get(spawnPoint.position, spawnPoint.rotation);
    }
    public void Release(GameObject prisoner)
    {
        if (prisoner == null) return;
        prisoner.gameObject.SetActive(false);
        pool.Enqueue(prisoner);
    }

}
