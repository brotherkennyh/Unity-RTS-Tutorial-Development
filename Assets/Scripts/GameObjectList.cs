using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class GameObjectList : MonoBehaviour {
    public GameObject[] buildings;
    public GameObject[] units;
    public GameObject[] worldObjects;
    public GameObject player;
    public Texture2D[] avatars;
    private static bool created = false;

    // Use this for initialization
    void Start () {
		
	}
    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(transform.gameObject);
            ResourceManager.SetGameObjectList(this);
            created = true;
            PlayerManager.Load();
            PlayerManager.SetAvatarTextures(avatars);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
    public GameObject GetBuilding(string name)
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name) return buildings[i];
        }
        return null;
    }
    public GameObject GetUnit(string name)
    {
        for (int i = 0; i < units.Length; i++)
        {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit && unit.name == name) return units[i];
        }
        return null;
    }
    public GameObject GetWorldObject(string name)
    {
        foreach (GameObject worldObject in worldObjects)
        {
            if (worldObject.name == name) return worldObject;
        }
        return null;
    }
    public GameObject GetPlayerObject()
    {
        return player;
    }
    public List<int> GetBuildCost(string name)
    {
                List<int> list = new List<int>();
        for (int i = 0; i < buildings.Length; i++)
        {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name)
            {
                list.Add(building.stoneCost);
                list.Add(building.metalCost);
                list.Add(building.foodCost);
                list.Add(building.moneyCost);
                return list;
            }        
        }
        for (int i = 0; i < units.Length; i++)
        {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit && unit.name == name)
            {
                list.Add(unit.stoneCost);
                list.Add(unit.metalCost);
                list.Add(unit.foodCost);
                list.Add(unit.moneyCost);
                return list;
            }
        }
        return null;
    }
    public Texture2D GetBuildImage(string name)
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name) return building.buildImage;
        }
        for (int i = 0; i < units.Length; i++)
        {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit && unit.name == name) return unit.buildImage;
        }
        return null;
    }
    public Texture2D[] GetAvatars()
    {
        return avatars;
    }
}
