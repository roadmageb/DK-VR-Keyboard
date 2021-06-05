using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestTextBox : TextEntryBox
{
    private List<string> remainSentence;
    private string _currentSentence;
    private string currentSentence
    {
        get { return _currentSentence; }
        set
        {
            targetTextMesh.text = value;
            _currentSentence = value;
        }
    }


    private string _entryText;
    private string entryText
    {
        get { return _entryText; }
        set
        {
            typeTextMesh.text = value;
            _entryText = value;
        }
    }

    private float startTime;
    private int totalLetter;
    private float totalTime;
    private int totalType;
    private int totalTypo;
    private int currentPart;

    private Coroutine wrongAlarmRoutine;

    [SerializeField] TextMesh targetTextMesh, typeTextMesh;

    protected override void Start()
    {
        
    }

    private bool ChangeSentence()
    {
        if(remainSentence.Count > 0)
        {
            int rnd = UnityEngine.Random.Range(0, remainSentence.Count);
            currentSentence = remainSentence[rnd];
            remainSentence.RemoveAt(rnd);
            entryText = "";
            startTime = Time.realtimeSinceStartup;
            totalLetter = 0;
            totalTime = 0;
            totalType = 0;
            totalTypo = 0;
            currentPart = 0;
            return true;
        }
        return false;
    }

    public override void ProcessKeyCode(KeyCode key)
    {
        if (startTime == -1) startTime = Time.realtimeSinceStartup;
        switch(key)
        {
            case KeyCode.Backspace:
                if (entryText.Length > 0) entryText = entryText.Substring(0, entryText.Length - 1);
                break;
            case KeyCode.Return:
                if (entryText.Equals(currentSentence))
                {
                    totalTime += Time.realtimeSinceStartup - startTime;
                    totalLetter += currentSentence.Length;
                    if(!ChangeSentence())
                    {
                        SaveTestResult();
                        currentSentence = "==Test Done!!==";
                        entryText = "";
                        Manager.Inst.entryState = EntryState.SELECT;
                    }
                }
                else
                {
                    if (wrongAlarmRoutine != null) StopCoroutine(wrongAlarmRoutine);
                    wrongAlarmRoutine = StartCoroutine(WrongAnswerCoroutine());
                }
                break;
            default:
                entryText += Manager.Inst.keycodeStringDict[key].str;
                if (currentPart == entryText.Length - 1 && entryText.Length <= currentSentence.Length)
                {
                    totalType++;
                    if (entryText[entryText.Length - 1] == currentSentence[entryText.Length - 1])
                    {
                        currentPart++;
                    }
                    else
                    {
                        totalTypo++;
                    }
                }
                break;
        }
    }
    private void SaveTestResult()
    {
        TestResultJson result = new TestResultJson();
        result.typePerMinute = totalLetter / totalTime * 60;
        result.typeTargetRate = (1f - ((float)totalTypo / totalType)) * 100;

        string dir = Application.persistentDataPath + TestManager.Inst.savePath + TestManager.Inst.testName;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        int n = 0;
        Func<int, string> filePath = (x) => dir + TestManager.Inst.testName + "/" + "_" + x.ToString("000") + ".txt";
        for (; File.Exists(filePath(n)); n++) ;

        File.WriteAllText(filePath(n), JsonConvert.SerializeObject(result));
    }
    public override void StartEdit()
    {
        remainSentence = new List<string>(TestManager.Inst.sentenceBase);
        ChangeSentence();
        startTime = -1;
    }
    public override void EndEdit()
    {
        base.EndEdit();
    }
    private IEnumerator WrongAnswerCoroutine()
    {
        Action<float> a = (x) => { typeTextMesh.color = Color.Lerp(Color.red, Color.black, x); };
        float d = 1f;
        for(float t = 0; t < d; t += Time.deltaTime)
        {
            a(t / d);
            yield return null;
        }
        a(1f);
    }
}
public class TestResultJson
{
    public float typePerMinute;
    public float typeTargetRate;
    //public List<string> testSentence;
    //public List<float> testTime;
    //public List<float> typoRate;
    //public Dictionary<(KeyCode target, KeyCode typo), int> typoTypeCount;
    //public Dictionary<(KeyCode target, KeyCode prev), List<(float time, bool typoOccured)>> sequenceTypeTime;
}