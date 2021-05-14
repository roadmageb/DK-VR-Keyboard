using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerPointer : MonoBehaviour
{
    [SerializeField] private Hand hand;
    [SerializeField] private SteamVR_Input_Sources input;
    [SerializeField] private SteamVR_Action_Boolean grabButton;

    private TextEntryBox currentTextBox;
    private SpherePolygon myPolygon;

    [SerializeField] private Transform centerRay, forwardRay;
    [SerializeField] private TextMesh text;
    [SerializeField] private Transform cam;

    private EntryState entryState;

    public bool GetPointedKey(out KeyCode key)
    {
        Vector3 direction = transform.forward, xzdirection;
        float theta, phi;

        direction = currentTextBox.transform.InverseTransformDirection(direction);
        xzdirection = direction;
        xzdirection.y = 0;

        theta = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        phi = Mathf.Atan2(direction.y, xzdirection.magnitude) * Mathf.Rad2Deg;

        return myPolygon.GetPointedKey(new Vector2(theta, phi), out key);
    }

    private void SetDefaultKeyboard(Vector2 scale)
    {
        myPolygon = Manager.Inst.DefaultKeyBoard(hand, scale);
    }

    private void SetDefaultKeyboard()
    {
        SetDefaultKeyboard(Manager.Inst.defaultKeyboardScale);
    }
    private void Update()
    {
        if(entryState == EntryState.Input)
        {
            centerRay.rotation = currentTextBox.transform.rotation;

            if (GetPointedKey(out KeyCode key))
            {
                if (grabButton.GetLastStateDown(input))
                {
                    currentTextBox.ProcessKeyCode(key);
                    Manager.Inst.entryExitTrigger[(int)hand] = false;
                }
                text.text = key.ToString();
                text.transform.LookAt(cam, Vector3.up);
            }
            else
            {
                if (grabButton.GetLastStateDown(input))
                {
                    if (Manager.Inst.entryExitTrigger[1 - (int)hand]) Manager.Inst.entryState = EntryState.Select;
                    else Manager.Inst.entryExitTrigger[(int)hand] = true;
                }
                text.text = "";
            }
        }
        else if(entryState == EntryState.Select)
        {
            if(hand == Hand.Left && grabButton.GetLastStateDown(input))
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
                {
                    TextEntryBox t = hit.transform.GetComponent<TextEntryBox>();
                    if (t != null) Manager.Inst.SetCurrentTextBox(t);
                    Manager.Inst.entryState = EntryState.Input;
                }
            }
        }
    }
    private void Start()
    {
        Manager.Inst.InitContorllerPointer(hand, this);
        SetDefaultKeyboard();
    }

    public void ChangeState(EntryState newState)
    {
        entryState = newState;
        if(entryState == EntryState.Select)
        {
            centerRay.gameObject.SetActive(false);
            forwardRay.gameObject.SetActive(false);
            Manager.Inst.entryExitTrigger[(int)hand] = false;
        }
        else if(entryState == EntryState.Input)
        {
            centerRay.gameObject.SetActive(true);
            forwardRay.gameObject.SetActive(true);
        }
    }

    public void SetCurrentTextBox(TextEntryBox textBox)
    {
        currentTextBox = textBox;
    }
}
