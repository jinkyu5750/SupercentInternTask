using UnityEngine;

public sealed class UnlockZone : MonoBehaviour
{
    public enum UnlockType
    {
        ToolUpgrade,
        SpawnWorker,
        SpawnOfficer,
        ExpandPrison,
    }

    [Header("Unlock")]
    [SerializeField] private UnlockType unlockType;
    [SerializeField] private int cost = 50;
    [SerializeField] private float holdSeconds = 1.25f;
    [SerializeField] private bool unlocked;

    [Header("Spawn (optional)")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform spawnPoint;

    [Header("Prerequisite (optional)")]
    [SerializeField] private UnlockZone prerequisite;

    private float holdTimer;

    private void OnTriggerStay(Collider other)
    {
        if (unlocked)
            return;

        var player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        if (prerequisite != null && !prerequisite.unlocked)
        {
            holdTimer = 0f;
            return;
        }

        if (player.Money < cost)
        {
            holdTimer = 0f;
            return;
        }

        holdTimer += Time.deltaTime;
        if (holdTimer < holdSeconds)
            return;

        if (!player.TrySpendMoney(cost))
        {
            holdTimer = 0f;
            return;
        }

        Unlock(player);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Player>() != null)
            holdTimer = 0f;
    }

    private void Unlock(Player player)
    {
        unlocked = true;

        if (prefabToSpawn != null)
        {
            var pos = spawnPoint != null ? spawnPoint.position : transform.position;
            var rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            Instantiate(prefabToSpawn, pos, rot);
        }

        // Skeleton: hook into your upgrade system (e.g., Player tool level, prison size, etc.)
        switch (unlockType)
        {
            case UnlockType.ToolUpgrade:
                player.UpgradeTool();
                break;
        }
    }
}

