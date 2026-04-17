using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerPooling : MonoBehaviour
{
    public static PrisonerPooling instance;
    [SerializeField] private GameObject prisonerPrefab;
    [SerializeField] private int preloadCount = 20;
    public  Queue<GameObject> pool = new Queue<GameObject>();

    [SerializeField] private float spacing = 1f;
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
            var prisoner = Instantiate(prisonerPrefab, transform);
            prisoner.SetActive(false);
            pool.Enqueue(prisoner);
        }

        Vector3 size = spawnRange.size;
        Vector3 center = spawnRange.center;

        for (float x = size.x / 2; x > -size.x / 2; x -= spacing)
        {
       
                Vector3 localPos = new Vector3(x, 0, 0) + center;
                Vector3 worldPos = spawnRange.transform.TransformPoint(localPos);

                var prisoner = Get(worldPos, prisonerPrefab.transform.rotation);

            
        }


    }
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        var prisoner = pool.Count > 0 ? pool.Dequeue() : Instantiate(prisonerPrefab, transform);
        prisoner.transform.SetPositionAndRotation(position, rotation);
        prisoner.gameObject.SetActive(true);
        return prisoner;
    }
    public void Release(GameObject prisoner)
    {
        if (prisoner == null) return;
        prisoner.gameObject.SetActive(false);
        pool.Enqueue(prisoner);
    }

}
