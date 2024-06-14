using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    public enum BuildingType { Basic, Production, Magic, Farming }
    public BuildingType buildingType;
    public string buildingName;
    public Sprite buildingImage;
    public ItemBlueprint[] buildingCost;
    public int[] buildingCostAmount;


}
