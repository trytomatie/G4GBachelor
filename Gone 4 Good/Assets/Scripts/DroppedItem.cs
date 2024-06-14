using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    private Rigidbody rb;
    public float force;
    public ItemBlueprint droppedItem;
    public int amount = 1;
    public SpriteRenderer spriteRenderer;
    public TextMeshPro amountText;
    public bool isKeyItem = false;
    private Transform target;
    public GameObject keyItemVFX;

    private bool hitGround = false;
    private float spawnTime;
    private float pickUpLockout = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Random.onUnitSphere + new Vector3(0,force,0);
        target = GameManager.Instance.player.transform;
        spawnTime = Time.time;
    }

    public void SetUpDroppedItem(ItemBlueprint item, int amount)
    {
        spriteRenderer.sprite = item.itemIcon;
        droppedItem = item;
        this.amount = amount;
        isKeyItem = item.keyItem;
        keyItemVFX.SetActive(isKeyItem);
        amountText.text = "x" + amount.ToString();
    }

    private void Update()
    {
        if (Time.time < spawnTime + pickUpLockout) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 1.5f)
        {
            if (distance < 0.1f)
            {
                PickUpItem();
            }
            Vector3 dir =  target.position- transform.position;
            transform.position += dir.normalized * 7 * Time.deltaTime;
        }
    }

    private void PickUpItem()
    {
        if (isKeyItem)
        {
            Item item = new Item(droppedItem.id, amount);
            target.GetComponent<Inventory>().AddItem(item);
            GameUI.instance.SetUpGetItemScreen(item);
        }
        else
        {
            target.GetComponent<Inventory>().AddItem(new Item(droppedItem.id, amount));
        }
        AudioManager.PlayGeneralSound(transform.position, 1);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        hitGround = true;
        CombineNearStacks();
    }

    private void CombineNearStacks()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1);
        foreach (var hitCollider in hitColliders)
        {
            DroppedItem droppedItem = hitCollider.GetComponent<DroppedItem>();
            if (droppedItem != null && droppedItem.droppedItem == this.droppedItem && droppedItem.amount < droppedItem.droppedItem.maxStackSize)
            {
                int amountToCombine = Mathf.Min(droppedItem.droppedItem.maxStackSize - droppedItem.amount, this.amount);
                droppedItem.amount += amountToCombine;
                this.amount -= amountToCombine;
                if (this.amount == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    amountText.text = "x" + this.amount.ToString();
                }
                droppedItem.amountText.text = "x" + droppedItem.amount.ToString();
            }
        }
    }

}
