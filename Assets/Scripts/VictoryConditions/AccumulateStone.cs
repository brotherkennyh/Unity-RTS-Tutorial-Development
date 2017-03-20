using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class AccumulateStone : VictoryCondition
{

    public int amount = 50;

    private ResourceType type = ResourceType.Stone;

    public override string GetDescription()
    {
        return "Accumulating Stone";
    }

    public override bool PlayerMeetsConditions(Player player)
    {
        return player && !player.IsDead() && player.GetResourceAmount(type) >= amount;
    }
}