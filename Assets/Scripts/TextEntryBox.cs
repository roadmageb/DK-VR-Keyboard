using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TextEntryBox : MonoBehaviour
{
    protected TextMesh text;
    protected virtual void Start()
    {
        text = GetComponentInChildren<TextMesh>();
    }
    public virtual void ProcessKeyCode(KeyCode key)
    {
        string str = text.text;
        switch(key)
        {
            case KeyCode.Backspace:
                if (str.Length > 0) str = str.Substring(0, str.Length - 1);
                break;
            default:
                str += Manager.Inst.keycodeStringDict[key].str;
                break;
        }
        text.text = str;
    }
}
