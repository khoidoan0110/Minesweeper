using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cell
{
    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }
    public Type type;

    public Vector3Int position;
    public int number;

    public bool revealed, exploded, flagged;
}
