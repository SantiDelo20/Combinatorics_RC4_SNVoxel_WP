using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinatorialFiller : MonoBehaviour
{
    //1. Set a first block
        //Probably just select a random voxel with Z index 0
        //Position a block on this voxel

    //2. Find all the possible next voxels
        //loop over all the blocks
        //Where possibleDirection contains elements
            //Loop over possible directions elements
                //Get neighbour voxels of these elements in the direction
                //Check if index of neighbour voxel is withing grid (theres a Util function for that)
                    //Check if neighbour voxel is still available
                        //Add the neighbour voxel to the list of possible direction
                        //neihgbour voxels need to be unique ==> Look into hashset
    
    //3. Try adding a block on a random neighbourvoxel until the next block is built 

    //4. Loop over 2 --> 3 till you place a certain amount of blocks, or no more blocks can be added


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
