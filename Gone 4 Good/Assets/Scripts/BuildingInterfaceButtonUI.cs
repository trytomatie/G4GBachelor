using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildingInterfaceButtonUI : MonoBehaviour
{
    public TextMeshProUGUI buildingName;
    public Image buildingImage;
    private int buildingIndex;
    public void SetUp(int index)
    {
        BuildingObject bo = BuildingManager.instance.buildingPrefabs[index].GetComponent<BuildingObject>();
        buildingName.text = bo.buildingName;
        buildingImage.sprite = bo.buildingImage;
        buildingIndex = index;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        BuildingManager.instance.SetBuildingIndicator(buildingIndex);
        BuildingManager.instance.PlaceBuildingMode = true;
    }
}
