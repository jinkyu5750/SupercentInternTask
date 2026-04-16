using System.Collections;
using UnityEngine;

public sealed class PrisonOfficer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandcuffsMaker maker;
    [SerializeField] private CheckPoint checkpoint;

    [Header("Carry")]
    [SerializeField] private int carryHandcuffMax = 30;
    [SerializeField] private int carriedHandcuffs;

    [Header("AI")]
    [SerializeField] private float thinkInterval = 0.25f;
    [SerializeField] private float interactDistance = 1.25f;

    public int CarriedHandcuffs => carriedHandcuffs;
    public int CarryHandcuffMax => carryHandcuffMax;

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

    public int TakeAllCarriedHandcuffs()
    {
        var taken = carriedHandcuffs;
        carriedHandcuffs = 0;
        return taken;
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(thinkInterval);

            if (maker == null || checkpoint == null)
                continue;

            // 1) If carrying, go deposit (in real game you'd path to checkpoint).
            if (carriedHandcuffs > 0)
            {
                if (IsNear(checkpoint.transform.position))
                {
                    checkpoint.DepositHandcuffs(TakeAllCarriedHandcuffs());
                }
                else
                {
                    MoveTowards(checkpoint.transform.position);
                }
                continue;
            }

            // 2) If checkpoint is blocked by missing cuffs, fetch some from maker.
            var missing = checkpoint.GetMissingForFront();
            if (missing <= 0)
                continue;

            if (!IsNear(maker.transform.position))
            {
                MoveTowards(maker.transform.position);
                continue;
            }

            var request = Mathf.Min(carryHandcuffMax, missing);
            var taken = maker.TryWithdrawHandcuffs(request);
            carriedHandcuffs += taken;
        }
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
        transform.position = Vector3.MoveTowards(p, targetPos, 3.5f * Time.deltaTime);
    }
}

