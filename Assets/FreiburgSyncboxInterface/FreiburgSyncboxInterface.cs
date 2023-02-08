using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Purchasing.MiniJSON;

#if !UNITY_WEBGL // FreiburgSyncbox
using System.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
#endif // !UNITY_WEBGL

public abstract class IHostPCSyncbox : EventLoop
{
    public abstract string WaitForMessage(string type, int timeout);
    public abstract string WaitForMessages(string[] types, int timeout);
    public abstract void Connect(string ip, int port, string stimMode, string[] stimtags = null);
    public abstract void HandleMessage(string message, DateTime time);
    public abstract void SendMessage(string type, Dictionary<string, object> data);
    public abstract void SendMessageInternal(string type, Dictionary<string, object> data);
}

#if !UNITY_WEBGL // FreiburgSyncbox

public class FreiburgSyncboxListener {
    FreiburgSyncboxInterfaceHelper FreiburgSyncboxInterfaceHelper;
    Byte[] buffer; 
    const Int32 bufferSize = 2048;

    private volatile ManualResetEventSlim callbackWaitHandle;
    private ConcurrentQueue<string> queue = null;

    string messageBuffer = "";
    public FreiburgSyncboxListener(FreiburgSyncboxInterfaceHelper _FreiburgSyncboxInterfaceHelper) {
        FreiburgSyncboxInterfaceHelper = _FreiburgSyncboxInterfaceHelper;
        buffer = new Byte[bufferSize];
        callbackWaitHandle = new ManualResetEventSlim(true);
    }

    public bool IsListening() {
        return !callbackWaitHandle.IsSet;
    }

    public ManualResetEventSlim GetListenHandle() {
        return callbackWaitHandle;
    }

    public void StopListening() {
        if (IsListening())
            callbackWaitHandle.Set();
    }

    public void RegisterMessageQueue(ConcurrentQueue<string> messages) {
        queue = messages;
    }

    public void RemoveMessageQueue() {
        queue = null;
    }

    public void Listen() {
        if(IsListening()) {
            throw new AccessViolationException("Already Listening");
        }

        NetworkStream stream = FreiburgSyncboxInterfaceHelper.GetReadStream();
        callbackWaitHandle.Reset();
        stream.BeginRead(buffer, 0, bufferSize, Callback, 
                        new Tuple<NetworkStream, ManualResetEventSlim, ConcurrentQueue<string>>
                            (stream, callbackWaitHandle, queue));
    } 

    private void Callback(IAsyncResult ar) {
        NetworkStream stream;
        ConcurrentQueue<string> queue;
        ManualResetEventSlim handle;
        int bytesRead;

        Tuple<NetworkStream, ManualResetEventSlim, ConcurrentQueue<string>> state = (Tuple<NetworkStream, ManualResetEventSlim, ConcurrentQueue<string>>)ar.AsyncState;
        stream = state.Item1;
        handle = state.Item2;
        queue = state.Item3;

        bytesRead = stream.EndRead(ar);

        foreach(string msg in ParseBuffer(bytesRead)) {
            queue?.Enqueue(msg); // queue may be deleted by this point, if wait has ended
        }

        handle.Set();
    }
    
    private List<string> ParseBuffer(int bytesRead) {
        messageBuffer += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        List<string> received = new List<string>();

        UnityEngine.Debug.Log("ParseBuffer\n" + messageBuffer.ToString());
        while (messageBuffer.IndexOf("\n") != -1) {
            string message = messageBuffer.Substring(0, messageBuffer.IndexOf("\n") + 1);
            received.Add(message);
            messageBuffer = messageBuffer.Substring(messageBuffer.IndexOf("\n") + 1);
            
            FreiburgSyncboxInterfaceHelper.HandleMessage(message, System.DateTime.UtcNow);
        }

        return received;
    }
}

// NOTE: the gotcha here is avoiding deadlocks when there's an error
// message in the queue and some blocking wait in the EventLoop thread
public class FreiburgSyncboxInterfaceHelper : IHostPCSyncbox
{
    //public InterfaceManager im;

    int messageTimeout = 3000;
    int heartbeatTimeout = 8000; // TODO: configuration

    private TcpClient freiburgSyncboxServer;
    private FreiburgSyncboxListener listener;
    private int heartbeatCount = 0;

    private ScriptedEventReporter scriptedEventReporter;

