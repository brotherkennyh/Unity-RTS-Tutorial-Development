using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storehouse : Building
{
    //nothing special to specify
    protected override bool ShouldMakeDecision()
    {
        return false;
    }
}
