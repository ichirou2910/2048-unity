using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockStateEnum
{
    Idle,
    
    // Merge direction: [Merging] <<< [ToBeMerged]
    Merging,
    ToBeMerged
}