    private bool interfaceDisabled = true;

    public FreiburgSyncboxInterfaceHelper(ScriptedEventReporter _scriptedEventReporter, bool _interfaceDisabled, string ip, int port, string stimMode, string[] stimTags = null) {
        //im = _im;

        interfaceDisabled = _interfaceDisabled;
        if (interfaceDisabled) return;

        scriptedEventReporter = _scriptedEventReporter;
        listener = new FreiburgSyncboxListener(this);
        Start();
        StartLoop();
        Connect(ip, port, stimMode, stimTags);
        //Do(new EventBase(Connect));
    }

    ~FreiburgSyncboxInterfaceHelper() {
        freiburgSyncboxServer.Close();
    }


    private NetworkStream GetWriteStream() {
        // TODO implement locking here
        if(freiburgSyncboxServer == null) {
            throw new InvalidOperationException("Socket not initialized.");
        }

        return freiburgSyncboxServer.GetStream();
    }

    // Should only be used by FreiburgSyncboxListener
    public NetworkStream GetReadStream() {
        // TODO implement locking here
        if(freiburgSyncboxServer == null) {
            throw new InvalidOperationException("Socket not initialized.");
        }

        return freiburgSyncboxServer.GetStream();
    }

    public override void Connect(string ip, int port, string stimMode, string[] stimTags = null) {
        if (interfaceDisabled) return;

        freiburgSyncboxServer = new TcpClient();

        //try {
        IAsyncResult result = freiburgSyncboxServer.BeginConnect(ip, port, null, null);
        result.AsyncWaitHandle.WaitOne(messageTimeout);
        freiburgSyncboxServer.EndConnect(result);
        //}
        //catch(SocketException) {    // TODO: set hostpc state on task side
        //    //im.Do(new EventBase<string>(im.SetHostPCStatus, "ERROR")); 
        //    throw new OperationCanceledException("Failed to Connect");
        //}

        //im.Do(new EventBase<string>(im.SetHostPCStatus, "INITIALIZING")); 



        SendMessageInternal("FNSBOPENUSB");
        WaitForMessage("FNSBOPENUSB_OK", messageTimeout);

        // excepts if there's an issue with latency, else returns
        DoLatencyCheck();

        DoRepeating(new RepeatingEvent(new EventBase(SyncPulse), -1, 0, 1000));
    }

    private void DoLatencyCheck() {
        // except if latency is unacceptable
        Stopwatch sw = new Stopwatch();
        float[] delay = new float[20];

        for(int i=0; i < 20; i++) {
            sw.Restart();
            Heartbeat();
            sw.Stop();

            delay[i] = sw.ElapsedTicks * (1000f / Stopwatch.Frequency);

            Thread.Sleep(50 - (int)delay[i]);
        }
        
        float max = delay.Max();
        float mean = delay.Sum() / delay.Length;
        float acc = (1000L*1000L*1000L) / Stopwatch.Frequency;

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("max_latency", max);
        dict.Add("mean_latency", mean);
        dict.Add("accuracy", acc);

        //im.Do(new EventBase<string, Dictionary<string, object>>(im.ReportEvent, "latency check", dict));
        scriptedEventReporter.ReportScriptedEvent("latency check", dict, true);
    }

    public override string WaitForMessage(string type, int timeout)
    {
        if (interfaceDisabled) return null;

        return WaitForMessages(new[] { type }, timeout);
    }

    // TODO: JPB: Make this a helper so that it calls a Do() instead (threading issue if you wait on another event)
    public override string WaitForMessages(string[] types, int timeout) {
        if (interfaceDisabled) return null;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        ManualResetEventSlim wait;
        int waitDuration;
        ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

        listener.RegisterMessageQueue(queue);
        while (sw.ElapsedMilliseconds < timeout) {
            listener.Listen();
            wait = listener.GetListenHandle();
            waitDuration = timeout - (int)sw.ElapsedMilliseconds;
            waitDuration = waitDuration > 0 ? waitDuration : 0;

            wait.Wait(waitDuration);

            string message;
            while (queue.TryDequeue(out message))
            {
                if (types.Contains(message.Trim('\n')))
                {
                    listener.RemoveMessageQueue();
                    return message;
                }
            }
        }

        sw.Stop();
        listener.StopListening();
        listener.RemoveMessageQueue();
        UnityEngine.Debug.Log("Wait for message(s) timed out\n" + String.Join(",", types));
        throw new TimeoutException("Timed out waiting for response");
    }

