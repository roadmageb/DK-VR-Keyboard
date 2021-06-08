using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class Manager : Singleton<Manager>
{
    [Header("General Settings")]
    public KeyboardMode keyboardMode;
    private string jsonPath;
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
    public string saveDataName;
    [SerializeField] private bool defaultSet;
    public Vector2 defaultKeyboardScale;
    private Vector2 currentKeyboardScale;
    [SerializeField] private DKLayoutPreset[] layoutList;
    [SerializeField] private string currentLayoutName;
    private Texture2D[] presets;
    [SerializeField] private KeyCode[] leftKeyCodes, rightKeyCodes;
    [HideInInspector] public bool[] entryExitTrigger;
    [SerializeField] private int initialLearningCount;

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
        //File.WriteAllText(Application.persistentDataPath + "/savedata/left.txt", JsonConvert.SerializeObject(leftKeyCodes));
        //File.WriteAllText(Application.persistentDataPath + "/savedata/right.txt", JsonConvert.SerializeObject(rightKeyCodes));
        KeycodeStringDictInit();
        ImportLayout();
        entryExitTrigger = new bool[2];
        entryState = EntryState.SELECT;
        jsonPath = Application.persistentDataPath + "/savedata/" + saveDataName + ".json";
        if (keyboardMode != KeyboardMode.BASELINE)
        {
            GameObject g = GameObject.Find("BaseLineKeyboard");
            if(g!=null)g.SetActive(false);
        }
    }

    private void ImportLayout()
    {
        foreach(DKLayoutPreset layout in layoutList)
        {
            if (layout.name == currentLayoutName)
            {
                presets = layout.image;
                leftKeyCodes = JsonConvert.DeserializeObject<KeyCode[]>(layout.binding[0].text);
                rightKeyCodes = JsonConvert.DeserializeObject<KeyCode[]>(layout.binding[1].text);
                break;
            }
        }
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
                KeyboardJson keyJson = JsonConvert.DeserializeObject<KeyboardJson>(str);

                currentLayoutName = keyJson.layoutName;
                ImportLayout();

                SpherePolygon safeDefault = DefaultKeyboard(hand, keyJson.originScale);
                currentKeyboardScale = keyJson.originScale;

                Debug.Log(safeDefault.polygons.Count);

                return new SpherePolygon(
                    hand == Hand.LEFT ? keyJson.leftVertices : keyJson.rightVertices,
                    safeDefault.adjCenters,
                    safeDefault.polygons,
                    hand == Hand.LEFT ? keyJson.leftCentroids : keyJson.rightCentroids,
                    hand == Hand.LEFT ? keyJson.leftLearnCount : keyJson.rightLearnCount);
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

        KeyboardJson keyJson = new KeyboardJson(
            currentLayoutName,
            currentKeyboardScale, 
            controllerPointers[0].myPolygon.vertices, 
            controllerPointers[1].myPolygon.vertices,
            controllerPointers[0].myPolygon.centroids,
            controllerPointers[1].myPolygon.centroids,
            controllerPointers[0].myPolygon.learnCount,
            controllerPointers[1].myPolygon.learnCount);

        string dir = jsonPath.Substring(0, jsonPath.LastIndexOf('/') + 1);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(keyJson));
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
        Dictionary<KeyCode, int> learnCount = new Dictionary<KeyCode, int>();
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
        Debug.Log(loopVertices.Count);
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
            learnCount.Add(loopKey, initialLearningCount);
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
        ret.learnCount = learnCount;

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

[Serializable]
public class DKLayoutPreset
{
    public string name;
    public Texture2D[] image;
    public TextAsset[] binding;
}