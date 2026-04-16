using System.Collections;
using UnityEngine;

public sealed class Ore : MonoBehaviour
{

    public bool IsAvailable => isAvailable;

    private bool isAvailable = true;


    private void OnEnable()
    {
        isAvailable = true; // 풀에서 다시 꺼낼 때 초기화
    }
    public bool TryMineOne()
    {
        if (!isAvailable)
            return false;

       OrePooling.instance.DespawnAndScheduleRespawn(gameObject);
        return true;
    }


   

}

