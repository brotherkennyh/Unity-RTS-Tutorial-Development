using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conquest : VictoryCondition
{

    public override string GetDescription()
    {
        return "Conquest";
    }

    public override bool GameFinished()
    {
        if (players == null) return true;
        int playersLeft = players.Length;
        foreach (Player player in players)
        {
            if (!PlayerMeetsConditions(player)) playersLeft--;
        }
        return playersLeft == 1;
    }

    public override bool PlayerMeetsConditions(Player player)
    {
        return player && !player.IsDead();
    }

}
