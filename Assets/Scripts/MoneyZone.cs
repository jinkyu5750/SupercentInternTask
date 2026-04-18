using UnityEngine;

public class MoneyZone : MonoBehaviour
{

    [SerializeField] private int money = 0;
    [SerializeField] Collider player;

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other != player)
            return;

        var _player = other.GetComponentInParent<Player>();
        if (_player != null)
        {
            _player.AddMoney(money);
            money = 0;
            StartCoroutine(_player.MoveItemToPlayer(transform,false));
        }
    }


    public void SetMoney(int m)
    {
        money += m;
    }
}
