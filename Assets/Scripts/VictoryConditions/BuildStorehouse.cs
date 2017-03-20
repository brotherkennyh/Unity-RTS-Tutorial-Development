using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildStorehouse : VictoryCondition
{

    public override string GetDescription()
    {
        return "Building Storehouse";
    }

    public override bool PlayerMeetsConditions(Player player)
    {
        Storehouse storehouse = player.GetComponentInChildren<Storehouse>();
        return player && !player.IsDead() && storehouse && !storehouse.UnderConstruction();
    }
}