using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;
using Newtonsoft.Json;

public class Building : WorldObject {
    public AudioClip finishedJobSound;
    public float finishedJobVolume = 1.0f;
    public float maxBuildProgress;
    public override bool IsActive { get { return !needsBuilding; } }
    public Texture2D rallyPointImage;
    public Texture2D sellImage;

    private bool needsBuilding = false;
    private float currentBuildProgress = 0.0f;
    private Vector3 spawnPoint;

    protected Queue<string> buildQueue;
    protected Vector3 rallyPoint;

    protected override void Awake()
    {
        base.Awake();
        buildQueue = new Queue<string>();
        SetSpawnPoint();
    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void Update()
    {
        base.Update();
        ProcessBuildQueue();
    }
    protected override void OnGUI()
    {
        base.OnGUI();
        if (needsBuilding) DrawBuildProgress();
    }
    protected void CreateUnit(string unitName)
    {
        GameObject unit = ResourceManager.GetUnit(unitName);
        Unit unitObject = unit.GetComponent<Unit>();
        if (player && unitObject)
        {
            player.RemoveResource(ResourceType.Stone, unitObject.stoneCost);
            player.RemoveResource(ResourceType.Food, unitObject.foodCost);
            player.RemoveResource(ResourceType.Metal, unitObject.metalCost);
            player.RemoveResource(ResourceType.Money, unitObject.moneyCost);
        }

        buildQueue.Enqueue(unitName);
    }
    protected void ProcessBuildQueue()
    {
        if (buildQueue.Count > 0)
        {
            currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
            if (currentBuildProgress > maxBuildProgress)
            {
                if (player)
                {
                    if (audioElement != null) audioElement.Play(finishedJobSound);
                    player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
                }
                currentBuildProgress = 0.0f;
            }
        }
    }
    public string[] getBuildQueueValues()
    {
        string[] values = new string[buildQueue.Count];
        int pos = 0;
        foreach (string unit in buildQueue) values[pos++] = unit;
        return values;
    }
    public float getBuildPercentage()
    {
        return currentBuildProgress / maxBuildProgress;
    }
    public bool hasSpawnPoint()
    {
        return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
    }
    public override void SetSelection(bool selected, Rect playingArea)
    {
        base.SetSelection(selected, playingArea);
        if (player)
        {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
            if (selected)
            {
                if (flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition)
                {
                    flag.transform.localPosition = rallyPoint;
                    flag.transform.forward = transform.forward;
                    flag.Enable();
                }
            }
            else
            {
                if (flag && player.human) flag.Disable();
            }
        }
    }
    public override void SetHoverState(GameObject hoverObject)
    {
        base.SetHoverState(hoverObject);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected)
        {
            if (WorkManager.ObjectIsGround(hoverObject))
            {
                if (player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
            }
        }
    }
    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {
        base.MouseClick(hitObject, hitPoint, controller);
        //only handle iput if owned by a human player and currently selected
        if (player && player.human && currentlySelected)
        {
            if (WorkManager.ObjectIsGround(hitObject))
            {
                if ((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition)
                {
                    SetRallyPoint(hitPoint);
                }
            }
        }
    }
    public override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);
        SaveManager.WriteBoolean(writer, "NeedsBuilding", needsBuilding);
        SaveManager.WriteVector(writer, "SpawnPoint", spawnPoint);
        SaveManager.WriteVector(writer, "RallyPoint", rallyPoint);
        SaveManager.WriteFloat(writer, "BuildProgress", currentBuildProgress);
        SaveManager.WriteStringArray(writer, "BuildQueue", buildQueue.ToArray());
        if (needsBuilding) SaveManager.WriteRect(writer, "PlayingArea", playingArea);
    }
    public void SetRallyPoint(Vector3 position)
    {
        rallyPoint = position;
        if (player && player.human && currentlySelected)
        {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
            if (flag) flag.transform.localPosition = rallyPoint;
        }
    }
    public void Sell()
    {
        if (player)
        {
            player.AddResource(ResourceType.Stone, stoneSellValue);
            player.AddResource(ResourceType.Food, foodSellValue);
            player.AddResource(ResourceType.Metal, metalSellValue);
            player.AddResource(ResourceType.Money, moneySellValue);
        }

        if (currentlySelected) SetSelection(false, playingArea);
        Destroy(this.gameObject);
    }
    public void StartConstruction()
    {
        CalculateBounds();
        needsBuilding = true;
        hitPoints = 0;
    }
    public void Construct(int amount)
    {
        hitPoints += amount;
        if (hitPoints >= maxHitPoints)
        {
            hitPoints = maxHitPoints;
            needsBuilding = false;
            RestoreMaterials();
            SetSpawnPoint();
            SetTeamColor();
        }
    }

    public static List<string> LoadStringArray(JsonTextReader reader)
    {
        List<string> values = new List<string>();
        while (reader.Read())
        {
            if (reader.Value != null)
            {
                values.Add((string)reader.Value);
            }
            else if (reader.TokenType == JsonToken.EndArray) return values;
        }
        return values;
    }
    private void DrawBuildProgress()
    {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
        //Draw the selection box around the currently selected object, within the bounds of the main draw area
        GUI.BeginGroup(playingArea);
        CalculateCurrentHealth(0.5f, 0.99f);
        DrawHealthBar(selectBox, "Building ...");
        GUI.EndGroup();
    }
    private void SetSpawnPoint()
    {

        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
        rallyPoint = spawnPoint;

    }
    public bool UnderConstruction()
    {
        return needsBuilding;
    }
    
    protected override void HandleLoadedProperty(JsonTextReader reader, string propertyName, object readValue)
    {
        base.HandleLoadedProperty(reader, propertyName, readValue);
        switch (propertyName)
        {
            case "NeedsBuilding": needsBuilding = (bool)readValue; break;
            case "SpawnPoint": spawnPoint = LoadManager.LoadVector(reader); break;
            case "RallyPoint": rallyPoint = LoadManager.LoadVector(reader); break;
            case "BuildProgress": currentBuildProgress = (float)(double)readValue; break;
            case "BuildQueue": buildQueue = new Queue<string>(LoadManager.LoadStringArray(reader)); break;
            case "PlayingArea": playingArea = LoadManager.LoadRect(reader); break;
            default: break;
        }
    }
    protected override void InitialiseAudio()
    {
        base.InitialiseAudio();
        if (finishedJobVolume < 0.0f) finishedJobVolume = 0.0f;
        if (finishedJobVolume > 1.0f) finishedJobVolume = 1.0f;
        List<AudioClip> sounds = new List<AudioClip>();
        List<float> volumes = new List<float>();
        sounds.Add(finishedJobSound);
        volumes.Add(finishedJobVolume);
        audioElement.Add(sounds, volumes);
    }
}
