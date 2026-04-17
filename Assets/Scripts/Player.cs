using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public sealed class Player : MonoBehaviour
{
    public enum MiningTool
    {
        Pickaxe = 1,   // mines up to 1 ore per tick
        Drill = 3,     // mines up to 3 ores per tick
        BigDrill = 5,  // mines up to 5 ores per tick
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationLerpSpeed = 12f;

    [Header("Drag Joystick (screen space)")]
    [Tooltip("Drag distance (pixels) that maps to full input magnitude (1.0).")]
    [SerializeField] private float joystickRadiusPixels = 120f;
    [Tooltip("Ignore tiny drags under this pixel distance.")]
    [SerializeField] private float deadZonePixels = 8f;

    [Header("Carry (no inventory storage)")]
    [SerializeField] private int carryOreMax = 20;
    [SerializeField] private int carryHandcuffMax = 20;

    [Header("Economy")]
    [SerializeField] private int money = 0;

    [Header("Mining")]
    [SerializeField] private GameObject miningTool;
    [SerializeField] private GameObject[] miningToolList;
    [SerializeField] private int miningToolLv=0;
    
    [SerializeField] private GameObject carriedOrePos;
    [SerializeField] private GameObject carriedHandcuffPos;
    [SerializeField] private GameObject carriedOrePrefab;
    int pickaxeRemaining;
    public int Money => money;
    public int CarriedOre => carriedOre;
    public int CarriedHandcuffs => carriedHandcuffs;
    public int CarryOreMax => carryOreMax;
    public int CarryHandcuffMax => carryHandcuffMax;


    [SerializeField] private int carriedOre;
    [SerializeField] private int carriedHandcuffs;

    private Camera cam;

    private bool pointerDown;
    private Vector2 pointerStartScreen;
    private Vector2 pointerNowScreen;
    private float mineTimer;


    private Animator ani;

    private void Start()
    {
        ani = GetComponent<Animator>();
        cam = Camera.main;
        cam.transform.position = transform.position + new Vector3(0, 0, -10);
        cam.transform.position = new Vector3(cam.transform.position.x, 8f, cam.transform.position.z);
    }
    private void Update()
    {
        UpdatePointer();
        MoveByDragJoystick();

    }

    private void UpdatePointer()
    {


        if (Input.GetMouseButtonDown(0))
        {
            pointerDown = true;
            pointerStartScreen = Input.mousePosition;
            pointerNowScreen = pointerStartScreen;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            pointerDown = false;
        }
        else if (pointerDown)
        {
            pointerNowScreen = Input.mousePosition;
        }
    }

    private void MoveByDragJoystick()
    {
        if (!pointerDown)
            return;

        var delta = pointerNowScreen - pointerStartScreen;
        var mag = delta.magnitude;
        if (mag < deadZonePixels)
            return;

        // Joystick input in [-1..1] with clamp at radius.
        var clamped = Vector2.ClampMagnitude(delta, joystickRadiusPixels);
        var input = clamped / Mathf.Max(1f, joystickRadiusPixels);


        // Convert screen joystick to world direction using camera axes on XZ plane.
        var camForward = cam.transform.forward;
        var camRight = cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        var moveDir = camRight * input.x + camForward * input.y;
        if (moveDir.sqrMagnitude < 0.0001f)
            return;

        moveDir.Normalize();
        var speedScale = Mathf.Clamp01(input.magnitude);
        transform.position += moveDir * (moveSpeed * speedScale) * Time.deltaTime;
        cam.transform.position = transform.position + new Vector3(0, 0, -10);
        cam.transform.position = new Vector3(cam.transform.position.x, 8f, cam.transform.position.z);

        var targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);
    }


    public void AddMoney(int amount)
    {
        money += Mathf.Max(0, amount);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
            return true;
        if (money < amount)
            return false;
        money -= amount;
        return true;
    }

    public GameObject TakeAllCarriedOre()
    {
        var taken = carriedOrePos;
        carriedOre = 0;
        return taken;
    }


    public GameObject TakeAllCarriedHandcuffs()
    {
        var taken = carriedHandcuffPos;
        carriedHandcuffs = 0;
        return taken;
    }

    public void AddCarriedHandcuffs(int amount)
    {
        if (amount <= 0)
            return;

        var canAdd = Mathf.Min(amount, carryHandcuffMax - carriedHandcuffs);
        if (canAdd <= 0)
            return;

        carriedHandcuffs += canAdd;

    }

    public void AddCarriedOre(int amount)
    {
        if (amount <= 0)
            return;

        var canAdd = Mathf.Min(amount, carryOreMax - carriedOre);
        if (canAdd <= 0)
            return;

        carriedOre += canAdd;

        var ore = Instantiate(carriedOrePrefab, carriedOrePos.transform);
        Vector3 scale = ore.transform.localScale;
        ore.transform.localPosition = new Vector3(0, 0, carriedOre * 0.2f);
        ore.transform.localRotation = Quaternion.Euler(Vector3.zero);
        ore.transform.DOScale(scale* 2.5f, 0.1f).OnComplete(() => ore.transform.DOScale(scale * 1f, 0.2f));
    }

    public void UpgradeTool()
    {
        if (miningToolLv > miningToolList.Length - 1)
            return;

      //  miningTool.SetActive(false); 이건 필요없을듯
        miningToolLv++;
        miningTool = miningToolList[miningToolLv];


    }
    public void OnAxeCol()
    {
        pickaxeRemaining = 1;
        miningTool.GetComponent<CapsuleCollider>().enabled = true;
    }
    public void OffAxeCol()
    {
        miningTool.GetComponent<CapsuleCollider>().enabled = false;
    }
    public IEnumerator MoveHandcuffToPlayer(Transform zone)
    {
        var handCuffNum = zone.transform.childCount;
        if (handCuffNum <= 0) yield break;

        Vector3 scale = zone.transform.GetChild(0).localScale;

        for (int i = handCuffNum - 1; i >= 0; i--)
        {

            Transform handcuff = zone.transform.GetChild(i);
            handcuff.SetParent(carriedHandcuffPos.transform);
            handcuff.DOLocalMove(new Vector3(0,carriedHandcuffPos.transform.childCount * 0.1f,0), 0.3f).SetEase(Ease.OutCubic).OnComplete(() => handcuff.DOScale(scale * 1.5f, 0.1f).OnComplete(() => handcuff.DOScale(scale * 1f, 0.2f)));
            handcuff.localRotation = Quaternion.Euler(Vector3.zero);

            //  ore.transform.localRotation = Quaternion.Euler(-90f, 0, 0);

            yield return new WaitForSeconds(0.05f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("MiningZone"))
        {
            miningTool.SetActive(true);
            ani.SetInteger("Mining", miningToolLv);
        }

        if (other.tag.Equals("Ore"))
        {

           
            if (miningToolLv == 0 && pickaxeRemaining == 1)
            {
                pickaxeRemaining--;
                other.GetComponent<Ore>().TryMineOne();
                AddCarriedOre(1);
            }
            else if(miningToolLv>=1)
            {
                other.GetComponent<Ore>().TryMineOne();
                AddCarriedOre(1);
            }


        }

        if (other.name.Equals("HandcuffWithdrawZone"))
        {
            AddCarriedHandcuffs(other.transform.parent.GetComponent<HandcuffsMaker>().WithdrawHandcuffs()); // 수갑양 ++
            StartCoroutine(MoveHandcuffToPlayer(other.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("MiningZone"))
        {
            miningTool.SetActive(false);
            ani.SetInteger("Mining", -1);
        }
    }
}

