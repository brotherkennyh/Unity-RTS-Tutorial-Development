﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;
using Newtonsoft.Json;

public class Harvester : Unit {

    public float capacity;
    public Building resourceStore;
    public float collectionAmount, depositAmount;
    public AudioClip emptyHarvestSound, harvestSound, startHarvestSound;
    public float emptyHarvestVolume = 0.5f, harvestVolume = 0.5f, startHarvestVolume = 1.0f;

    private bool harvesting = false, emptying = false;
    private float currentLoad = 0.0f;
    private int loadedDepositId = -1, loadedStoreId = -1;
    private ResourceType harvestType;
    private Resource resourceDeposit;
    private float currentDeposit = 0.0f;


    /*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Start()
    {
        base.Start();
        if (loadedSavedValues)
        {
            if (player)
            {
                if (loadedStoreId >= 0)
                {
                    WorldObject obj = player.GetObjectForId(loadedStoreId);
                    if (obj.GetType().IsSubclassOf(typeof(Building))) resourceStore = (Building)obj;
                }
                if (loadedDepositId >= 0)
                {
                    WorldObject obj = player.GetObjectForId(loadedDepositId);
                    if (obj.GetType().IsSubclassOf(typeof(Resource))) resourceDeposit = (Resource)obj;
                }
            }
        }
        else
        {
            harvestType = ResourceType.Unknown;
        }
    }
    protected override void Update()
    {
        base.Update();
        if (!rotating && !moving)
        {
            if (harvesting || emptying)
            {
                PickAxes[] pickaxes = GetComponentsInChildren<PickAxes>();
                //foreach (PickAxes pickaxe in pickaxes) pickaxe.renderer.enabled = true;
                foreach (PickAxes pickaxe in pickaxes) pickaxe.GetComponent<Renderer>().enabled = true;
                if (harvesting)
                {
                    Collect();
                    if (currentLoad >= capacity)
                        {
                        //make sure that we have a whole number to avoid bugs
                        //caused by floating point numbers
                        currentLoad = Mathf.Floor(currentLoad);
                        harvesting = false;
                        emptying = true;
                        //foreach (PickAxes pickaxe in pickaxes) pickaxe.renderer.enabled = false;
                        foreach(PickAxes pickaxe in pickaxes) pickaxe.GetComponent<Renderer>().enabled = false;
                        StartMove(resourceStore.transform.position, resourceStore.gameObject);
                    }
                }
                else
                {
                    Deposit();
                    if (currentLoad <= 0)
                    {
                        emptying = false;
                        //foreach (PickAxes pickaxe in pickaxes) pickaxe.renderer.enabled = false;
                        foreach (PickAxes pickaxe in pickaxes) pickaxe.GetComponent<Renderer>().enabled = false;
                        if (!resourceDeposit.isEmpty())
                        {
                            harvesting = true;
                            StartMove(resourceDeposit.transform.position, resourceDeposit.gameObject);
                        }
                    }
                }
            }
        }
    }

    /* Public Methods */

