using UnityEngine;

[CreateAssetMenu(menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public int woodCost;
    public int stoneCost;
    public int goldCost;
    public BuildingData upgradeTo;
} 