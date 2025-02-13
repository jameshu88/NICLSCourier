﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Luminosity.IO;

[AddComponentMenu("UnityEPL/Reporters/Input Reporter")]
public class InputReporter : DataReporter
{

    public bool reportKeyStrokes = false;
    public bool reportMouseClicks = false;
    public bool reportMousePosition = false;
    public int framesPerMousePositionReport = 60;
    private Dictionary<int, bool> keyDownStates = new Dictionary<int, bool>();
    private Dictionary<int, bool> mouseDownStates = new Dictionary<int, bool>();

    private int lastMousePositionReportFrame;

    // TODO: JPB: This is a hack and should be removed
    private ElememInterface elememInterface;

    void Update()
    {
        if (reportMouseClicks)
            CollectMouseEvents();
        if (reportKeyStrokes)
            CollectKeyEvents();
        if (reportMousePosition && Time.frameCount - lastMousePositionReportFrame > framesPerMousePositionReport)
            CollectMousePosition();
    }

    void CollectMouseEvents()
    {
        // #if !UNITY_WEBGL // Mac Application
        //     if (IsMacOS())
        //     {
        //         int eventCount = UnityEPL.CountMouseEvents();
        //         if (eventCount >= 1)
        //         {
        //             int mouseButton = UnityEPL.PopMouseButton();
        //             double timestamp = UnityEPL.PopMouseTimestamp();
        //             bool downState;
        //             mouseDownStates.TryGetValue(mouseButton, out downState);
        //             mouseDownStates[mouseButton] = !downState;
        //             ReportMouse(mouseButton, mouseDownStates[mouseButton], OSXTimestampToTimestamp(timestamp));
        //         }
        //     }
        // #endif
    }

    private void ReportMouse(int mouseButton, bool pressed, System.DateTime timestamp)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("key code", mouseButton);
        dataDict.Add("is pressed", pressed);
        string label = "mouse press/release";
        eventQueue.Enqueue(new DataPoint(label, timestamp, dataDict));

        #if !UNITY_WEBGL
        if (Config.elememOn)
        {
            if (elememInterface == null)
                elememInterface = GameObject.Find("ElememInterface").GetComponent<ElememInterface>();

            string type = label;
            string elemem_type = type.ToUpper();
            elemem_type = elemem_type.Replace(' ', '_');

            elememInterface.SendLogMessage(elemem_type, dataDict);
        }
        #endif
    }

    void CollectKeyEvents() // do we really need to separate out MacOS?
    {
        // if (IsMacOS())
        // {
        //     int eventCount = UnityEPL.CountKeyEvents();
        //     if (eventCount >= 1)
        //     {
        //         int keyCode = UnityEPL.PopKeyKeycode();
        //         double timestamp = UnityEPL.PopKeyTimestamp();
        //         bool downState;
        //         keyDownStates.TryGetValue(keyCode, out downState);
        //         keyDownStates[keyCode] = !downState;
        //         ReportKey(keyCode, keyDownStates[keyCode], OSXTimestampToTimestamp(timestamp));
        //     }
        // }
        // else
        // {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (InputManager.GetKeyDown(keyCode))
            {
                ReportKey((int)keyCode, true, DataReporter.RealWorldTime());
            }
            if (InputManager.GetKeyUp(keyCode))
            {
                ReportKey((int)keyCode, false, DataReporter.RealWorldTime());
            }
        }
        // }
    }

    private void ReportKey(int keyCode, bool pressed, System.DateTime timestamp)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("key code", keyCode);
        dataDict.Add("is pressed", pressed);
        string label = "key press/release";
        // if (!IsMacOS())
        //     label = "key/mouse press/release";
        eventQueue.Enqueue(new DataPoint(label, timestamp, dataDict));

        #if !UNITY_WEBGL
        if (Config.elememOn)
        {
            if (elememInterface == null)
                elememInterface = GameObject.Find("ElememInterface").GetComponent<ElememInterface>();

            string type = label;
            string elemem_type = type.ToUpper();
            elemem_type = elemem_type.Replace(' ', '_');

            elememInterface.SendLogMessage(elemem_type, dataDict);
        }
        #endif
    }

    void CollectMousePosition()
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("position", InputManager.mousePosition);
        string label = "mouse position";
        eventQueue.Enqueue(new DataPoint(label, DataReporter.RealWorldTime(), dataDict));
        lastMousePositionReportFrame = Time.frameCount;

        #if !UNITY_WEBGL
        if (Config.elememOn)
        {
            if (elememInterface == null)
                elememInterface = GameObject.Find("ElememInterface").GetComponent<ElememInterface>();

            string type = label;
            string elemem_type = type.ToUpper();
            elemem_type = elemem_type.Replace(' ', '_');

            elememInterface.SendLogMessage(elemem_type, dataDict);
        }
        #endif
    }
}