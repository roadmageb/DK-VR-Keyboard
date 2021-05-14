using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public static class KeyCodeTranslator
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int ToUnicode(
    uint virtualKeyCode,
    uint scanCode,
    byte[] keyboardState,
    StringBuilder receivingBuffer,
    int bufferSize,
    uint flags
    );

    public static string GetCharsFromKeys(KeyCode keys, bool shift)
    {
        var buf = new StringBuilder(256);
        var keyboardState = new byte[256];
        if (shift)
        {
            keyboardState[(int)KeyCode.LeftShift] = 0xff;
        }
        ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
        return buf.ToString();
    }
}
