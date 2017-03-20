using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;
using Newtonsoft.Json;

public class Player : MonoBehaviour {

    public string username;
    public bool human;
    public HUD hud;
    public WorldObject SelectedObject { get; set; }
    public int startStone, startStoneLimit, startMetal, startMetalLimit, startFood, startFoodLimit, startMoney, startMoneyLimit;
    public Material notAllowedMaterial, allowedMaterial;
    public Color teamColor;

    private Dictionary<ResourceType, int> resources, resourceLimits;
    private Building tempBuilding;
    private Unit tempCreator;
    private bool findingPlacement = false;

    // Use this for initialization
    void Start () {
        hud = GetComponentInChildren<HUD>();
        AddStartResourceLimits();
        AddStartResources();
    }
    void Awake()
    {
        resources = InitResourceList();
        resourceLimits = InitResourceList();
    }
    // Update is called once per frame
    void Update () {
        if (human)
        {
            hud.SetResourceValues(resources, resourceLimits);
        }
        if (findingPlacement)
        {
            tempBuilding.CalculateBounds();
            if (CanPlaceBuilding()) tempBuilding.SetTransparentMaterial(allowedMaterial, false);
            else tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
        }
    }
    private Dictionary<ResourceType, int> InitResourceList()
    {
        Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
        list.Add(ResourceType.Stone, 0);
        list.Add(ResourceType.Metal, 0);
        list.Add(ResourceType.Food, 0);
        list.Add(ResourceType.Money, 0);
        return list;
    }
    private void AddStartResourceLimits()
    {
        IncrementResourceLimit(ResourceType.Stone, startStoneLimit);
        IncrementResourceLimit(ResourceType.Metal, startMetalLimit);
        IncrementResourceLimit(ResourceType.Food, startFoodLimit);
        IncrementResourceLimit(ResourceType.Money, startMoneyLimit);


    }
    private void AddStartResources()
    {
        AddResource(ResourceType.Stone, startStone);
        AddResource(ResourceType.Metal, startMetal);
        AddResource(ResourceType.Food, startFood);
        AddResource(ResourceType.Money, startMoney);
    }
    private void LoadResources(JsonTextReader reader)
    {
        if (reader == null) return;
        string currValue = "";
        while (reader.Read())
        {
            if (reader.Value != null)
            {
                if (reader.TokenType == JsonToken.PropertyName) currValue = (string)reader.Value;
                else
                {
                    switch (currValue)
                    {
                        case "Stone": startStone = (int)(System.Int64)reader.Value; break;
                        case "Stone_Limit": startStoneLimit = (int)(System.Int64)reader.Value; break;
                        case "Food": startFood = (int)(System.Int64)reader.Value; break;
                        case "Food_Limit": startFoodLimit = (int)(System.Int64)reader.Value; break;
                        case "Metal": startMetal = (int)(System.Int64)reader.Value; break;
                        case "Metal_Limit": startMetalLimit = (int)(System.Int64)reader.Value; break;
                        case "Money": startMetal = (int)(System.Int64)reader.Value; break;
                        case "Money_Limit": startMetalLimit = (int)(System.Int64)reader.Value; break;
                        default: break;
                    }
                }
            }
            else if (reader.TokenType == JsonToken.EndArray)
            {
                return;
            }
        }
    }
    private void LoadBuildings(JsonTextReader reader)
    {
        if (reader == null) return;
        Buildings buildings = GetComponentInChildren<Buildings>();
        string currValue = "", type = "";
        while (reader.Read())
        {
            if (reader.Value != null)
            {
                if (reader.TokenType == JsonToken.PropertyName) currValue = (string)reader.Value;
                else if (currValue == "Type")
                {
                    type = (string)reader.Value;
                    GameObject newObject = (GameObject)GameObject.Instantiate(ResourceManager.GetBuilding(type));
                    Building building = newObject.GetComponent<Building>();
                    building.LoadDetails(reader);
                    building.transform.parent = buildings.transform;
                    building.SetPlayer();
                    building.SetTeamColor();
                    if (building.UnderConstruction())
                    {
                        building.SetTransparentMaterial(allowedMaterial, true);
                    }
                }
            }
            else if (reader.TokenType == JsonToken.EndArray) return;
        }
    }
    private void LoadUnits(JsonTextReader reader)
    {
        if (reader == null) return;
        Units units = GetComponentInChildren<Units>();
        string currValue = "", type = "";
        while (reader.Read())
        {
            if (reader.Value != null)
            {
                if (reader.TokenType == JsonToken.PropertyName) currValue = (string)reader.Value;
                else if (currValue == "Type")
                {
                    type = (string)reader.Value;
                    GameObject newObject = (GameObject)GameObject.Instantiate(ResourceManager.GetUnit(type));
                    Unit unit = newObject.GetComponent<Unit>();
                    unit.LoadDetails(reader);
                    unit.transform.parent = units.transform;
                    unit.SetPlayer();
                    unit.SetTeamColor();
                }
            }
            else if (reader.TokenType == JsonToken.EndArray) return;
        }
    }

