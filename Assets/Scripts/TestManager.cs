using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class TestManager : Singleton<TestManager>
{
    public string testName;
    public string savePath;
    public List<string> sentenceBase;
    public string[] csvTestNames;
    public string csvPath;
    public bool csvWriteOnStart;
    private void Start()
    {
        if(csvWriteOnStart)
        {
            int testNum = csvTestNames.Length;

            List<TestResultJson>[] resultList = new List<TestResultJson>[testNum];
            for(int i = 0; i < testNum; i++)
            {
                Func<int, string> filePath = (x) => Application.persistentDataPath + savePath + csvTestNames[i] + "/" + csvTestNames[i] + "_" + x.ToString("000") + ".json";
                resultList[i] = new List<TestResultJson>();
                for (int n = 0; File.Exists(filePath(n)); n++)
                {
                    resultList[i].Add(JsonConvert.DeserializeObject<TestResultJson>(File.ReadAllText(filePath(n))));
                }
            }

            int columnNum = 2;
            List<List<string>> stringList = new List<List<string>>();

            stringList.Add(new List<string>());
            for(int i = 0; i < testNum; i++)
            {
                for (int n = 0; n < columnNum; n++) stringList[0].Add(csvTestNames[i]);
                stringList[0].Add("");
            }

            bool caseEnd = false;
            for (int n = 0; !caseEnd; n++)
            {
                List<string> newLine = new List<string>();
                caseEnd = true;
                for(int i = 0; i < testNum; i++)
                {
                    if(resultList[i].Count > n)
                    {
                        TestResultJson t = resultList[i][n];
                        newLine.Add(t.typePerMinute.ToString());
                        newLine.Add(t.typeTargetRate.ToString());
                        newLine.Add("");
                        caseEnd = false;
                    }
                }
                stringList.Add(newLine);
            }

            string ret = "";
            foreach(List<string> line in stringList)
            {
                ret += string.Join(",", line.ToArray()) + "\n";
            }

            string dir = Application.persistentDataPath + csvPath;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(dir + "result.csv", ret);
        }
    }
}
