using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed class HandcuffsMaker : MonoBehaviour
{
    private Stack<GameObject> oreQueue = new Stack<GameObject>();
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



        StartCoroutine(MoveOre_PlayerToZone(ores, dropped));
        DepositOre(dropped);
    }

    public IEnumerator MoveOre_PlayerToZone(GameObject ores, int dropped)
    {

        for (int i = dropped - 1; i >= 0; i--)
        {
            var ore = ores.transform.GetChild(i).gameObject;
            oreQueue.Push(ore);
            MoveSingleOreToZone(ore.transform);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.3f);

        while (oreQueue.Count > 0)
        {
            var ore = oreQueue.Pop();
            MoveZoneToConveyor(ore);
            yield return new WaitForSeconds(craftInterval);

        }
    }


    public IEnumerator MoveOre_WorkerToZone(GameObject ore)
    {
        
        yield return MoveSingleOreToZone(ore.transform, 1f).WaitForCompletion();
        MoveZoneToConveyor(ore);
    }
 
    public Tween MoveSingleOreToZone(Transform ore, float moveSpeed = 0.3f)
    {
        Vector3 scale = new Vector3(40, 15, 20);
        Vector3 pos = depositeZone.GetComponent<BoxCollider>().center;
        pos.y = 0.5f + depositeZone.childCount * 0.5f;

        ore.SetParent(depositeZone);

        ore.localRotation = Quaternion.Euler(-90f, 0, 0);
        Sequence seq = DOTween.Sequence(); 
        seq.Append(ore.DOLocalMove(pos, moveSpeed).SetEase(Ease.OutCubic));
        seq.Append(ore.DOScale(scale * 1.5f, 0.1f));
        seq.Append(ore.DOScale(scale, 0.2f));
        return seq; 
    }
    private void MoveZoneToConveyor(GameObject ore)
    {
        ore.transform.SetParent(transform);
        ore.transform.localPosition = new Vector3(0f, 1.2f, 3f);
        ore.transform.DOLocalMoveZ(0f, 1f).OnComplete(() => Destroy(ore.gameObject));
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
        yield return new WaitForSeconds(1f); // 연출용
        while (oreInputBuffer > 0)
        {
            if (handcuffStored >= handcuffStoredMax)
            {
                yield return null;
                continue;
            }

            oreInputBuffer -= 1;
            handcuffStored += 1;
            var handcuff = Instantiate(handcuffPrefab, withdrawZone.transform);
            Vector3 pos = withdrawZone.GetComponent<BoxCollider>().center; pos.y = 0.5f + handcuffStored * 0.2f;
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
        if (handcuffStored <= 0) return 0;
        var taken = HandcuffStored;
        handcuffStored = 0;
        return taken;
    }
}

