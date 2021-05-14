using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TextEntryBox : MonoBehaviour
{
    TextMesh text;
    private void Start()
    {
        text = GetComponentInChildren<TextMesh>();
    }
    public void ProcessKeyCode(KeyCode key)
    {
        string str = text.text;
        switch(key)
        {
            case KeyCode.Backspace:
                if (str.Length > 0) str = str.Substring(0, str.Length - 1);
                break;
            case KeyCode.Space:
                str += " ";
                break;
            default:
                str += key.ToString();
                break;
        }
        text.text = str;
    }
}
