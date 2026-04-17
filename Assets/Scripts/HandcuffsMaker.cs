using DG.Tweening;
using System.Collections;
using UnityEngine;
public sealed class HandcuffsMaker : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private int handcuffStoredMax = 200;
    [SerializeField] private int handcuffStored;

    [Header("Crafting (1 ore -> 1 handcuff)")]
    [SerializeField] private float craftInterval = 0.5f;
    [SerializeField] private int oreInputBuffer;

    [Header("Zones")]
    [SerializeField] private Collider player;
    [SerializeField] private Transform depositeZone;
    [SerializeField] private Transform withdrawZone;
    [SerializeField] private GameObject handcuffPrefab;
    public int HandcuffStored => handcuffStored;
    public int HandcuffStoredMax => handcuffStoredMax;

    private Coroutine craftRoutine;

    /*   private void Reset()
       {
           oreDepositZone = GetComponent<Collider>();
       }*/

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other != player)
            return;

        var _player = other.GetComponentInParent<Player>();
        if (_player == null)
            return;

        var ores = _player.TakeAllCarriedOre();
        var dropped = ores.transform.childCount;


        if (dropped <= 0)
            return;

        StartCoroutine(MoveOreToZone(ores, dropped));
        DepositOre(dropped);

    }

    public IEnumerator MoveOreToZone(GameObject ores, int dropped)
    {
        for (int i = dropped - 1; i >= 0; i--)
        {
            Vector3 pos = depositeZone.GetComponent<BoxCollider>().center; pos.y = (dropped - i)*0.3f;
            Vector3 scale = new Vector3(40,15,20);
            Transform ore = ores.transform.GetChild(i);
            ore.SetParent(depositeZone);
            ore.DOLocalMove(pos, 0.3f).SetEase(Ease.OutCubic).OnComplete(() => ore.DOScale(scale * 1.5f, 0.1f).OnComplete(() => ore.DOScale(scale * 1f, 0.2f)));


            ore.transform.localRotation = Quaternion.Euler(-90f, 0, 0);

            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.3f);

        for (int i = depositeZone.childCount - 1; i >= 0; i--)
        {
            Transform ore = depositeZone.GetChild(i);
            ore.localPosition = new Vector3(-1f, 2.8f, -2.5f);
            ore.DOLocalMoveX(3f, 1f).OnComplete(() => Destroy(ore.gameObject));
            yield return new WaitForSeconds(craftInterval);

        }

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
        yield return new WaitForSeconds(0.5f); // ¿¬Ãâ¿ë
        while (oreInputBuffer > 0)
        {
            if (handcuffStored >= handcuffStoredMax)
            {
                yield return null;
                continue;
            }

            oreInputBuffer -= 1;
            handcuffStored += 1;
            var handcuff=  Instantiate(handcuffPrefab, withdrawZone.transform);
            Vector3 pos = withdrawZone.GetComponent<BoxCollider>().center; pos.y = 0.5f + handcuffStored*0.2f;
            Vector3 scale = handcuff.transform.localScale;

            handcuff.transform.localPosition = pos;
            handcuff.transform.DOScale(scale * 1.5f, 0.1f).OnComplete(() => handcuff.transform.DOScale(scale * 1f, 0.2f));

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

    public int WithdrawHandcuffs()
    {
        if (handcuffStored <= 0) return 0 ;
        var taken = HandcuffStored;
        handcuffStored = 0;
        return taken;
    }
}

