using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;
using Newtonsoft.Json;

public class Unit : WorldObject {

    /*** Game Engine methods, all can be overridden by subclass ***/
    protected bool moving, rotating;

    private int loadedDestinationTargetId = -1;
    private GameObject destinationTarget;
    private Quaternion targetRotation;
    private Vector3 destination;

    public AudioClip driveSound, moveSound;
    public float driveVolume = 0.5f, moveVolume = 1.0f;
    public float moveSpeed, rotateSpeed;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        if (player && loadedSavedValues && loadedDestinationTargetId >= 0)
        {
            destinationTarget = player.GetObjectForId(loadedDestinationTargetId).gameObject;
        }
    }
    protected override void Update()
    {
        base.Update();
        if (rotating) TurnToTarget();
        else if (moving) MakeMove();
    }
    protected override void OnGUI()
    {
        base.OnGUI();
    }
    public override void SetHoverState(GameObject hoverObject)
    {
        base.SetHoverState(hoverObject);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected)
        {
            bool moveHover = false;
            if (WorkManager.ObjectIsGround(hoverObject))
            {
                moveHover = true;
            }
            else
            {
                Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
                if (resource && resource.isEmpty()) moveHover = true;
            }
            if (moveHover) player.hud.SetCursorState(CursorState.Move);
        }
    }
    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {
        base.MouseClick(hitObject, hitPoint, controller);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected)
        {
            bool clickedOnEmptyResource = false;
            if (hitObject.transform.parent)
            {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();
                if (resource && resource.isEmpty()) clickedOnEmptyResource = true;
            }
            if ((WorkManager.ObjectIsGround(hitObject) || clickedOnEmptyResource) && hitPoint != ResourceManager.InvalidPosition)
            {
                float x = hitPoint.x;
                //makes sure that the unit stays on top of the surface it is on
                float y = hitPoint.y + player.SelectedObject.transform.position.y;
                float z = hitPoint.z;
                Vector3 destination = new Vector3(x, y, z);
                StartMove(destination);
            }
        }
    }
    public override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);
        SaveManager.WriteBoolean(writer, "Moving", moving);
        SaveManager.WriteBoolean(writer, "Rotating", rotating);
        SaveManager.WriteVector(writer, "Destination", destination);
        SaveManager.WriteQuaternion(writer, "TargetRotation", targetRotation);
        if (destinationTarget)
        {
            WorldObject destinationObject = destinationTarget.GetComponent<WorldObject>();
            if (destinationObject) SaveManager.WriteInt(writer, "DestinationTargetId", destinationObject.ObjectId);
        }
    }
    public virtual void StartMove(Vector3 destination)
    {
        if (audioElement != null) audioElement.Play(moveSound);
        this.destination = destination;
        targetRotation = Quaternion.LookRotation(destination - transform.position);
        rotating = true;
        moving = false;
        destinationTarget = null;
    }
    public virtual void StartMove(Vector3 destination, GameObject destinationTarget)
    {
        StartMove(destination);
        this.destinationTarget = destinationTarget;
    }
    private void TurnToTarget()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
        //sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
        Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
        if (transform.rotation == targetRotation || transform.rotation == inverseTargetRotation)
        {
            if (audioElement != null) audioElement.Play(driveSound);
            rotating = false;
            moving = true;
        }
        CalculateBounds();
        if (destinationTarget) CalculateTargetDestination();
    }
    private void MakeMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
        if (transform.position == destination)
        {
            if (audioElement != null) audioElement.Stop(driveSound);
            moving = false;
            movingIntoPosition = false;
        }
        CalculateBounds();
    }
    private void CalculateTargetDestination()
    {
        //calculate number of unit vectors from unit centre to unit edge of bounds
        Vector3 originalExtents = selectionBounds.extents;
        Vector3 normalExtents = originalExtents;
        normalExtents.Normalize();
        float numberOfExtents = originalExtents.x / normalExtents.x;
        int unitShift = Mathf.FloorToInt(numberOfExtents);

        //calculate number of unit vectors from target centre to target edge of bounds
        WorldObject worldObject = destinationTarget.GetComponent<WorldObject>();
        if (worldObject) originalExtents = worldObject.GetSelectionBounds().extents;
        else originalExtents = new Vector3(0.0f, 0.0f, 0.0f);
        normalExtents = originalExtents;
        normalExtents.Normalize();
        numberOfExtents = originalExtents.x / normalExtents.x;
        int targetShift = Mathf.FloorToInt(numberOfExtents);

        //calculate number of unit vectors between unit centre and destination centre with bounds just touching
        int shiftAmount = targetShift + unitShift;

        //calculate direction unit needs to travel to reach destination in straight line and normalize to unit vector
        Vector3 origin = transform.position;
        Vector3 direction = new Vector3(destination.x - origin.x, 0.0f, destination.z - origin.z);
        direction.Normalize();

        //destination = center of destination - number of unit vectors calculated above
        //this should give us a destination where the unit will not quite collide with the target
        //giving the illusion of moving to the edge of the target and then stopping
        for (int i = 0; i < shiftAmount; i++) destination -= direction;
        destination.y = destinationTarget.transform.position.y;
    }
    public virtual void SetBuilding(Building creator)
    {
        //specific initialization for a unit can be specified here
    }
    protected override void HandleLoadedProperty(JsonTextReader reader, string propertyName, object readValue)
    {
        base.HandleLoadedProperty(reader, propertyName, readValue);
        switch (propertyName)
        {
            case "Moving": moving = (bool)readValue; break;
            case "Rotating": rotating = (bool)readValue; break;
            case "Destination": destination = LoadManager.LoadVector(reader); break;
            case "TargetRotation": targetRotation = LoadManager.LoadQuaternion(reader); break;
            case "DestinationTargetId": loadedDestinationTargetId = (int)(System.Int64)readValue; break;
            default: break;
        }
    }
    protected override void InitialiseAudio()
    {
        base.InitialiseAudio();
        List<AudioClip> sounds = new List<AudioClip>();
        List<float> volumes = new List<float>();
        if (driveVolume < 0.0f) driveVolume = 0.0f;
        if (driveVolume > 1.0f) driveVolume = 1.0f;
        volumes.Add(driveVolume);
        sounds.Add(driveSound);
        if (moveVolume < 0.0f) moveVolume = 0.0f;
        if (moveVolume > 1.0f) moveVolume = 1.0f;
        sounds.Add(moveSound);
        volumes.Add(moveVolume);
        audioElement.Add(sounds, volumes);
    }
    protected override bool ShouldMakeDecision()
    {
        if (moving || rotating) return false;
        return base.ShouldMakeDecision();
    }
}
