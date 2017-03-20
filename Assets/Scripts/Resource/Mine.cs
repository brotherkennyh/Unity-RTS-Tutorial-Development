using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Mine : Resource {

    private int numBlocks;

    protected override void Start()
    {
        base.Start();
        numBlocks = GetComponentsInChildren< Rock >().Length;
        resourceType = ResourceType.Rock;
    }

    protected override void Update()
    {
        base.Update();
        float percentLeft = (float)amountLeft / (float)capacity;
        if (percentLeft < 0) percentLeft = 0;
        int numBlocksToShow = (int)(percentLeft * numBlocks);
        Rock[] blocks = GetComponentsInChildren<Rock>();
        if (numBlocksToShow >= 0 && numBlocksToShow < blocks.Length)
        {
            Rock[] sortedBlocks = new Rock[blocks.Length];
            //sort the list from highest to lowest
            foreach (Rock rock in blocks)
            {
                sortedBlocks[blocks.Length - int.Parse(rock.name)] = rock;
            }
            for (int i = numBlocksToShow; i < sortedBlocks.Length; i++)
            {
                //sortedBlocks[i].renderer.enabled = false;
                sortedBlocks[i].GetComponent<Renderer>().enabled = false;
            }
            CalculateBounds();
        }
    }
}
