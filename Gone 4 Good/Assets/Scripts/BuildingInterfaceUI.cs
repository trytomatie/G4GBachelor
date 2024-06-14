using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInterfaceUI : MonoBehaviour
{
    public GameObject basicPanel;
    public GameObject productionPanel;
    public GameObject magicPanel;
    public GameObject farmingPanel;

    [Header("Prefabs")]
    public GameObject buildingObjectSelectionButtonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        SetUpButtons();
    }

    private void SetUpButtons()
    {
        int i = 0;
        foreach (GameObject building in BuildingManager.instance.buildingPrefabs)
        {
            BuildingObject buildingObject = building.GetComponent<BuildingObject>();
            GameObject newButton = Instantiate(buildingObjectSelectionButtonPrefab, basicPanel.transform);
            newButton.GetComponent<BuildingInterfaceButtonUI>().SetUp(i);
            i++;
        }
    }
}
