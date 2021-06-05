using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Manager : Singleton<Manager>
{
    [Header("General Settings")]
    public KeyboardMode keyboardMode;
    private string jsonPath;
    public string saveDataName;
    public Transform worldSpaceParent;
    public ControllerPointer[] controllerPointers { get; private set; } = new ControllerPointer[2];
    private EntryState _entryState;
    public EntryState entryState
    {
        get { return _entryState; }
        set
        {
            ChangeState(value);
            foreach (ControllerPointer p in controllerPointers) if (p != null) p.ChangeState(value);
            _entryState = value;
        }
    }

    [Header("Translations")]
    public KeyCodeStringPair[] keycodeTranslation;
    public Dictionary<KeyCode, (string str, string onBoard)> keycodeStringDict;

    [Header("DK Keyboard Settings")]
    [SerializeField] private bool defaultSet;
    public Vector2 defaultKeyboardScale;
    private Vector2 currentKeyboardScale;
    [SerializeField] private Texture2D[] presets;
    [SerializeField] private KeyCode[] leftKeyCodes, rightKeyCodes;
    [HideInInspector] public bool[] entryExitTrigger;

    [Header("Basline Keyboard Settings")]
    private float baselineKeyboardSize;
    //General functions
    private void ChangeState(EntryState s)
    {
        
    }
    public void InitContorllerPointer(Hand hand, ControllerPointer instance)
    {
        controllerPointers[(int)hand] = instance;
        instance.ChangeState(entryState);
    }
    public void SetCurrentTextBox(TextEntryBox textBox)
    {
        foreach(ControllerPointer p in controllerPointers)
        {
            p.SetCurrentTextBox(textBox);
        }
        textBox.StartEdit();
    }
    private void KeycodeStringDictInit()
    {
        keycodeStringDict = new Dictionary<KeyCode, (string str, string onBoard)>();
        foreach (KeyCodeStringPair p in keycodeTranslation) keycodeStringDict.Add(p.keycode, (p.str, p.onBoard));
    }
    private void Awake()
    {
        KeycodeStringDictInit();
        entryExitTrigger = new bool[2];
        entryState = EntryState.SELECT;
        jsonPath = Application.persistentDataPath + "/savedata/" + saveDataName + ".json";
        if (keyboardMode != KeyboardMode.BASELINE) GameObject.Find("BaseLineKeyboard").SetActive(false);
    }


    //DK Keyboard functions
    public List<KeyCode> GetAvailableKeyList()
    {
        List<KeyCode> ret = new List<KeyCode>();
        foreach(KeyCode key in leftKeyCodes)
        {
            if (!ret.Contains(key)) ret.Add(key);
        }
        foreach (KeyCode key in rightKeyCodes)
        {
            if (!ret.Contains(key)) ret.Add(key);
        }
        return ret;
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
                Dictionary<KeyCode, Vector2> centroids = new Dictionary<KeyCode, Vector2>();

                Vector2[] targetVArr = hand == Hand.LEFT ? keyJson.leftCentroids : keyJson.rightCentroids;
                KeyCode[] targetKArr = hand == Hand.LEFT ? leftKeyCodes : rightKeyCodes;

                int i = 0;
                foreach(KeyCode k in targetKArr) centroids[k] = targetVArr[i++];

                return new SpherePolygon(new List<Vector2>(
                    hand == Hand.LEFT ? keyJson.leftVertices : keyJson.rightVertices),
                    safeDefault.adjCenters,
                    safeDefault.polygons,
                    centroids);
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
        List<Vector2>[] centroidList = new List<Vector2>[2];

        for(int i = 0; i < 2; i++)
        {
            KeyCode[] target = i == 0 ? leftKeyCodes : rightKeyCodes;
            centroidList[i] = new List<Vector2>();
            foreach(KeyCode k in target)
            {
                centroidList[i].Add(controllerPointers[i].myPolygon.centroids[k]);
            }
        }

        KeyboardJson keyJson = new KeyboardJson(
            currentKeyboardScale, 
            controllerPointers[0].myPolygon.vertices.ToArray(), 
            controllerPointers[1].myPolygon.vertices.ToArray(),
            centroidList[0].ToArray(),
            centroidList[1].ToArray());

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
        int handidx = hand==Hand.LEFT ? 0 : 1;
        Vector2 origin = new Vector2();

        List<Vector2> vertices = new List<Vector2>();
        List<Vector2> centroids = new List<Vector2>();
        List<int> loopVertices = new List<int>();
        Dictionary<KeyCode, List<int>> polygons = new Dictionary<KeyCode, List<int>>();
        List<List<KeyCode>> adjCenters = new List<List<KeyCode>>();

        //convert points in image to vertices, centerPoints, keyVertices, loopVertices, origin
        for (int j = presets[handidx].height - 1; j >= 0; j--)
        {
            for (int i = 0; i < presets[handidx].width; i++)
            {
                Color c = presets[handidx].GetPixel(i, j);
                if (c.Equals(Color.black) || c.Equals(Color.blue))
                {
                    if (c.Equals(Color.blue)) loopVertices.Add(vertices.Count);
                    vertices.Add(new Vector2(i, j));
                }
                else if (c.Equals(Color.red))
                {
                    centroids.Add(new Vector2(i, j));
                }
                else if (c.Equals(Color.green))
                {
                    centroids.Add(new Vector2(i, j));
                    origin = new Vector2(i, j);
                }
            }
        }

        //init adjCenters
        foreach (Vector2 v in vertices) adjCenters.Add(new List<KeyCode>());

        //check each loop, construct adjCenters
        Vector2[] dirVec = new Vector2[] { new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1) };
        int codeidx = 0;
        foreach (int loop in loopVertices)
        {
            Vector2 pos = new Vector2(vertices[loop].x, vertices[loop].y);
            KeyCode loopKey = hand == Hand.LEFT ? leftKeyCodes[codeidx] : rightKeyCodes[codeidx];
            int dir = 0;
            pos += dirVec[dir];

            List<int> arr = new List<int>();
            arr.Add(loop);
            adjCenters[loop].Add(loopKey);

            while (!pos.Equals(vertices[loop]))
            {
                Color currentPixel = presets[handidx].GetPixel((int)pos.x, (int)pos.y);
                if (currentPixel.Equals(Color.black) || currentPixel.Equals(Color.blue))
                {
                    int nextDir = (dir + 1) % 4;
                    Vector2 nextPos = pos + dirVec[nextDir];
                    Color nextPixel = presets[handidx].GetPixel((int)nextPos.x, (int)nextPos.y);
                    if (!nextPixel.Equals(Color.white)) dir = (dir + 1) % 4;

                    int cIdx = vertices.FindIndex(x => x.Equals(pos));

                    arr.Add(cIdx);
                    adjCenters[cIdx].Add(loopKey);
                }
                pos += dirVec[dir];
            }

            polygons.Add(loopKey, arr);
            codeidx++;
        }

        //scaling
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] -= origin;
            vertices[i] *= scale;
        }

        for (int i = 0; i < centroids.Count; i++)
        {
            centroids[i] -= origin;
            centroids[i] *= scale;
        }

        SpherePolygon ret = new SpherePolygon(vertices, adjCenters, polygons);
        Dictionary<KeyCode, Vector2> retCentroids = new Dictionary<KeyCode, Vector2>();

        foreach(Vector2 centroid in centroids)
        {
            bool t = ret.GetPointedKey(centroid, out KeyCode key);
            if (t) retCentroids.Add(key, centroid);
        }
        ret.centroids = retCentroids;

        return ret;
    }


    //Baseline keyboard functions
    public KeyPushState GetKeyPushState(KeyCode key)
    {
        KeyPushState ret = KeyPushState.IDLE;
        foreach(ControllerPointer p in controllerPointers)
        {
            ret = (KeyPushState)Mathf.Max((int)ret, (int)p.GetKeyPushState(key));
        }
        return ret;
    }
}

[Serializable]
public class KeyCodeStringPair
{
    public KeyCode keycode;
    [TextArea]
    public string str;
    public string onBoard;
}