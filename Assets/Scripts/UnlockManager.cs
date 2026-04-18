using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] UnlockZone toolUpgradeZone;
    [SerializeField] UnlockZone WorkerUpgradeZone;

    private bool isUnlockedTool = false;
    private void Update()
    {
        if (player == null) return;
    
    
        if(player.Money > toolUpgradeZone.Cost && !isUnlockedTool)
        {
            toolUpgradeZone.transform.DOMoveY(1f, 1f);
            isUnlockedTool = true;
        }
    }
}
