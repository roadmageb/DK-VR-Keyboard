using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardJson
{
    public Vector2 originScale;
    public Vector2[] leftVertices;
    public Vector2[] rightVertices;
    public Vector2[] leftCentroids;
    public Vector2[] rightCentroids;
    public KeyboardJson(Vector2 scale, Vector2[] l, Vector2[] r, Vector2[] lc, Vector2[] rc)
    {
        originScale = scale;
        leftVertices = l;
        rightVertices = r;
        leftCentroids = lc;
        rightCentroids = rc;
    }
}
