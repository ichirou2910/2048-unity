using System;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector2 Position => transform.position;

    public Block OccupiedBlock;
}
