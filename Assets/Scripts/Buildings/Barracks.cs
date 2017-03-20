using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : Building {

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Speeder", "Speeder", "Speeder", "Speeder", "Speeder", "Speeder", "Speeder", "Speeder" };
    }
    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
    protected override bool ShouldMakeDecision()
    {
        return false;
    }
}
