using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CheckPoint : MonoBehaviour
{
    [Header("Storage (deposited handcuffs)")]
    [SerializeField] private int handcuffsDeposited;

    [Header("Processing")]
    [SerializeField] private float imprisonInterval = 0.15f;
    [SerializeField] private Transform prisonMoveTarget;

    [Header("Zones")]
    [SerializeField] private Collider handcuffDepositZone;

    private readonly Queue<Prisoner> queue = new Queue<Prisoner>();
    private Coroutine processRoutine;

    public int HandcuffsDeposited => handcuffsDeposited;

    private void Reset()
    {
        handcuffDepositZone = GetComponent<Collider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (handcuffDepositZone != null && other != handcuffDepositZone)
            return;

        var player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            var dropped = player.TakeAllCarriedHandcuffs();
            if (dropped > 0)
                DepositHandcuffs(dropped);
            return;
        }

        var officer = other.GetComponentInParent<PrisonOfficer>();
        if (officer != null)
        {
            var dropped = officer.TakeAllCarriedHandcuffs();
            if (dropped > 0)
                DepositHandcuffs(dropped);
        }
    }

    public void EnqueuePrisoner(Prisoner prisoner)
    {
        if (prisoner == null)
            return;

        queue.Enqueue(prisoner);
        EnsureProcessing();
    }

    public int GetFrontDemand()
    {
        if (queue.Count == 0)
            return 0;
        return queue.Peek().RequiredHandcuffs;
    }

    public int GetMissingForFront()
    {
        var demand = GetFrontDemand();
        if (demand <= 0)
            return 0;
        return Mathf.Max(0, demand - handcuffsDeposited);
    }

    public void DepositHandcuffs(int amount)
    {
        if (amount <= 0)
            return;

        handcuffsDeposited += amount;
        EnsureProcessing();
    }

    private void EnsureProcessing()
    {
        if (processRoutine == null && gameObject.activeInHierarchy)
            processRoutine = StartCoroutine(ProcessLoop());
    }

    private IEnumerator ProcessLoop()
    {
        while (true)
        {
            if (queue.Count == 0)
                break;

            var front = queue.Peek();
            var need = front.RequiredHandcuffs;
            if (need <= 0)
            {
                queue.Dequeue();
                continue;
            }

            if (handcuffsDeposited < need)
            {
                yield return null;
                continue;
            }

            handcuffsDeposited -= need;
            queue.Dequeue();

            front.Imprison(prisonMoveTarget);

            // Reward hookup is intentionally loose in skeleton.
            // Typically you'd call GameEconomy.AddMoney(front.RewardMoney) or notify Player.

            yield return new WaitForSeconds(imprisonInterval);
        }

        processRoutine = null;
    }
}

