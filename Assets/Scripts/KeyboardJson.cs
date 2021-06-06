using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardJson
{
    public string layoutName;
    public Vector2 originScale;
    public List<Vector2> leftVertices;
    public List<Vector2> rightVertices;
    public Dictionary<KeyCode, Vector2> leftCentroids;
    public Dictionary<KeyCode, Vector2> rightCentroids;
    public Dictionary<KeyCode, int> leftLearnCount;
    public Dictionary<KeyCode, int> rightLearnCount;
    public KeyboardJson(
        string name,
        Vector2 scale,
        List<Vector2> l,
        List<Vector2> r,
        Dictionary<KeyCode, Vector2> lc,
        Dictionary<KeyCode, Vector2> rc,
        Dictionary<KeyCode, int> ll,
        Dictionary<KeyCode, int> rl)
    {
        layoutName = name;
        originScale = scale;
        leftVertices = l;
        rightVertices = r;
        leftCentroids = lc;
        rightCentroids = rc;
        leftLearnCount = ll;
        rightLearnCount = rl;
    }
}
