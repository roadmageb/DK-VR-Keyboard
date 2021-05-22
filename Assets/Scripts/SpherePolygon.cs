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
    /// List of List(index of vertices) polygons
    /// two adjacent indices form an edge
    /// </summary>
    public Dictionary<KeyCode, List<int>> polygons;

    public float[,] safeDistance;
    public Dictionary<KeyCode, List<(Vector2 v, float w)>> learningLog;

    private float maxStepDistance = 0.5f;
    private float stepDistRate = 1;
    private float maxPushDistance = 1;

    public SpherePolygon (List<Vector2> _vertices, Dictionary<KeyCode, List<int>> _polygons, float[,] _safeDistance)
    {
        vertices = new List<Vector2>(_vertices);
        polygons = new Dictionary<KeyCode, List<int>>(_polygons);
        safeDistance = _safeDistance.Clone() as float[,];
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

    public void InitLearning(float maxDist, float rate)
    {
        maxStepDistance = maxDist;
        stepDistRate = rate;
        InitLearning();
    }

    public void StepLearning(KeyCode key, Vector2 pos)
    {
        if (learningLog == null) InitLearning();

        Vector2[] diff = new Vector2[vertices.Count];
        Func<float, float> distMoveFunction = x => 1 - x;

        learningLog[key].Add((pos, 1f));

        foreach(KeyCode k in learningLog.Keys)
        {
            foreach(var log in learningLog[k])
            {
                KeyCode outKey;
                bool clicked = GetPointedKey(log.v, out outKey);

                if (!clicked || outKey != k)
                {
                    float[] distArr = new float[polygons[k].Count];
                    float minDist = -1, maxDist = -1;
                    for (int i = 0; i < polygons[k].Count; i++)
                    {
                        float d = Vector2.Distance(vertices[polygons[k][i]], log.v);
                        if (minDist < 0 || d < minDist) minDist = d;
                        if (maxDist < 0 || d > maxDist) maxDist = d;
                        distArr[i] = d;
                    }
                    if (minDist < maxDist)
                    {
                        for (int i = 0; i < polygons[k].Count; i++)
                        {
                            distArr[i] = (distArr[i] - minDist) / (maxDist - minDist);
                            distArr[i] = distMoveFunction(distArr[i]);

                            Vector2 temp = (log.v - vertices[polygons[k][i]]) * stepDistRate;
                            diff[polygons[k][i]] += temp * distArr[i] * log.w;
                        }
                    }
                }
                else if (outKey == k)
                {
                    for (int i = 0; i < polygons[k].Count; i++)
                    {
                        float d = Vector2.Distance(vertices[polygons[k][i]], log.v);
                        if(d < maxPushDistance)
                        {
                            Vector2 temp = (vertices[polygons[k][i]] - log.v).normalized * (maxPushDistance - d) * 0.8f;
                            diff[polygons[k][i]] += temp * log.w;
                        }
                    }
                }
            }
            for (int i = 0; i < learningLog[k].Count; i++)
            {
                learningLog[k][i] = (learningLog[k][i].v, learningLog[k][i].w * 0.9f);
            }
        }
        for(int i = 0; i < vertices.Count; i++)
        {
            if (diff[i].magnitude > maxStepDistance) diff[i] *= maxStepDistance / diff[i].magnitude;
            vertices[i] += diff[i];
        }

        diff = new Vector2[vertices.Count];

        for(int i = 0; i < vertices.Count; i++)
        {
            for(int j = 0; j < vertices.Count; j++)
            {
                if (Vector2.Distance(vertices[i], vertices[j]) < safeDistance[i,j])
                {
                    diff[i] += (vertices[i] - vertices[j]).normalized * (safeDistance[i, j] - Vector2.Distance(vertices[i], vertices[j])) / 2;
                }
            }
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] += diff[i];
        }
    }
}