    public bool CanPlaceBuilding()
    {
        bool canPlace = true;

        Bounds placeBounds = tempBuilding.GetSelectionBounds();
        //shorthand for the coordinates of the center of the selection bounds
        float cx = placeBounds.center.x;
        float cy = placeBounds.center.y;
        float cz = placeBounds.center.z;
        //shorthand for the coordinates of the extents of the selection box
        float ex = placeBounds.extents.x;
        float ey = placeBounds.extents.y;
        float ez = placeBounds.extents.z;

        //Determine the screen coordinates for the corners of the selection bounds
        List<Vector3> corners = new List<Vector3>();
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)));
        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz - ez)));

        foreach (Vector3 corner in corners)
        {
            GameObject hitObject = WorkManager.FindHitObject(corner);
            if (hitObject && !WorkManager.ObjectIsGround(hitObject))
            {
                WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
                if (worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds())) canPlace = false;
            }
        }
        return canPlace;
    }
    public bool IsDead()
    {
        Building[] buildings = GetComponentsInChildren<Building>();
        Unit[] units = GetComponentsInChildren<Unit>();
        if (buildings != null && buildings.Length > 0) return false;
        if (units != null && units.Length > 0) return false;
        return true;
    }
    public int GetResourceAmount(ResourceType type)
    {
        return resources[type];
    }
    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
    }
    public void IncrementResourceLimit(ResourceType type, int amount)
    {
        resourceLimits[type] += amount;
    }
    public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, Building creator)
    {
        Units units = GetComponentInChildren<Units>();

        GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
        newUnit.transform.parent = units.transform;
        Unit unitObject = newUnit.GetComponent<Unit>();
        if (unitObject && spawnPoint != rallyPoint) unitObject.StartMove(rallyPoint);
        if (unitObject)
        {
            unitObject.SetBuilding(creator);
            unitObject.ObjectId = ResourceManager.GetNewObjectId();
            if (spawnPoint != rallyPoint) unitObject.StartMove(rallyPoint);
        }
    }
    public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea)
    {
        GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());

        tempBuilding = newBuilding.GetComponent<Building>();
        if (tempBuilding)
        {
            tempCreator = creator;
            tempBuilding.ObjectId = ResourceManager.GetNewObjectId();
            findingPlacement = true;
            tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
            tempBuilding.SetColliders(false);
            tempBuilding.SetPlayingArea(playingArea);
            tempBuilding.hitPoints = 0;
        }
        else Destroy(newBuilding);
    }
    public bool IsFindingBuildingLocation()
    {
        return findingPlacement;
    }
    public void FindBuildingLocation()
    {
        Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
        newLocation.y = 0;
        tempBuilding.transform.position = newLocation;
    }
    public void StartConstruction()
    {
        findingPlacement = false;
        Buildings buildings = GetComponentInChildren<Buildings>();
        if (buildings) tempBuilding.transform.parent = buildings.transform;
        tempBuilding.SetPlayer();
        tempBuilding.SetColliders(true);
        tempCreator.SetBuilding(tempBuilding);
        tempBuilding.StartConstruction();
        RemoveResource(ResourceType.Stone, tempBuilding.stoneCost);
        RemoveResource(ResourceType.Metal, tempBuilding.metalCost);
        RemoveResource(ResourceType.Food, tempBuilding.foodCost);
        RemoveResource(ResourceType.Money, tempBuilding.moneyCost);
    }
    public void CancelBuildingPlacement()
    {
        findingPlacement = false;
        Destroy(tempBuilding.gameObject);
        tempBuilding = null;
        tempCreator = null;
    }
    public void LoadDetails(JsonTextReader reader)
    {
        if (reader == null) return;
        string currValue = "";
        while (reader.Read())
        {
            if (reader.Value != null)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    currValue = (string)reader.Value;
                }
                else
                {
                    switch (currValue)
                    {
                        case "Username": username = (string)reader.Value; break;
                        case "Human": human = (bool)reader.Value; break;
                        default: break;
                    }
                }
            }
            else if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
            {
                switch (currValue)
                {
                    case "TeamColor": teamColor = LoadManager.LoadColor(reader); break;
                    case "Resources": LoadResources(reader); break;
                    case "Buildings": LoadBuildings(reader); break;
                    case "Units": LoadUnits(reader); break;
                    default: break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject) return;
        }
    }
    public void RemoveResource(ResourceType type, int amount)
    {
        resources[type] -= amount;
    }
    public virtual void SaveDetails(JsonWriter writer)
    {
        SaveManager.WriteString(writer, "Username", username);
        SaveManager.WriteBoolean(writer, "Human", human);
        SaveManager.WriteColor(writer, "TeamColor", teamColor);
        SaveManager.SavePlayerResources(writer, resources, resourceLimits);
        SaveManager.SavePlayerBuildings(writer, GetComponentsInChildren<Building>());
        SaveManager.SavePlayerUnits(writer, GetComponentsInChildren<Unit>());
    }
    public WorldObject GetObjectForId(int id)
    {
        WorldObject[] objects = GameObject.FindObjectsOfType(typeof(WorldObject)) as WorldObject[];
        foreach (WorldObject obj in objects)
        {
            if (obj.ObjectId == id) return obj;
        }
        return null;
    }

}
