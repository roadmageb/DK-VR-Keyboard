using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerPointer : MonoBehaviour
{
    [SerializeField] private Hand hand;
    public SteamVR_Input_Sources input;
    public SteamVR_Action_Boolean grabButton;

    public Transform keyboardAxis;
    private SpherePolygon myPolygon;

    public bool GetPointedKey(out KeyCode key)
    {
        Vector3 direction = transform.forward, xzdirection;
        float theta, phi;

        direction = keyboardAxis.InverseTransformDirection(direction);
        xzdirection = direction;
        xzdirection.y = 0;

        theta = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        phi = Mathf.Atan2(direction.y, xzdirection.magnitude) * Mathf.Rad2Deg;

        return myPolygon.GetPointedKey(new Vector2(theta, phi), out key);
    }

    private void SetDefaultKeyboard(float scale)
    {
        myPolygon = Manager.Inst.DefaultKeyBoard(hand, scale);
    }

    private void SetDefaultKeyboard()
    {
        SetDefaultKeyboard(Manager.Inst.defaultKeyboardScale);
    }
    private void Update()
    {
        if(grabButton.GetLastStateDown(input))
        {
            if(GetPointedKey(out KeyCode key))
            {
                Debug.Log(hand + " " + key);
            }
        }
    }
    private void Start()
    {
        SetDefaultKeyboard();
        if(hand == Hand.Left)
        {
            string str = "";
            foreach(int i in myPolygon.polygons[KeyCode.D])
            {
                str += myPolygon.vertices[i] + " ";
            }
            Debug.Log(str);
        }
    }
}
