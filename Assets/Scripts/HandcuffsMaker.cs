using System.Collections;
using UnityEngine;

public sealed class HandcuffsMaker : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private int handcuffStoredMax = 200;
    [SerializeField] private int handcuffStored;

    [Header("Crafting (1 ore -> 1 handcuff)")]
    [SerializeField] private float craftInterval = 0.05f;
    [SerializeField] private int oreInputBuffer;

    [Header("Zones")]
    [SerializeField] private Collider oreDepositZone;

    public int HandcuffStored => handcuffStored;
    public int HandcuffStoredMax => handcuffStoredMax;

    private Coroutine craftRoutine;

    private void Reset()
    {
        oreDepositZone = GetComponent<Collider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (oreDepositZone != null && other != oreDepositZone)
            return;

        var player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        var dropped = player.TakeAllCarriedOre();
        if (dropped <= 0)
            return;

        DepositOre(dropped);
    }

    public void DepositOre(int amount)
    {
        if (amount <= 0)
            return;

        oreInputBuffer += amount;

        if (craftRoutine == null && gameObject.activeInHierarchy)
            craftRoutine = StartCoroutine(CraftLoop());
    }

    private IEnumerator CraftLoop()
    {
        while (oreInputBuffer > 0)
        {
            if (handcuffStored >= handcuffStoredMax)
            {
                yield return null;
                continue;
            }

            oreInputBuffer -= 1;
            handcuffStored += 1;
            yield return new WaitForSeconds(craftInterval);
        }

        craftRoutine = null;
    }

    public int TryWithdrawHandcuffs(int requested)
    {
        if (requested <= 0)
            return 0;

        var taken = Mathf.Min(requested, handcuffStored);
        handcuffStored -= taken;
        return taken;
    }
}

