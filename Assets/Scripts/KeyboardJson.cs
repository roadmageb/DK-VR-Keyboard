using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardJson
{
    public string layoutName;
    public Vector2 originScale;
    public List<Vector2>[] vertices;
    public Dictionary<KeyCode, Vector2>[] centroids;
    public Dictionary<KeyCode, int>[] learnCounts;
    public Dictionary<KeyCode, Vector2>[] learnVariance;
    public KeyboardJson(
        string name,
        Vector2 scale,
        List<Vector2>[] v,
        Dictionary<KeyCode, Vector2>[] c,
        Dictionary<KeyCode, int>[] cnt,
        Dictionary<KeyCode, Vector2>[] var)
    {
        layoutName = name;
        originScale = scale;
        vertices = v;
        centroids = c;
        learnCounts = cnt;
        learnVariance = var;
    }
}
