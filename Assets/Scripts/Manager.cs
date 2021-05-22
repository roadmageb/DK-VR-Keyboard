using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Manager : Singleton<Manager>
{
    private string jsonPath;

    public KeyCodeStringPair[] keycodeTranslation;
    public Dictionary<KeyCode, string> keycodeStringDict;
    public ControllerPointer[] controllerPointers { get; private set; } = new ControllerPointer[2];
    public bool[] entryExitTrigger;

    [SerializeField] private bool defaultSet;
    public Vector2 defaultKeyboardScale;
    private Vector2 currentKeyboardScale;
    [SerializeField] private Texture2D[] presets;
    [SerializeField] private KeyCode[] leftKeyCodes, rightKeyCodes;

    private EntryState _entryState;
    public EntryState entryState
    {
        get { return _entryState; }
        set { foreach (ControllerPointer p in controllerPointers) if(p != null) p.ChangeState(value); _entryState = value; }
    }

    public void InitContorllerPointer(Hand hand, ControllerPointer instance)
    {
        controllerPointers[(int)hand] = instance;
    }

    public void SetCurrentTextBox(TextEntryBox textBox)
    {
        foreach(ControllerPointer p in controllerPointers)
        {
            p.SetCurrentTextBox(textBox);
        }
    }

    public SpherePolygon GetKeyboard(Hand hand)
    {
        if (!defaultSet)
        {
            try
            {
                string str = File.ReadAllText(jsonPath);
                KeyboardJson keyJson = JsonUtility.FromJson<KeyboardJson>(str);
                SpherePolygon safeDefault = DefaultKeyboard(hand, keyJson.originScale);
                currentKeyboardScale = keyJson.originScale;

                return new SpherePolygon(new List<Vector2>(hand == Hand.Left ? keyJson.leftVertices : keyJson.rightVertices), safeDefault.polygons, safeDefault.safeDistance);
            }
            catch (DirectoryNotFoundException)
            {
                defaultSet = true;
            }
            catch (FileNotFoundException)
            {
                defaultSet = true;
            }
        }
        currentKeyboardScale = defaultKeyboardScale;
        return DefaultKeyboard(hand, defaultKeyboardScale);
    }
    public void SaveKeyboard()
    {
        KeyboardJson keyJson = new KeyboardJson(currentKeyboardScale, controllerPointers[0].myPolygon.vertices.ToArray(), controllerPointers[1].myPolygon.vertices.ToArray());

        string dir = jsonPath.Substring(0, jsonPath.LastIndexOf('/') + 1);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(jsonPath, JsonUtility.ToJson(keyJson));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="scale"></param>
    public SpherePolygon DefaultKeyboard(Hand hand, Vector2 scale)
    {
        int handidx = hand==Hand.Left ? 0 : 1;
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

            polygons.Add(hand==Hand.Left? leftKeyCodes[codeidx++]: rightKeyCodes[codeidx++], arr);
        }
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] -= centerPoint;
            vertices[i] *= scale;
        }

        float[,] safeDistance = new float[vertices.Count, vertices.Count];

        for(int i = 0; i < vertices.Count; i++)
        {
            for(int j = 0; j < vertices.Count; j++)
            {
                safeDistance[i, j] = Vector2.Distance(vertices[i], vertices[j]) * 0.9f;
            }
        }

        return new SpherePolygon(vertices, polygons, safeDistance);
    }

    private void Start()
    {
        KeycodeStringDictInit();
        entryExitTrigger = new bool[2];
        entryState = EntryState.Select;
        jsonPath = Application.persistentDataPath + "/savedata/data.json";
        Debug.Log(jsonPath);
    }
    private void KeycodeStringDictInit()
    {
        keycodeStringDict = new Dictionary<KeyCode, string>();
        foreach (KeyCodeStringPair p in keycodeTranslation) keycodeStringDict.Add(p.keycode, p.str);
    }
}

[Serializable]
public class KeyCodeStringPair
{
    public KeyCode keycode;
    public string str;
}