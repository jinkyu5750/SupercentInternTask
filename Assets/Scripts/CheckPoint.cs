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

    [Header("Prisoner Queue (line in front of checkpoint)")]
    [SerializeField] private Transform[] queueSlots;
    [SerializeField] private float slotMoveDuration = 0.2f;
    [SerializeField] private Ease slotMoveEase = Ease.OutCubic;
    [SerializeField] private bool autoFillOnStart = true;

    [Header("Zones")]
    [SerializeField] private Collider player;
    [SerializeField] private Transform depositZone;
    private readonly List<GameObject> linedPrisoners = new List<GameObject>();
    private Coroutine processRoutine;


    public int HandcuffsDeposited => handcuffsDeposited;

    /*  private void Reset()
      {
          player = GetComponent<Collider>();
      }*/

    private void Start()
    {
        EnsureLineListSize();
        if (autoFillOnStart)
            FillBackUntilFull();
    }

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
            Vector3 pos = transform.GetComponent<BoxCollider>().center; pos.x -= 0.2f; pos.y = 0.5f + transform.childCount * 0.2f;
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
        if (queueSlots == null || queueSlots.Length == 0)
            return 0;

        EnsureLineListSize();
        var frontGo = linedPrisoners.Count > 0 ? linedPrisoners[0] : null;
        if (frontGo == null)
            return 0;
        var prisoner = frontGo.GetComponent<Prisoner>();
        return prisoner != null ? prisoner.RequiredHandcuffs : 0;
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
            EnsureLineListSize();

            if (queueSlots == null || queueSlots.Length == 0)
                break;

            var frontGo = linedPrisoners[0];
            if (frontGo == null)
            {
                FillBackUntilFull();
                yield return null;
                continue;
            }

            var front = frontGo.GetComponent<Prisoner>();
            if (front == null)
            {
                linedPrisoners[0] = null;
                ShiftForward();
                FillBackUntilFull();
                yield return null;
                continue;
            }

            var need = front.RequiredHandcuffs;
            if (need <= 0)
            {
                linedPrisoners[0] = null;
                ShiftForward();
                FillBackUntilFull();
                continue;
            }

            if (handcuffsDeposited < need)
            {
                yield return null;
                continue;
            }

            handcuffsDeposited -= need;
            front.Imprison(prisonMoveTarget);
            //¿©±â¼­ ¼ö°© Á¤¸®ÇØ¾ßµÊ
            // Remove the front prisoner from the line immediately so the rest can advance.
            linedPrisoners[0] = null;
            ShiftForward();
            FillBackUntilFull();

            // Reward hookup is intentionally loose in skeleton.
            // Typically you'd call GameEconomy.AddMoney(front.RewardMoney) or notify Player.

            yield return new WaitForSeconds(imprisonInterval);
        }

        processRoutine = null;
    }

    private void EnsureLineListSize()
    {
        var target = queueSlots != null ? queueSlots.Length : 0;
        if (target <= 0)
            return;

        while (linedPrisoners.Count < target)
            linedPrisoners.Add(null);
        while (linedPrisoners.Count > target)
            linedPrisoners.RemoveAt(linedPrisoners.Count - 1);
    }

    private void FillBackUntilFull()
    {
        if (queueSlots == null || queueSlots.Length == 0)
            return;
        if (PrisonerPooling.instance == null)
            return;

        EnsureLineListSize();

        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (linedPrisoners[i] != null)
                continue;

            var slot = queueSlots[i];
            if (slot == null)
                continue;

            // Spawn directly at slot position to guarantee a clean line.
            var go = PrisonerPooling.instance.Get(slot.position, slot.rotation);
            linedPrisoners[i] = go;
        }
    }

    private void ShiftForward()
    {
        if (queueSlots == null || queueSlots.Length == 0)
            return;

        EnsureLineListSize();

        for (int i = 0; i < queueSlots.Length - 1; i++)
        {
            if (linedPrisoners[i] != null)
                continue;

            // Pull the next prisoner forward.
            int next = i + 1;
            var go = linedPrisoners[next];
            if (go == null)
                continue;

            linedPrisoners[i] = go;
            linedPrisoners[next] = null;

            var slot = queueSlots[i];
            if (slot != null)
            {
                go.transform.DOKill();
                go.transform.DOMove(slot.position, slotMoveDuration).SetEase(slotMoveEase);
                go.transform.DORotateQuaternion(slot.rotation, slotMoveDuration).SetEase(slotMoveEase);
            }
        }
    }
}

