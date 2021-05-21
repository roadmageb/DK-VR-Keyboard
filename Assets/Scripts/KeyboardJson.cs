using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardJson
{
    public Vector2 originScale;
    public Vector2[] leftVertices;
    public Vector2[] rightVertices;
    public KeyboardJson(Vector2 scale, Vector2[] l, Vector2[] r)
    {
        originScale = scale;
        leftVertices = l;
        rightVertices = r;
    }
}
