using DG.Tweening;
using UnityEngine;

public sealed class Prisoner : MonoBehaviour
{
    [Header("Demand")]
    [SerializeField] private int minRequiredHandcuffs = 1;
    [SerializeField] private int maxRequiredHandcuffs = 6;
    [SerializeField] private int requiredHandcuffs;

    [Header("Reward")]
    [SerializeField] private int averageRewardMoney = 25;
    [SerializeField] private int rewardMoney;
    [SerializeField] private GameObject moneyPrefab;
    public int RequiredHandcuffs => requiredHandcuffs;
    public int RewardMoney => rewardMoney;
    private Collider moneyZone;

    private bool isImprisoned;

    private void Awake()
    {
        if (requiredHandcuffs <= 0)
            requiredHandcuffs = Random.Range(minRequiredHandcuffs, maxRequiredHandcuffs + 1);

        if (rewardMoney <= 0)
            rewardMoney = Random.Range(averageRewardMoney - 5, averageRewardMoney + 6);
        moneyZone = GameObject.Find("MoneyZone").GetComponent<BoxCollider>();
    }

    public void Imprison(Transform prisonTarget)
    {
        if (isImprisoned)
            return;

        isImprisoned = true;
        GetComponent<Renderer>().material.color = new Color32(255, 133, 0, 255);
        for (int i = 0; i < 6; i++) // 2,3 Çà·Ä·Î ½×±â
        {
            int row = i / 3; 
            int column = i %3; 
      
            Vector3 pos =Vector3.zero;
            pos.x += (row * 0.45f) - 0.2f;
            pos.y += 0.5f + (moneyZone.transform.childCount/6 )*0.3f;
            pos.z += (column * 0.25f) - 0.25f; 

            Vector3 scale = new Vector3(0.4f, 0.4f, 1f);

            var money = Instantiate(moneyPrefab, transform.position, Quaternion.identity);

            money.transform.SetParent(moneyZone.transform);
            money.transform.DOLocalMove(pos, 0.3f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                money.transform.DOScale(scale * 1.1f, 0.1f).OnComplete(() =>
                money.transform.DOScale(scale * 1f, 0.2f));
            });
            money.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
        }

        // Skeleton: replace with NavMeshAgent / tween / animation as needed.
        if (prisonTarget != null)
        {
            transform.DOMove(new Vector3(1.5f,2f,-7.5f),3f).OnComplete(
                ()=>transform.DOMove(prisonTarget.position,2f).OnComplete(
                    ()=>transform.SetParent(prisonTarget.transform)));


            moneyZone.GetComponent<MoneyZone>().SetMoney(rewardMoney);
        }
        else
            gameObject.SetActive(false);
    }
}

