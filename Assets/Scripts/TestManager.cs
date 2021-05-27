using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : Singleton<TestManager>
{
    public string testName;
    public string savePath;
    public List<string> sentenceBase;
}
