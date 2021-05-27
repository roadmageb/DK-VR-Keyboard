using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerPointer : MonoBehaviour
{
    [SerializeField] private Hand hand;
    [SerializeField] private SteamVR_Input_Sources input;
    [SerializeField] private SteamVR_Action_Boolean grabButton;
    [SerializeField] private SteamVR_Action_Vibration hapticAction;

    private TextEntryBox currentTextBox;
    private Transform currentTextDir;
    public SpherePolygon myPolygon { get; private set; }

    [SerializeField] private Transform centerRay, forwardRay;
    [SerializeField] private LineRenderer pointRay;
    [SerializeField] private TextMesh text;
    [SerializeField] private Transform cam;
    [SerializeField] private GameObject keyLinePrefab;
    [SerializeField] private float keyLineRadius;
    [SerializeField] private Color keyLineColor;
    private LineRenderer[] keyLines;
    [SerializeField] private GameObject keyTextPrefab;
    [SerializeField] private float keyTextSize;
    [SerializeField] private Color keyTextColor;
    private TextMesh[] keyTexts;

    private EntryState entryState;

    private bool previousKeyBool = false;
    private KeyCode previousKeyCode = KeyCode.A;

    public bool GetPointedKey(out KeyCode key, out Vector2 vec)
    {
        Vector3 direction = transform.forward, xzdirection;
        float theta, phi;

        direction = currentTextDir.InverseTransformDirection(direction);
        xzdirection = direction;
        xzdirection.y = 0;

        theta = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        phi = Mathf.Atan2(direction.y, xzdirection.magnitude) * Mathf.Rad2Deg;

        vec = new Vector2(theta, phi);

        return myPolygon.GetPointedKey(new Vector2(theta, phi), out key);
    }
    private void KeyboardInteraction()
    {
        centerRay.rotation = currentTextDir.rotation;
        bool pointed = GetPointedKey(out KeyCode key, out Vector2 pos);
        if (pointed)
        {
            if (grabButton.GetLastStateDown(input))
            {
                Manager.Inst.entryExitTrigger[(int)hand] = false;
                if (currentTextBox is LearningTextEntryBox)
                {
                    KeyCode target = (currentTextBox as LearningTextEntryBox).currentTarget;
                    if (myPolygon.polygons.ContainsKey(target))
                    {
                        myPolygon.StepLearning(target, pos);
                        Manager.Inst.SaveKeyboard();
                    }
                }
                currentTextBox.ProcessKeyCode(key);
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
                if (currentTextBox is LearningTextEntryBox)
                {
                    KeyCode target = (currentTextBox as LearningTextEntryBox).currentTarget;
                    if (myPolygon.polygons.ContainsKey(target))
                    {
                        myPolygon.StepLearning(target, pos);
                        Manager.Inst.SaveKeyboard();
                    }
                    currentTextBox.ProcessKeyCode(key);
                }
            }
            text.text = "";
        }

        if (previousKeyBool != pointed || previousKeyCode != key)
        {
            hapticAction.Execute(0, 0.01f, 100, 100, input);
        }
        previousKeyBool = pointed;
        previousKeyCode = key;

        float camHandAngle = Vector3.Angle(cam.forward, transform.position - cam.position);
        float alpha = 1f - 1f * Mathf.Min(1, camHandAngle / 45f);
        int idx = 0;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(keyLineColor, 0f), new GradientColorKey(keyLineColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) });
        foreach (KeyCode k in myPolygon.polygons.Keys)
        {
            List<Vector3> positions = new List<Vector3>();
            Func<Vector3, Vector3> trnsDir = (x) =>
            {
                Vector3 v = x * Mathf.Deg2Rad;
                Vector3 temp = new Vector3(keyLineRadius * Mathf.Sin(v.x) * Mathf.Cos(v.y), keyLineRadius * Mathf.Sin(v.y), keyLineRadius * Mathf.Cos(v.x) * Mathf.Cos(v.y));
                temp = currentTextDir.TransformDirection(temp) + transform.position;
                return temp;
            };
            foreach (int i in myPolygon.polygons[k])
            {
                Vector3 v = myPolygon.vertices[i];
                positions.Add(trnsDir(v));
            }
            keyLines[idx].positionCount = positions.Count;
            keyLines[idx].numCornerVertices = positions.Count;
            keyLines[idx].SetPositions(positions.ToArray());
            keyLines[idx].colorGradient = gradient;

            Color textColor = keyTextColor;
            textColor.a = alpha;
            keyTexts[idx].characterSize = keyTextSize / 100;
            keyTexts[idx].color = textColor;
            keyTexts[idx].transform.position = trnsDir(myPolygon.centroids[k]);
            keyTexts[idx].transform.LookAt(transform.position, Vector3.up);
            keyTexts[idx].text = Manager.Inst.keycodeStringDict[k].onBoard;
            idx++;
        }
    }
    private void Update()
    {
        if(entryState == EntryState.Input)
        {
            KeyboardInteraction();
        }
        else if(entryState == EntryState.Select)
        {
            if(grabButton.GetLastStateDown(input))
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

    private void SetVisualKeyboardActive(bool active)
    {
        if (keyLines == null) return;
        foreach (LineRenderer l in keyLines) l.enabled = active;
        foreach (TextMesh t in keyTexts) t.gameObject.SetActive(active);
    }
    private void SetDefaultKeyboard()
    {
        myPolygon = Manager.Inst.GetKeyboard(hand);
        keyLines = new LineRenderer[myPolygon.polygons.Count];
        keyTexts = new TextMesh[myPolygon.polygons.Count];

        for (int i = 0; i < keyLines.Length; i++)
        {
            keyLines[i] = Instantiate(keyLinePrefab, Manager.Inst.worldSpaceParent).GetComponent<LineRenderer>();
            keyTexts[i] = Instantiate(keyTextPrefab, Manager.Inst.worldSpaceParent).GetComponent<TextMesh>();
        }

        SetVisualKeyboardActive(false);
    }
    public void ChangeState(EntryState newState)
    {
        entryState = newState;
        if(entryState == EntryState.Select)
        {
            centerRay.gameObject.SetActive(false);
            forwardRay.gameObject.SetActive(false);
            Manager.Inst.entryExitTrigger[(int)hand] = false;
            SetVisualKeyboardActive(false);
            text.text = "";
            pointRay.enabled = true;
        }
        else if(entryState == EntryState.Input)
        {
            centerRay.gameObject.SetActive(true);
            forwardRay.gameObject.SetActive(true);
            SetVisualKeyboardActive(true);
            pointRay.enabled = false;
        }
    }

    public void SetCurrentTextBox(TextEntryBox textBox)
    {
        Vector3 tempVec;
        currentTextBox = textBox;
        if(currentTextDir == null)
        {
            currentTextDir = new GameObject().transform;
        }
        tempVec = cam.position;
        tempVec.y = currentTextBox.transform.position.y;
        currentTextDir.position = tempVec;
        currentTextDir.LookAt(currentTextBox.transform.position, Vector3.up);

        if (textBox is LearningTextEntryBox)
        {
            myPolygon.InitLearning();
        }
    }
}
