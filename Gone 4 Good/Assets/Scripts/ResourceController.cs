using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    public GameObject[] visuals;
    public ItemBlueprint itemDrop;
    public Vector2Int dropAmountRange = new Vector2Int(1,1);
    public ShaderController shaderController;
    private ResourceStatusManager rsm;

    public void SetVisual()
    {
        int index = Random.Range(0, visuals.Length);
        foreach (var visual in visuals)
        {
            visual.SetActive(false);
        }
        visuals[index].SetActive(true);
        visuals[index].transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        shaderController.TargetRenderer = visuals[index].GetComponentInChildren<Renderer>();
         shaderController.TargetMaterial = visuals[index].GetComponentInChildren<Renderer>().material;
        shaderController.enabled = true;

    }
    public void DropItem()
    {
        int dropAmount = Random.Range(dropAmountRange.x,dropAmountRange.y+1);
        for (int i = 0; i < dropAmount; i++)
        {
            GameObject go = Instantiate(GameManager.Instance.droppedItem, transform.position + new Vector3(0,0.3f,0), Quaternion.identity);
            go.GetComponent<DroppedItem>().SetUpDroppedItem(itemDrop, 1);
        }
    }

    private void Start()
    {
        rsm = GetComponent<ResourceStatusManager>();
        GetComponent<ResourceStatusManager>().OnDeath.AddListener(AddExperience);
    }

    public void AddExperience()
    {
        GameManager.Instance.player.GetComponent<PlayerExp>().AddExperience(transform.position,rsm.experienceDrop);
    }

}
