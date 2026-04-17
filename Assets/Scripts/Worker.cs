using System.Collections;
using UnityEngine;

public sealed class Worker : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandcuffsMaker maker;

    [Header("Mining")]
    [SerializeField] private int hitsPerOre = 2;
    [SerializeField] private float hitInterval = 0.5f;
    [SerializeField] private float searchRadius = 12f;
    [SerializeField] private LayerMask oreMask = ~0;
    [SerializeField] private GameObject orePrefab;
    [Header("Carry")]
    [SerializeField] private int carryOreMax = 1;
    [SerializeField] private int carriedOre;

    [Header("AI")]
    [SerializeField] private float thinkInterval = 0.25f;
    [SerializeField] private float interactDistance = 1.25f;
    [SerializeField] private float moveSpeed;
    private Coroutine workRoutine;

    private void OnEnable()
    {
        if (workRoutine == null && gameObject.activeInHierarchy)
            workRoutine = StartCoroutine(WorkLoop());
    }

    private void OnDisable()
    {
        if (workRoutine != null)
        {
            StopCoroutine(workRoutine);
            workRoutine = null;
        }
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(thinkInterval);

            if (maker == null)
                continue;

            var ore = FindNearestAvailableOre();
            if (ore == null)
                continue;

            if (!IsNear(ore.transform.position))
            {
                MoveTowards(ore.transform.position);
                continue;
            }

            yield return MineRoutine(ore);
        }
    }

    private IEnumerator MineRoutine(Ore ore)
    {
        if (ore == null)
            yield break;
        if (carriedOre >= carryOreMax)
            yield break;

        for (var i = 0; i < hitsPerOre; i++)
            yield return new WaitForSeconds(hitInterval);

        if (ore.TryMineOne())
        {
            var _orePrefab = Instantiate(orePrefab, transform.position, Quaternion.identity);
            StartCoroutine(maker.MoveOre_WorkerToZone(_orePrefab));
            maker.DepositOre(1);
        }
    }


    private Ore FindNearestAvailableOre()
    {
        var hits = Physics.OverlapSphere(transform.position, searchRadius, oreMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
            return null;

        Ore best = null;
        var bestSqr = float.PositiveInfinity;

        for (var i = 0; i < hits.Length; i++)
        {
            var ore = hits[i].GetComponent<Ore>();
            if (ore == null || !ore.IsAvailable)
                continue;

            var sqr = (ore.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = ore;
            }
        }

        return best;
    }

    private bool IsNear(Vector3 targetPos)
    {
        var p = transform.position;
        targetPos.y = p.y;
        return (targetPos - p).sqrMagnitude <= interactDistance * interactDistance;
    }

    private void MoveTowards(Vector3 targetPos)
    {
        // Skeleton: replace with NavMeshAgent.
        var p = transform.position;
        targetPos.y = p.y;
        transform.position = Vector3.MoveTowards(p, targetPos, moveSpeed * Time.deltaTime);
    }
}

