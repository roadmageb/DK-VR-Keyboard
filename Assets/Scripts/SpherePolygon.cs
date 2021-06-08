using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePolygon
{
    /// <summary>
    /// List of (theta, phi) vertices
    /// </summary>
    public List<Vector2> vertices;
    /// <summary>
    /// adjacent center points of each vertices
    /// idx < 0 then borderPoints[-(idx+1)]
    /// </summary>
    public List<List<KeyCode>> adjCenters;
    /// <summary>
    /// List of List(index of vertices) polygons
    /// two adjacent indices form an edge
    /// </summary>
    public Dictionary<KeyCode, List<int>> polygons;
    /// <summary>
    /// center points of each polygons
    /// </summary>
    public Dictionary<KeyCode, Vector2> centroids;
    public Dictionary<KeyCode, int> learnCount;
    public Dictionary<KeyCode, List<(Vector2 v, float w)>> learningLog;

    private float maxStepDistance = 5;

    public SpherePolygon (List<Vector2> _vertices, List<List<KeyCode>> _adjCenters, Dictionary<KeyCode, List<int>> _polygons)
    {
        vertices = _vertices;
        adjCenters = _adjCenters;
        polygons = _polygons;
    }
    public SpherePolygon(
        List<Vector2> _vertices,
        List<List<KeyCode>> _adjCenters,
        Dictionary<KeyCode, List<int>> _polygons,
        Dictionary<KeyCode, Vector2> _centroids,
        Dictionary<KeyCode, int> _learnCount
        )
    {
        vertices = _vertices;
        adjCenters = _adjCenters;
        polygons = _polygons;
        centroids = _centroids;
        learnCount = _learnCount;
    }

    public bool GetPointedKey(Vector2 target, out KeyCode key)
    {
        foreach(KeyCode k in polygons.Keys)
        {
            List<int> pidx = polygons[k];
            int vnum = pidx.Count;
            int cnt = 0;
            for (int i=0; i<vnum; i++)
            {
                int j = (i + 1) % vnum;
                Vector2 veci = vertices[pidx[i]];
                Vector2 vecj = vertices[pidx[j]];

                if ((veci.y > target.y) != (vecj.y > target.y))
                {
                    double atX = (vecj.x - veci.x) * (target.y - veci.y) / (vecj.y - veci.y) + veci.x;
                    if (target.x < atX) cnt++;
                }
            }
            if (cnt % 2 > 0)
            {
                key = k; return true;
            }
        }
        key = KeyCode.A; return false;
    }

    public void InitLearning()
    {
        learningLog = new Dictionary<KeyCode, List<(Vector2 v, float w)>>();
        foreach(KeyCode k in polygons.Keys)
        {
            learningLog.Add(k, new List<(Vector2 v, float w)>());
        }
    }

    public void InitLearning(float maxDist)
    {
        maxStepDistance = maxDist;
        InitLearning();
    }

    public void StepLearning(KeyCode key, Vector2 pos)
    {
        Vector2 moveVec = (pos - centroids[key]) / (learnCount[key] + 1);
        //if (moveVec.magnitude > maxStepDistance) moveVec = moveVec.normalized * maxStepDistance;

        foreach (int idx in polygons[key])
        {
            float weightSum = 0, weightTarget = 0;
            foreach(KeyCode ctr in adjCenters[idx])
            {
                float dist = 1f / Mathf.Max(Vector2.Distance(centroids[ctr], vertices[idx]), 0.0001f);
                weightSum += dist;
                if (ctr == key) weightTarget = dist;
            }
            vertices[idx] += moveVec * (weightTarget / weightSum);
        }

        centroids[key] += moveVec;
        learnCount[key]++;
    }
}