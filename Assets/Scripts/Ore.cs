using System.Collections;
using UnityEngine;

public sealed class Ore : MonoBehaviour
{
    [SerializeField] private float respawnSeconds = 5f;
    [SerializeField] private GameObject visual;
    [SerializeField] private Collider oreCollider;

    public bool IsAvailable => isAvailable;

    private bool isAvailable = true;
    private Coroutine respawnRoutine;

    private void Reset()
    {
        visual = gameObject;
        oreCollider = GetComponent<Collider>();
    }

    public bool TryMineOne()
    {
        if (!isAvailable)
            return false;

        Deplete();
        return true;
    }

    private void Deplete()
    {
        isAvailable = false;

        if (visual != null)
            visual.SetActive(false);
        if (oreCollider != null)
            oreCollider.enabled = false;

        if (respawnRoutine != null)
            StopCoroutine(respawnRoutine);
        respawnRoutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnSeconds);
        Respawn();
    }

    private void Respawn()
    {
        isAvailable = true;

        if (visual != null)
            visual.SetActive(true);
        if (oreCollider != null)
            oreCollider.enabled = true;
    }
}

