using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaselineKey : MonoBehaviour
{
    public KeyCode key;
    private TextMesh text;
    private Material material;
    [SerializeField] Color idleColor, overColor, downColor;
    void Start()
    {
        text = GetComponentInChildren<TextMesh>();
        text.text = Manager.Inst.keycodeStringDict[key].onBoard;
        material = GetComponentInChildren<Renderer>().material;
    }
    private void Update()
    {
        switch(Manager.Inst.GetKeyPushState(key))
        {
            case KeyPushState.IDLE:
                material.color = idleColor;
                break;
            case KeyPushState.OVER:
            case KeyPushState.UP:
                material.color = overColor;
                break;
            case KeyPushState.DOWN:
                material.color = downColor;
                break;
        }
    }
}
