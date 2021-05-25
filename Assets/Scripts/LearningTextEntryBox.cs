using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearningTextEntryBox : TextEntryBox
{
    public List<KeyCode> keyCodeList;
    public KeyCode currentTarget;
    public void NextTarget()
    {
        int n = Random.Range(0, keyCodeList.Count);
        currentTarget = keyCodeList[n];
        text.text = currentTarget.ToString();
    }
    protected override void Start()
    {
        keyCodeList = Manager.Inst.GetAvailableKeyList();
        base.Start();
        NextTarget();
    }
    public override void ProcessKeyCode(KeyCode key)
    {
        NextTarget();
    }
}
