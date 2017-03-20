using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sanctuary : Building {

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Builder" };
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
