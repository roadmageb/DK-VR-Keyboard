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

    public SpherePolygon (List<Vector2> _vertices, Dictionary<KeyCode, List<int>> _polygons)
    {
        vertices = new List<Vector2>(_vertices);
        polygons = new Dictionary<KeyCode, List<int>>(_polygons);
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
}