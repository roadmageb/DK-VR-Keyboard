using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : Singleton<Manager>
{
    [SerializeField] private Texture2D[] presets;
    [SerializeField] private KeyCode[] leftKeyCodes, rightKeyCodes;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hand">F/T:Left/Right</param>
    /// <param name="scale"></param>
    public SpherePolygon DefaultKeyBoard(bool hand, float scale)
    {
        int handidx = hand ? 1 : 0;
        Vector2 centerPoint = new Vector2();

        List<Vector2> vertices = new List<Vector2>();
        List<int> keyVertices = new List<int>();
        Dictionary<KeyCode, List<int>> polygons = new Dictionary<KeyCode, List<int>>();

        for (int j = presets[handidx].height - 1; j >= 0; j--)
        {
            for (int i = 0; i < presets[handidx].width; i++)
            {
                Color c = presets[handidx].GetPixel(i, j);
                if (c.Equals(Color.black) || c.Equals(Color.blue))
                {
                    if (c.Equals(Color.blue)) keyVertices.Add(vertices.Count);
                    vertices.Add(new Vector2(i, j));
                }
                else if (c.Equals(Color.red)) centerPoint = new Vector2(i, j);
            }
        }

        Vector2[] dirVec = new Vector2[] { new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1) };
        int codeidx = 0;
        foreach (int key in keyVertices)
        {
            Vector2 pos = new Vector2(vertices[key].x, vertices[key].y);
            int dir = 0;
            pos += dirVec[dir];

            List<int> arr = new List<int>();
            arr.Add(key);

            while (!pos.Equals(vertices[key]))
            {
                Color currentPixel = presets[handidx].GetPixel((int)pos.x, (int)pos.y);
                if (currentPixel.Equals(Color.black) || currentPixel.Equals(Color.blue))
                {
                    int nextDir = (dir + 1) % 4;
                    Vector2 nextPos = pos + dirVec[nextDir];
                    Color nextPixel = presets[handidx].GetPixel((int)nextPos.x, (int)nextPos.y);
                    if (!nextPixel.Equals(Color.white)) dir = (dir + 1) % 4;

                    arr.Add(vertices.FindIndex(x => x.Equals(pos)));
                }
                pos += dirVec[dir];
            }
            polygons.Add(hand?rightKeyCodes[codeidx++]:leftKeyCodes[codeidx++], arr);
        }

        foreach (Vector2 vertex in vertices)
        {
            Vector2 v = vertex;
            v -= centerPoint;
            v *= scale;
        }

        return new SpherePolygon(vertices, polygons);
    }

    private void Start()
    {
    }
}
