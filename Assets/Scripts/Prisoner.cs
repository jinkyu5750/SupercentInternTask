using UnityEngine;

public sealed class Prisoner : MonoBehaviour
{
    [Header("Demand")]
    [SerializeField] private int minRequiredHandcuffs = 1;
    [SerializeField] private int maxRequiredHandcuffs = 6;
    [SerializeField] private int requiredHandcuffs;

    [Header("Reward")]
    [SerializeField] private int rewardMoney = 5;

    public int RequiredHandcuffs => requiredHandcuffs;
    public int RewardMoney => rewardMoney;

    private bool isImprisoned;

    private void Awake()
    {
        if (requiredHandcuffs <= 0)
            requiredHandcuffs = Random.Range(minRequiredHandcuffs, maxRequiredHandcuffs + 1);
    }

    public void Imprison(Transform prisonTarget)
    {
        if (isImprisoned)
            return;

        isImprisoned = true;

        // Skeleton: replace with NavMeshAgent / tween / animation as needed.
        if (prisonTarget != null)
            transform.position = prisonTarget.position;
        else
            gameObject.SetActive(false);
    }
}