    public override void SetHoverState(GameObject hoverObject)
    {
        base.SetHoverState(hoverObject);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected)
        {
            if (!WorkManager.ObjectIsGround(hoverObject))
            {
                Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
                if (resource && !resource.isEmpty()) player.hud.SetCursorState(CursorState.Harvest);
            }
        }
    }
    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {
        base.MouseClick(hitObject, hitPoint, controller);
        //only handle input if owned by a human player
        if (player && player.human)
        {
            if (!WorkManager.ObjectIsGround(hitObject))
            {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();
                if (resource && !resource.isEmpty())
                {
                    //make sure that we select harvester remains selected
                    if (player.SelectedObject) player.SelectedObject.SetSelection(false, playingArea);
                    SetSelection(true, playingArea);
                    player.SelectedObject = this;
                    StartHarvest(resource);
                }
            }
            else StopHarvest();
        }
    }
    public override void SetBuilding(Building creator)
    {
        base.SetBuilding(creator);
        resourceStore = creator;
    }
    public override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);
        SaveManager.WriteBoolean(writer, "Harvesting", harvesting);
        SaveManager.WriteBoolean(writer, "Emptying", emptying);
        SaveManager.WriteFloat(writer, "CurrentLoad", currentLoad);
        SaveManager.WriteFloat(writer, "CurrentDeposit", currentDeposit);
        SaveManager.WriteString(writer, "HarvestType", harvestType.ToString());
        if (resourceDeposit) SaveManager.WriteInt(writer, "ResourceDepositId", resourceDeposit.ObjectId);
        if (resourceStore) SaveManager.WriteInt(writer, "ResourceStoreId", resourceStore.ObjectId);
    }

    /* Private Methods */

    private void StartHarvest(Resource resource)
    {
        if (audioElement != null) audioElement.Play(startHarvestSound);
        resourceDeposit = resource;
        StartMove(resource.transform.position, resource.gameObject);
        //we can only collect one resource at a time, other resources are lost
        if (harvestType == ResourceType.Unknown || harvestType != resource.GetResourceType())
        {
            harvestType = resource.GetResourceType();
            currentLoad = 0.0f;
        }
        harvesting = true;
        emptying = false;
    }
    private void StopHarvest()
    {

    }
    private void Collect()
    {
        if (audioElement != null && Time.timeScale > 0) audioElement.Play(harvestSound);
        float collect = collectionAmount * Time.deltaTime;
        //make sure that the harvester cannot collect more than it can carry
        if (currentLoad + collect > capacity) collect = capacity - currentLoad;
        if (resourceDeposit.isEmpty())
        {
            //Arms[] arms = GetComponentsInChildren<Arms>();
            //foreach (Arms arm in arms) arm.renderer.enabled = false;
            DecideWhatToDo();
        }
        else
        {
            resourceDeposit.Remove(collect);
        }
        currentLoad += collect;
    }
    private void Deposit()
    {
        currentLoad = Mathf.Floor(currentLoad);
        if (audioElement != null && Time.timeScale > 0) audioElement.Play(emptyHarvestSound);
        currentDeposit += depositAmount * Time.deltaTime;
        int deposit = Mathf.FloorToInt(currentDeposit);
        if (deposit >= 1)
        {
            if (deposit > currentLoad) deposit = Mathf.FloorToInt(currentLoad);
            currentDeposit -= deposit;
            currentLoad -= deposit;
            ResourceType depositType = harvestType;
            if (harvestType == ResourceType.Rock) depositType = ResourceType.Stone;
            player.AddResource(depositType, deposit);
        }
    }
    protected override void DrawSelectionBox(Rect selectBox)
    {
        base.DrawSelectionBox(selectBox);
        float percentFull = currentLoad / capacity;
        float maxHeight = selectBox.height - 4;
        float height = maxHeight * percentFull;
        float leftPos = selectBox.x + selectBox.width - 7;
        float topPos = selectBox.y + 2 + (maxHeight - height);
        float width = 5;
        Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
        if (resourceBar) GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
    }
    protected override void HandleLoadedProperty(JsonTextReader reader, string propertyName, object readValue)
    {
        base.HandleLoadedProperty(reader, propertyName, readValue);
        switch (propertyName)
        {
            case "Harvesting": harvesting = (bool)readValue; break;
            case "Emptying": emptying = (bool)readValue; break;
            case "CurrentLoad": currentLoad = (float)(double)readValue; break;
            case "CurrentDeposit": currentDeposit = (float)(double)readValue; break;
            case "HarvestType": harvestType = WorkManager.GetResourceType((string)readValue); break;
            case "ResourceDepositId": loadedDepositId = (int)(System.Int64)readValue; break;
            case "ResourceStoreId": loadedStoreId = (int)(System.Int64)readValue; break;
            default: break;
        }
    }
    protected override void InitialiseAudio()
    {
        base.InitialiseAudio();
        List<AudioClip> sounds = new List<AudioClip>();
        List<float> volumes = new List<float>();
        if (emptyHarvestVolume < 0.0f) emptyHarvestVolume = 0.0f;
        if (emptyHarvestVolume > 1.0f) emptyHarvestVolume = 1.0f;
        sounds.Add(emptyHarvestSound);
        volumes.Add(emptyHarvestVolume);
        if (harvestVolume < 0.0f) harvestVolume = 0.0f;
        if (harvestVolume > 1.0f) harvestVolume = 1.0f;
        sounds.Add(harvestSound);
        volumes.Add(harvestVolume);
        if (startHarvestVolume < 0.0f) startHarvestVolume = 0.0f;
        if (startHarvestVolume > 1.0f) startHarvestVolume = 1.0f;
        sounds.Add(startHarvestSound);
        volumes.Add(startHarvestVolume);
        audioElement.Add(sounds, volumes);
    }
    protected override bool ShouldMakeDecision()
    {
        if (harvesting || emptying) return false;
        return base.ShouldMakeDecision();
    }
    protected override void DecideWhatToDo()
    {
        base.DecideWhatToDo();
        List<WorldObject> resources = new List<WorldObject>();
        foreach (WorldObject nearbyObject in nearbyObjects)
        {
            Resource resource = nearbyObject.GetComponent<Resource>();
            if (resource && !resource.isEmpty()) resources.Add(nearbyObject);
        }
        WorldObject nearestObject = WorkManager.FindNearestWorldObjectInListToPosition(resources, transform.position);
        if (nearestObject)
        {
            Resource closestResource = nearestObject.GetComponent<Resource>();
            if (closestResource) StartHarvest(closestResource);
        }
        else if (harvesting)
        {
            harvesting = false;
            if (currentLoad > 0.0f)
            {
                //make sure that we have a whole number to avoid bugs
                //caused by floating point numbers
                currentLoad = Mathf.Floor(currentLoad);
                emptying = true;
                //Arms[] arms = GetComponentsInChildren<Arms>();
                //foreach (Arms arm in arms) arm.renderer.enabled = false;
                StartMove(resourceStore.transform.position, resourceStore.gameObject);
            }
        }
    }
}
