using DG.Tweening;
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
    [SerializeField] private Collider player;
    [SerializeField] private Transform depositZone;
    private Queue<GameObject> queue = new Queue<GameObject>();
    private Coroutine processRoutine;


    public int HandcuffsDeposited => handcuffsDeposited;

    /*  private void Reset()
      {
          player = GetComponent<Collider>();
      }*/

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other != player)
            return;

        var _player = other.GetComponentInParent<Player>();
        if (_player != null)
        {
            ChangeZoneColor(true);
            var handcuffs = _player.TakeAllCarriedHandcuffs();
            var dropped = handcuffs.transform.childCount;
            if (dropped > 0)
            {
                DepositHandcuffs(dropped);
                StartCoroutine(MoveHandcuffToZone(handcuffs, dropped));

            }
            return;
        }

        /*     var officer = other.GetComponentInParent<PrisonOfficer>();
             if (officer != null)
             {
                 var dropped = officer.TakeAllCarriedHandcuffs();
                 if (dropped > 0)
                     DepositHandcuffs(dropped);
             }*/
    }

    private void OnTriggerExit(Collider other)
    {
        ChangeZoneColor(false);
    }
    public void ChangeZoneColor(bool active)
    {
        depositZone.GetComponent<MeshRenderer>().material.color = active? new Color32(50, 255, 0,255):new Color32(255,255,255,255);
    }
    public IEnumerator MoveHandcuffToZone(GameObject handcuffs, int dropped)
    {
        for (int i = dropped - 1; i >= 0; i--)
        {
            Vector3 pos = transform.GetComponent<BoxCollider>().center; pos.x -= 0.2f; pos.y = 0.5f + (dropped - i) * 0.2f;
            Vector3 scale = handcuffs.transform.localScale; scale.y *= 5f;
            Transform handcuff = handcuffs.transform.GetChild(i);
            handcuff.SetParent(transform);
            handcuff.DOLocalMove(pos, 0.3f).SetEase(Ease.OutCubic).OnComplete(() => handcuff.DOScale(scale * 1.5f, 0.1f).OnComplete(() => handcuff.DOScale(scale * 1f, 0.2f)));


            handcuff.transform.localRotation = Quaternion.identity;

            yield return new WaitForSeconds(0.05f);
        }


    }

    public int GetFrontDemand()
    {
        if (queue.Count == 0)
            return 0;
        return queue.Peek().GetComponent<Prisoner>().RequiredHandcuffs;
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
            queue = PrisonerPooling.instance.pool;
            if (queue.Count == 0)
                break;

            var front = queue.Peek().GetComponent<Prisoner>();
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
            //    queue.Dequeue();

            front.Imprison(prisonMoveTarget);

            // Reward hookup is intentionally loose in skeleton.
            // Typically you'd call GameEconomy.AddMoney(front.RewardMoney) or notify Player.

            yield return new WaitForSeconds(imprisonInterval);
        }

        processRoutine = null;
    }
}