    public override void HandleMessage(string message, DateTime time) {
        if (interfaceDisabled) return;

        string type = message.Split()[0];

        JObject json = new JObject();
        json.Add("task pc time", time);
        json.Add("type", type);
        json.Add("data", message);
        ReportMessage(json.ToString(Newtonsoft.Json.Formatting.None), false);

        if (type == null)
        {
            throw new Exception("Message is missing \"type\" field: " + json.ToString());
        }

        if (type.Contains("ERROR") == true) {
            throw new Exception("Error received from Host PC.");
        }
        if(type == "EXIT") {
            return;
        }

        // // start listener if not running
        // if(!listener.IsListening()) {
        //     listener.Listen();
        // }
    }

    // data is unused in this function
    public override void SendMessageInternal(string type, Dictionary<string, object> data = null) {
        if (interfaceDisabled) return;

        if (data == null)
            data = new Dictionary<string, object>();

        string message = type;

        UnityEngine.Debug.Log("Sent Message");
        UnityEngine.Debug.Log(message + " " + data.toJson());

        Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message+"\n");

        NetworkStream stream = GetWriteStream();
        stream.Write(bytes, 0, bytes.Length);
        ReportMessage(message, true);
    }

    public override void SendMessage(string type, Dictionary<string, object> data = null)
    {
        if (interfaceDisabled) return;
        Do(new EventBase<string, Dictionary<string, object>>(SendMessageInternal, type, data));
    }

    private void ReportMessage(string message, bool sent)
    {
        Dictionary<string, object> messageDataDict = new Dictionary<string, object>();
        messageDataDict.Add("message", message);
        messageDataDict.Add("sent", sent.ToString());

        scriptedEventReporter.ReportScriptedEvent("network", messageDataDict);
        //im.Do(new EventBase<string, Dictionary<string, object>, DateTime>(im.ReportEvent, "network", 
        //                        messageDataDict, System.DateTime.UtcNow));

    }

    private void Heartbeat()
    {
        var data = new Dictionary<string, object>();
        data.Add("count", heartbeatCount);
        heartbeatCount++;
        SendMessageInternal("FNSBHEARTBEAT", data);
        WaitForMessage("FNSBHEARTBEAT_OK", heartbeatTimeout);
    }

    private void SyncPulse()
    {
        SendMessageInternal("FNSBSYNCPULSE");
    }
}

#else

public class FreiburgSyncboxInterfaceHelper : IHostPC
{
    public FreiburgSyncboxInterfaceHelper(ScriptedEventReporter _scriptedEventReporter, bool _interfaceDisabled, string ip, int port, string stimMode, string[] stimTags = null) {}

    public override JObject WaitForMessage(string type, int timeout) { return new JObject(); }
    public override JObject WaitForMessages(string[] types, int timeout) { return new JObject(); }
    public override void Connect(string ip, int port, string stimMode, string[] stimtags = null) { }
    public override void HandleMessage(string message, DateTime time) { }
    public override void SendMessage(string type, Dictionary<string, object> data) { }
    public override void SendMessageInternal(string type, Dictionary<string, object> data) { }
}

#endif // !UNITY_WEBGL

public class FreiburgSyncboxInterface : MonoBehaviour
{
    //This will be updated with warnings about the status of FreiburgSyncbox connectivity
    public UnityEngine.UI.Text FreiburgSyncboxWarningText;
    //This will be activated when a warning needs to be displayed
    public GameObject FreiburgSyncboxWarning;
    //This will be used to log messages
    public ScriptedEventReporter scriptedEventReporter;

    public FreiburgSyncboxInterfaceHelper freiburgSyncboxInterfaceHelper = null;

    private int switchCount = 0;
    public List<string> stimTags = null;

    // CONNECTED, CONFIGURE, READY, and HEARTBEAT
    public IEnumerator BeginNewSession(bool disableInterface, int port)
    {
        yield return null;
        freiburgSyncboxInterfaceHelper = new FreiburgSyncboxInterfaceHelper(scriptedEventReporter, disableInterface, "127.0.0.1", port, Config.elememStimMode);
        if (!disableInterface)
            UnityEngine.Debug.Log("Started FreiburgSyncbox Interface");
    }

}
