﻿using Newtonsoft.JsonCC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace WarpWorld.CrowdControl
{
    /// <summary> The Crowd Control client instance. Handles communications with the server and triggering effects. </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Crowd Control/Crowd Control Manager")]
    [RequireComponent(typeof(CCEffectEntries))]
    public sealed class CrowdControl : MonoBehaviour {
        #region Configuration 

        [SerializeField] private string _gameName = "Unity Demo";
        [SerializeField] private string _gameID = "UnityDemo";
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _startSessionAuto = true;
       
        private CCEffectEntries ccEffectEntries;

        [Space]

        [SerializeField] private short _reconnectRetryCount = -1;
        [SerializeField] private float _reconnectRetryDelay = 5;

        [Header("Visuals")]
#pragma warning disable 0649, 1591


#pragma warning disable 0169
        [SerializeField] private Sprite _tempUserIcon;
        [SerializeField] private Sprite _crowdUserIcon;
        [SerializeField] private Sprite _errorUserIcon;

        [SerializeField] private Color _tempUserColor;
        [SerializeField] private Color _crowdUserColor;
        [SerializeField] private Color _errorUserColor;
#pragma warning restore 0169
        [Range(0, 10)] [SerializeField] private float delayBetweenEffects = .5f;

        [Header("Debug Outputs")]
        [SerializeField] private bool _debugLog = true; 
        [SerializeField] private bool _debugWarning = true;
        [SerializeField] private bool _debugError = true;
        [SerializeField] private bool _debugExceptions = true;

        private SocketProvider _socketProvider;
        private ushort _port = 443;
        private string _token = "";
        private string _streamerID = "";
        private string _userHash = "";
        private bool _disconnectedFromDisable = false;

#pragma warning restore 0649, 1591

        #endregion

        #region State

        /// <summary>Singleton instance. Will be <see langword="null"/> if the behaviour isn't in the scene.</summary>
        public static CrowdControl instance { get; private set; }

        /// <summary>Reference to the test user object. Used to dispatch local effects.</summary>
        public static StreamUser testUser { get; private set; }

        /// <summary>Reference to the crowd user object. Used to dispatch pooled effects.</summary>
        public static StreamUser crowdUser { get; private set; }

        /// <summary>Reference to the crowd user object. Used to dispatch effects with an unknown contributor.</summary>
        public static StreamUser anonymousUser { get; private set; }

        /// <summary>Reference to the streamer user object.</summary>
        public static StreamUser basicUser { get; private set; }

        /// <summary>Unique ID Identifier for this game.</summary>
        public static string GameID { get { return instance._gameID; } }

        /// <summary>Start the game session as soon as your login is verified.</summary>
        public static bool StartSessionAutomatically { get; private set; }

        /// <summary>ID of the current game session.</summary>
        private string _gameSessionID;

        private Queue<CCEffectInstance> pendingQueue;
        private Dictionary<string, Queue<CCEffectInstanceTimed>> haltedTimers;
        private Dictionary<string, CCEffectInstanceTimed> runningEffects = new Dictionary<string, CCEffectInstanceTimed>(); // Timed effects currently running.
        private readonly Dictionary<string, StreamUser> streamUsers = new Dictionary<string, StreamUser>();
        private Dictionary<string, CCGeneric> generics = new Dictionary<string, CCGeneric>();
        private Dictionary<string, CCEffectBase> effectsByID = new Dictionary<string, CCEffectBase>();

        private float timeUntilNextEffect;
        private float timeUntilProcessJSON = 0.0f;
        private float timeToNextPing = float.MaxValue; // When to send the next ping message.
        private float timeToTimeout = float.MaxValue; // When to consider the server connection as timed out.

        private short _currentRetryCount;
        private bool _duplicatedInstance = false;
        private bool _paused = false;
        private bool _adjustPauseTime = false;
        private bool _disconnectFromTimeout = false;
        public bool ActiveSession { get; private set; } = false;
        private List<string> effectInstanceIDs = new List<string>();

        internal static Server CCServer { get; private set; } = Server.Production;
        public StreamUser Streamer { get; private set; } = null;

        internal static Queue<KeyValuePair<Action<string>, string>> PostGetResponses = new Queue<KeyValuePair<Action<string>, string>>();

        private string WebSocketServer {
            get {
                switch (CCServer) {
                    case Server.Dev:
                        return "ws://dev-pubsub.crowdcontrol.live";
                    case Server.Production:
                        return "wss://pubsub.crowdcontrol.live";
                    case Server.Staging:
                        return "wss://r8073rtqd8.execute-api.us-east-1.amazonaws.com/staging/";
                }

                return string.Empty;
            }
        }

        private string AuthURL {
            get {
                switch (CCServer) {
                    case Server.Dev:
                        return "https://dev-auth.crowdcontrol.live";
                    case Server.Production:
                        return "https://auth.crowdcontrol.live";
                    case Server.Staging:
                        return "https://beta-auth.crowdcontrol.live";
                }

                return string.Empty;
            }
        }

        private string MenuURL {
            get
            {
                switch (CCServer)
                {
                    case Server.Dev:
                        return "https://dev-extension.crowdcontrol.live/#/";
                    case Server.Production:
                        return "https://interact.crowdcontrol.live/#/";
                    case Server.Staging:
                        return "https://staging-extension.crowdcontrol.live/#/";
                }

                return string.Empty;
            }
        }

        internal string CurrentToken {
            get {
                if (string.IsNullOrEmpty(_token))
                    _token = PlayerPrefs.GetString($"CCToken{_gameID}{CCServer}", string.Empty);

                return _token;
            } set {
                PlayerPrefs.SetString($"CCToken{_gameID}{CCServer}", value);
                _token = value;
            }
        }

        private string CurrentStreamer {
            get {
                if (string.IsNullOrEmpty(_streamerID))
                    _streamerID = PlayerPrefs.GetString($"CCStreamer{_gameID}{CCServer}", string.Empty);

                return _streamerID;
            }
            set {
                PlayerPrefs.SetString($"CCStreamer{_gameID}{CCServer}", value);
                _streamerID = value;
            }
        }

        internal string CurrentUserHash {
            get {
                if (string.IsNullOrEmpty(_userHash))
                    _userHash = PlayerPrefs.GetString($"CCUserHash{_gameID}{CCServer}", string.Empty);

                return _userHash;
            }
            private set {
                PlayerPrefs.SetString($"CCUserHash{_gameID}{CCServer}", value);
                _userHash = value;
            }
        }

        /// <summary>Clears all saved tokens from a previous session.</summary>
        public void ClearSavedTokens() {
            ClearSavedTokensFromServer(CCServer);
        }

        private void ClearSavedTokensFromServer(Server targetServer) {
            PlayerPrefs.SetString($"CCToken{_gameID}{targetServer}", string.Empty);
            PlayerPrefs.SetString($"CCStreamer{_gameID}{targetServer}", string.Empty);
            PlayerPrefs.SetString($"CCUserHash{_gameID}{targetServer}", string.Empty);
            UnityEngine.Debug.Log("[CC] Cleared Saved Tokens on " + targetServer);
        }

        /// <summary>Whether the connection to the server is currently initializing.</summary>
        public bool isConnecting { get; private set; }

        /// <summary>Are you connected or not</summary>
        public bool isConnected => _socketProvider != null && _socketProvider.Connected;

        /// <summary>The latest disconnect occured due to an error.</summary>
        public bool disconnectedFromError { get; private set; }

        private string connectionID;
        private Queue<string> jsonQueue = new Queue<string>();

        #endregion

        #region Events

        /// <summary>Invoked when attempting a connection to the Crowd Control server.</summary>
        public event Action OnConnecting;
        /// <summary>Invoked when the connection to the Crowd Control server has failed.</summary>
        public event Action<Exception> OnConnectionError;
        /// <summary>Invoked when successfully connected to the Crowd Control server.</summary>
        public event Action OnConnected;
        /// <summary>Invoked when disconnected from the Crowd Control server.</summary>
        public event Action OnDisconnected;

        internal void RunConnectedAction() {
            OnConnected?.Invoke();
        }

        /// <summary>Invoked when an effect is scheduled for execution.</summary>
        public event Action<CCEffectInstance> OnEffectQueue;
        /// <summary>Invoked when an effect leaves the scheduling queue.</summary>
        public event Action<string, EffectResult> OnEffectDequeue;
        /// <summary>Invoked when an important message needs to be displayed.</summary>
        public event Action<string, float, Sprite> OnDisplayMessage;
        /// <summary>Invoked when you fail to login.</summary>
        public event Action OnLoginFailure;

        /// <summary>Invoked when an <see cref="CCEffectBase"/> is successfully triggered.</summary>
        public event Action<CCEffectInstance> OnEffectTrigger;
        /// <summary>Invoked when a <see cref="CCEffectTimed"/> is successfully started.</summary>
        public event Action<CCEffectInstanceTimed> OnEffectStart;
        /// <summary>Invoked when a <see cref="CCEffectTimed"/> is successfully stopped.</summary>
        public event Action<CCEffectInstanceTimed> OnEffectStop;
        /// <summary>Invoked on effect instances when the associated <see cref="CCEffectTimed"/> is disabled.</summary>
        public event Action<CCEffectInstanceTimed> OnEffectPause;
        /// <summary>Invoked on effect instances when the associated <see cref="CCEffectTimed"/> is enabled.</summary>
        public event Action<CCEffectInstanceTimed> OnEffectResume;
        /// <summary>Invoked on effect instances when the associated <see cref="CCEffectTimed"/> is reset.</summary>
        public event Action<CCEffectInstanceTimed> OnEffectReset;
        #endregion

        /// <summary>Invoked when you log out.</summary>
        public event Action OnLoggedOut; 
        /// <summary>Invoked when you log in.</summary>
        public event Action OnLoggedIn;
        /// <summary>Invoked when you subscribe to the server.</summary>
        public event Action OnSubscribed;
        /// <summary>Invoked when you fail to subscribe to the server.</summary>
        public event Action OnSubscribeFail;
        /// <summary>Invoked when the session starts.</summary>
        public event Action OnGameSessionStart;
        /// <summary>Invoked when the session stops.</summary>
        public event Action OnGameSessionStop;
        /// <summary>Invoked when an effect is requested.</summary>
        public event Action<CCEffectBase> OnEffectRequest;

        private Process wsProxy;
        private Process httpProxy;

        internal static bool sendingPost = false;

        #region Unity Component Life Cycle

        void Awake() {
            if (instance != null) {
                _duplicatedInstance = true;
                Destroy(gameObject);
                return;
            }

            if (_dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

            ccEffectEntries = gameObject.GetComponent<CCEffectEntries>();

            crowdUser = new StreamUser("The Crowd", _crowdUserIcon);
            anonymousUser = new StreamUser("Anonymous", _tempUserIcon);
            basicUser = new StreamUser("User", _tempUserIcon);

            streamUsers.Add(crowdUser.name, crowdUser);
            streamUsers.Add(anonymousUser.name, anonymousUser);
            streamUsers.Add(basicUser.name, basicUser);

            Assert.IsNull(instance);
            instance = this;

            pendingQueue = new Queue<CCEffectInstance>();
            jsonQueue = new Queue<string>();
            haltedTimers = new Dictionary<string, Queue<CCEffectInstanceTimed>>();
            OnConnected += () => SendJSON(new JSONMessageSend("whoami"));
        }

        void OnEnable() {
            if (!_disconnectedFromDisable || isConnecting)
                return;

            Connect();
            _disconnectedFromDisable = false;
        }

        void OnDisable() {
            
            if (_duplicatedInstance)
                return;

            StopAllCoroutines();
            StopAllEffects();
            Disconnect();
            _disconnectedFromDisable = true;
        }

        void OnDestroy() {
            return;
            if (_duplicatedInstance)
                return;

            if (ActiveSession)
                StopGameSession();

            if (_socketProvider != null) 
                _socketProvider.Dispose();

            Assert.IsNotNull(instance);
            instance = null;

            testUser = null;
            crowdUser = null;

            pendingQueue = null;
            runningEffects = null;
            haltedTimers = null;

            streamUsers.Clear();
            effectsByID.Clear();
        }

        void OnApplicationPause(bool paused) {
            _paused = paused;

            if (paused)
                _adjustPauseTime = true;
        }

        void Update() { // Handle connection timeout and reconnects.
            float now = Time.unscaledTime;

            if (_adjustPauseTime) {
                UpdateTimerEffectsFromIdle();

                if (!_paused) {
                    timeToNextPing = now + Protocol.PING_INTERVAL;
                    timeToTimeout = now + Protocol.PING_INTERVAL * 2;
                    _adjustPauseTime = false;
                }
            }

            if (_disconnectFromTimeout) {
                _disconnectFromTimeout = false;
                StartCoroutine(DisplayMessageWithIcon("The Crowd Control connection has timed out."));
                Disconnect(false);
                return;
            }

            if ((now >= timeToTimeout) && isConnected) {
                Disconnect(true);
            }
            else if (!isConnected && !isConnecting && now >= timeToNextPing && disconnectedFromError) {
                timeToNextPing = float.MaxValue;
                ConnectSocket();
            }

            UpdateTimerEffectStatuses();

            // Process effects.
            timeUntilNextEffect -= Time.unscaledDeltaTime;
            RunQueue(TryStop);

            timeUntilProcessJSON -= Time.unscaledDeltaTime;

            if (jsonQueue.Count > 0 && timeUntilProcessJSON <= 0.0f) {
                ProcessMessage(jsonQueue.Dequeue());
                timeUntilProcessJSON = 0.1f;
            } else if (PostGetResponses.Count > 0) {
                KeyValuePair<Action<string>, string> responseData = PostGetResponses.Dequeue();
                Log("RECEIVED: " + responseData.Value);
                timeUntilProcessJSON = 0.1f;
                responseData.Key?.Invoke(responseData.Value);
            }

            HandlePending();

            // Send messages to the server.
            if (isConnected) {
                if (Streamer != null && !streamUsers.ContainsKey(Streamer.name)) {
                    StartCoroutine(InstantiateViewer(Streamer, Streamer.name));
                }

                if (timeToNextPing <= now)  {
                    Assert.IsFalse(isConnecting);
                    SendJSON(new JSONData("ping"));
                    timeToNextPing = now + Protocol.PING_INTERVAL;
                    timeToTimeout = Time.unscaledTime + Protocol.PING_INTERVAL * 2;
                }
            }
        }

        #endregion

        #region Client

        private void HandlePending() {
            if (pendingQueue.Count == 0)
                return;

            CCEffectInstance currentPending = pendingQueue.Dequeue();

            if (IsRunning(currentPending)) {
                string id = currentPending.EffectID;
                if (!haltedTimers.ContainsKey(id))
                    haltedTimers.Add(id, new Queue<CCEffectInstanceTimed>());

                haltedTimers[id].Enqueue(currentPending as CCEffectInstanceTimed);
                return;
            }

            if (TryStart(currentPending))
                return;

            pendingQueue.Enqueue(currentPending);
        }

        private bool TryStart(CCEffectInstance effectInstance) {
            if (timeUntilNextEffect > 0)
                return false;

            var now = Time.unscaledTime;

            if (effectInstance.effect.delayUntilUnscaledTime > now)
                return false;

            return effectInstance.unscaledStartTime <= now && StartEffect(effectInstance);
        }

        private bool IsRunning(CCEffectInstance effectInstance) {
            if (!(effectInstance is CCEffectInstanceTimed))
                return false;

            return runningEffects.ContainsKey(effectInstance.EffectID);
        }

        private bool TryStop(CCEffectInstanceTimed effectInstance) {
            if (effectInstance.isPaused)
                return false;

            effectInstance.unscaledTimeLeft -= Time.unscaledDeltaTime;
            return effectInstance.unscaledTimeLeft <= 0 && StopEffect(effectInstance);
        }

        private void ConnectError() {
            if (_reconnectRetryCount != 0) {
                if (_reconnectRetryCount == -1 || ++_currentRetryCount < _reconnectRetryCount) {
                    isConnecting = true;
                    timeToNextPing = Time.unscaledTime + _reconnectRetryDelay;
                    return;
                }
            }

            isConnecting = false;
        }

        internal void SendJSON(object jsonContent) {
            Task.Factory.StartNew(() => _socketProvider.Send(JsonConvert.SerializeObject(jsonContent)));
        }

        /// <summary>Returns true if the timed effect is currently running.</summary>
        public bool IsRunning(string effectID) {
            return runningEffects.ContainsKey(effectID);
        }

        /// <summary>Gets the JSON Manifest of your effect pack.</summary>
        public string GetJSONManifest() {
            CCJsonBlock jsonBlock = new CCJsonBlock(_gameName, effectsByID);
            return jsonBlock.jsonString;
        }

        /// <summary>Connects to the Crowd Control server.</summary>
        public void Connect() {
            if (isConnected) {
                LogError("User is already connected.");
                return;
            }

            if (isConnecting)
                return;

            if ((_socketProvider != null && _socketProvider.webSocket != null) || isConnecting)
                throw new InvalidOperationException();

            _currentRetryCount = 0;
            timeToNextPing = float.MaxValue;
            _token = PlayerPrefs.GetString($"CCToken{_gameID}{CCServer}", string.Empty);

#if NET35
            if (wsProxy == null) {
                try {
                    wsProxy = new Process();

#if !UNITY_IOS && !UNITY_STANDALONE_OSX && !UNITY_STANDALONE_WIN
                    wsProxy.StartInfo.FileName = @"Assets\WarpWorld\Proxies\TlsWsProxy.exe";
#else
                    wsProxy.StartInfo.FileName = @"TlsWsProxy.exe";
#endif
                    wsProxy.StartInfo.Arguments = "-u " + WebSocketServer + " -p 24487";
                    wsProxy.StartInfo.UseShellExecute = false; // ensures that the process is started directly from the executable file
                    //wsProxy.StartInfo.RedirectStandardOutput = true; // enables you to programmatically capture the standard output
                    wsProxy.StartInfo.RedirectStandardError = true; // enables you to programmatically capture the error output
                    wsProxy.StartInfo.CreateNoWindow = true; // suppresses the display of the associated process window

                    wsProxy.Start();
                }
                catch (Exception ex) {
                    LogError($"An error occurred: {ex.Message}");
                }
            }

            if (httpProxy == null) {
                try {
                    httpProxy = new Process();

#if !UNITY_IOS && !UNITY_STANDALONE_OSX && !UNITY_STANDALONE_WIN
                    httpProxy.StartInfo.FileName = @"Assets\WarpWorld\Proxies\TlsHttpProxy.exe";
#else
                    httpProxy.StartInfo.FileName = @"TlsHttpProxy.exe";
#endif

                    httpProxy.StartInfo.Arguments = "-u " + ServerMessages.OpenApiURL + " -p 24488";
                    httpProxy.StartInfo.UseShellExecute = false; // ensures that the process is started directly from the executable file
                    //httpProxy.StartInfo.RedirectStandardOutput = true; // enables you to programmatically capture the standard output
                    httpProxy.StartInfo.RedirectStandardError = true; // enables you to programmatically capture the error output
                    httpProxy.StartInfo.CreateNoWindow = true; // suppresses the display of the associated process window

                    httpProxy.Start();
                }
                catch (Exception ex) {
                    LogError($"An error occurred: {ex.Message}");
                }
            }
#endif

            ConnectSocket();
        }

        private void DisconnectedSocket() {
            _disconnectFromTimeout = true;
        }

        private async void ConnectSocket() {
            isConnecting = true;
            OnConnecting?.Invoke();

            _socketProvider = new SocketProvider();
            
            //_socketProvider.OnDisconnected += DisconnectedSocket;
            await _socketProvider.Connect(WebSocketServer, _port);

            timeToNextPing = Time.unscaledTime + Protocol.PING_INTERVAL;
            timeToTimeout = timeToNextPing + Protocol.PING_INTERVAL;
        }

        /// <summary> Disconnects from the Crowd Control server. </summary>
        public void Disconnect() => Disconnect(false);

        internal void ConnectError(Exception e) {
            OnConnectionError?.Invoke(e);
        }

        internal void ContinueSession() {
            if (!string.IsNullOrEmpty(_gameSessionID)) {
                Subscribe();
                return;
            }

            WhoAmI();
        }

        private void WhoAmI()  {
            JSONMessageSend whoamI = new JSONMessageSend("whoami");
            SendJSON(whoamI);
        }

        private void Subscribe() {
            JSONSubscribe subscribe = new JSONSubscribe(CurrentUserHash);
            SendJSON(new JSONData("subscribe", JsonConvert.SerializeObject(subscribe)));
        }

        internal void RunDisconnectedAction() {
            OnDisconnected?.Invoke();
        }

        private void Disconnect(bool fromError) {
            Log("Disconnect");

            StopGameSession();
            OnDisconnected?.Invoke(); 
            isConnecting = false;
#if NET35
            if (wsProxy != null) 
                wsProxy.CloseMainWindow();

            if (httpProxy != null) 
                httpProxy.CloseMainWindow();
#endif

            if (_socketProvider != null && _socketProvider.Connected) {
                _socketProvider.Close();
                _socketProvider.Dispose();
                _socketProvider = null;
            }

            /*
            if (fromError) {
                ConnectError();
                timeToNextPing = Time.unscaledTime + Protocol.PING_INTERVAL;
            }
            else {
                timeToNextPing = float.MaxValue;
            }
            
            timeToTimeout = float.MaxValue;
            isConnecting = false;
            OnDisconnected?.Invoke();
            disconnectedFromError = fromError;*/
        }

        /// <summary>Returns true if the effect is a bid war. </summary>
        public bool EffectIsBidWar(string effectID) {
            return effectsByID.ContainsKey(effectID) && (effectsByID[effectID] is CCEffectBidWar);
        }

        /// <summary>Returns true if the effect is a timed effect. </summary>
        public bool EffectIsTimedEffect(string effectID) {
            return effectsByID.ContainsKey(effectID) && (effectsByID[effectID] is CCEffectTimed);
        }

        /// <summary>Returns true if the effect is a parameter effect. </summary>
        public bool EffectIsParameterEffect(string effectID) {
            return effectsByID.ContainsKey(effectID) && (effectsByID[effectID] is CCEffectParameters);
        }

        /// <summary> Check if the effect is registered already or not. </summary>
        public bool EffectIsRegistered(CCEffectBase effectBase) {
            return effectsByID.ContainsKey(effectBase.ID);
        }

        /// <summary> Register a Generic (Not Implemented Yet). </summary>
        public void RegisterGeneric(CCGeneric generic) {
            generics.Add(generic.Name, generic);
        }

        /// <summary> Registers this effect during runtime. </summary>
        internal void RegisterEffect(CCEffectBase effectBase, bool silent = false) {
            ccEffectEntries.PrivateAddEffect(effectBase);

            if (!effectsByID.ContainsKey(effectBase.ID)) {
                if (effectBase is CCEffectParameters)
                    (effectBase as CCEffectParameters).RegisterParameters(ccEffectEntries);

                effectsByID.Add(effectBase.ID, effectBase);
            }

            if (!silent) 
                Log("Registered Effect ID " + effectBase.ID);
        }

        private IEnumerator DisplayMessageWithIcon(string message, float displayTime = 5.0f) {
            yield return new WaitUntil(() => Application.isPlaying);
            yield return new WaitUntil(() => Streamer != null && Streamer.profileIcon != null);
            OnDisplayMessage?.Invoke(message, displayTime, Streamer.profileIcon);
        }

        /// <summary>Bring up the menu in a web browser.</summary>
        public void BringUpMenu() {
            if (Streamer == null) {
                return;
            }

            string url = string.Format("{0}{1}/{2}", MenuURL, Streamer.originSite, Streamer.originID);
            Application.OpenURL(url);
        }

        private void LoginPlatform(string platform) {
            string url = string.Format("{0}?platform={1}&connectionID={2}", AuthURL, platform, connectionID);
            Application.OpenURL(url);
        }

        /// <summary>Attempt to login with Twitch.</summary>
        public void LoginTwitch() {
            LoginPlatform("twitch");
        }

        /// <summary>Attempt to login with Youtube.</summary>
        public void LoginYoutube() {
            LoginPlatform("youtube");
        }

        /// <summary>Attempt to login with Discord.</summary>
        public void LoginDiscord() {
            LoginPlatform("discord");
        }

        /// <summary>Begins the game session.</summary>
        public void StartGameSession() {
            if (sendingPost)
                return;

            ServerMessages.SendPost("start", StartGameSessionProcess, new JSONStartSession(_gameID));
        }

        /// <summary>Ends the game session.</summary>
        public void StopGameSession() {
            if (string.IsNullOrEmpty(_gameSessionID))
                return;

            ServerMessages.SendPost("stop", StopGameSessionProcess, new JSONStopSession(_gameSessionID));
        }

        private void RequestUser() {
            ServerMessages.RequestGet("user/profile", StreamerProfileProcess);
        }

        private void StartGameSessionProcess(string serializedPayload) {
            JSONGameSession gameSessionStart = JsonConvert.DeserializeObject<JSONGameSession>(serializedPayload);

            _gameSessionID = gameSessionStart.m_gameSessionID;
            OnGameSessionStart?.Invoke();
            ActiveSession = true;

            StartCoroutine(DisplayMessageWithIcon("Began the Crowd Control session!"));
        }

        private void StopGameSessionProcess(string serializedPayload) {
            if (!ActiveSession)
                return;

            OnGameSessionStop?.Invoke();
            ActiveSession = false;

            if (isActiveAndEnabled)
                StartCoroutine(DisplayMessageWithIcon("Stopped the Crowd Control session!"));
        }

        private void EffectRequestProcess(string serializedPayload) {
            JSONEffectRequest effectRequest = JsonConvert.DeserializeObject<JSONEffectRequest>(serializedPayload);
            CCEffectBase effect = effectsByID[effectRequest.m_effectRequest.m_effect.m_effectID];
            QueueEffect(effect, effectRequest.m_effectRequest.m_requester, effectRequest.m_effectRequest.m_requestID, effectRequest.m_effectRequest.m_isTest, effectRequest.m_effectRequest.m_parameters);
            OnEffectRequest?.Invoke(effect);
        }

        private void StreamerProfileProcess(string serializedPayload) {
            JSONUserInfo userInfo = JsonConvert.DeserializeObject<JSONUserInfo>(serializedPayload);
            Streamer = new StreamUser(userInfo.m_profile);
        }

        private void ProcessWhoAmI() {
            if (string.IsNullOrEmpty(CurrentToken)) {
                OnLoggedOut?.Invoke();
                return;
            }

            Subscribe();
            OnLoggedIn?.Invoke();
        }

        private void ProcessMessage(string jsonString) {
            if (string.Equals(jsonString, "pong"))
                return;

            JSONMessageGet jsonMessage = JsonConvert.DeserializeObject<JSONMessageGet>(jsonString);
            string serializedPayload = JsonConvert.SerializeObject(jsonMessage.m_payload);

            switch (jsonMessage.m_type) {
                case "whoami":
                    JSONWhoAmI whoAmI = JsonConvert.DeserializeObject<JSONWhoAmI>(serializedPayload);
                    connectionID = whoAmI.m_connectionID;

                    ProcessWhoAmI();
                    break; 
                case "login-success":
                    JSONLoginSuccess loginSuccess = JsonConvert.DeserializeObject<JSONLoginSuccess>(serializedPayload);
                    string userContents = loginSuccess.DecodeToken();
                    Streamer user = JsonConvert.DeserializeObject<Streamer>(userContents);

                    CurrentToken = user.m_jti;
                    CurrentStreamer = user.m_ccUID;
                    CurrentUserHash = loginSuccess.m_token;

                    Subscribe();
                    OnLoggedIn?.Invoke();
                    break;
                case "subscription-result":
                    JSONSubResult subResult = JsonConvert.DeserializeObject<JSONSubResult>(serializedPayload);
                    
                    if (subResult.m_success.Length == 0) {
                        OnSubscribeFail?.Invoke();
                        return;
                    }

                    RequestUser();
                    
                    OnSubscribed?.Invoke();

                    if (_startSessionAuto)
                        StartGameSession();

                    break;
                case "effect-request":
                    JSONEffectRequest.JSONEffectBody effectRequest = JsonConvert.DeserializeObject<JSONEffectRequest.JSONEffectBody>(serializedPayload);

                    if (effectInstanceIDs.Contains(effectRequest.m_effect.m_effectID))
                        return;

                    if (effectRequest.m_parameters == null && effectRequest.m_quantity > 0) {
                        effectRequest.m_parameters = new Dictionary<string, JSONEffectRequest.JSONParameterEntry>();
                        effectRequest.m_parameters.Add("quantity", new JSONEffectRequest.JSONParameterEntry());
                        effectRequest.m_parameters["quantity"].m_value = effectRequest.m_quantity.ToString();
                    }

                    CCEffectBase effect = effectsByID[effectRequest.m_effect.m_effectID];
                    QueueEffect(effect, effectRequest.m_requester, effectRequest.m_requestID, effectRequest.m_isTest, effectRequest.m_parameters);
                    OnEffectRequest?.Invoke(effect);
                    break;
            }
        }

#endregion

#region Effect Handlings

        /// <summary>Test a generic being sent to the server. (Not Implemented Yet)</summary>
        public void SendGenericTest(CCGeneric generic) {
            
        }

        /// <summary>Test a generic locally. (Not Implemented Yet)</summary>
        public void GetGenericTest(CCGeneric generic) {
            
        }

        /// <summary>Test an effect locally. Its events won't be sent to the server.</summary>
        public void TestEffect(CCEffectBase effect) {
            Dictionary<string, JSONEffectRequest.JSONParameterEntry> parameters = null;

            if (effect is CCEffectParameters) {
                CCEffectParameters paramEffect = effect as CCEffectParameters;
                parameters = new Dictionary<string, JSONEffectRequest.JSONParameterEntry>();

                foreach (string paramID in paramEffect.ParameterEntries.Keys) {
                    parameters.Add(paramEffect.ParameterEntries[paramID].Name, new JSONEffectRequest.JSONParameterEntry());
                    parameters[paramEffect.ParameterEntries[paramID].Name].m_name = paramEffect.ParameterEntries[paramID].testParamName;
                    parameters[paramEffect.ParameterEntries[paramID].Name].m_value = paramEffect.ParameterEntries[paramID].testParamName;
                }
            }

            StartCoroutine(DownloadUserInfo(effect, null, (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds.ToString(), true, parameters));
        }

        /// <summary>Test an effect remotely.</summary>
        public void TestEffectRemotely(CCEffectBase effect) {
            ServerMessages.SendPost("effect-request", EffectRequestProcess, new JSONRequestEffect(_gameSessionID, effect.ID));
        }

        /// <summary>Add to the JSON Processing Queue</summary>
        public void AddToJsonQueue(string json) {
            jsonQueue.Enqueue(json);
        }

        // Allocates an effect instance and add it to the pending list.
        private void QueueEffect(CCEffectBase effect, JSONEffectRequest.JSONUser request, string requestID, bool test, Dictionary<string, JSONEffectRequest.JSONParameterEntry> parameters = null) {
            Assert.IsTrue(isActiveAndEnabled);
            StartCoroutine(DownloadUserInfo(effect, request, requestID, test, parameters));
        }

        private IEnumerator InstantiateViewer(StreamUser displayUser, string userName) {
            streamUsers.Add(userName, displayUser);
            yield return StartCoroutine(displayUser.DownloadSprite());
        } 

        private IEnumerator DownloadUserInfo(CCEffectBase effect, JSONEffectRequest.JSONUser request, string requestID, bool test, Dictionary<string, JSONEffectRequest.JSONParameterEntry> parameters = null) {
            StreamUser displayUser = null;

            if (!test) {
                string userName = request.m_name;

                if (!streamUsers.ContainsKey(userName)) {
                    displayUser = new StreamUser(request);
                    yield return StartCoroutine(InstantiateViewer(displayUser, userName));
                } else {
                    displayUser = streamUsers[userName];
                }
            } else {
                displayUser = anonymousUser;
            }

            if (effect is CCEffectTimed)
                CreateEffectInstance<CCEffectInstanceTimed>(displayUser, effect as CCEffectTimed, request, requestID, parameters);
            else if (effect is CCEffectParameters)
                CreateEffectInstance<CCEffectInstanceParameters>(displayUser, effect as CCEffectParameters, request, requestID, parameters);
            else if (effect is CCEffectBidWar)
                CreateEffectInstance<CCEffectInstanceBidWar>(displayUser, effect as CCEffectBidWar, request, requestID, parameters);
            else
                CreateEffectInstance<CCEffectInstance>(displayUser, effect, request, requestID, parameters);
        }

        private void CreateEffectInstance<T>(StreamUser user, CCEffectBase effect, JSONEffectRequest.JSONUser request, string requestID, Dictionary<string, JSONEffectRequest.JSONParameterEntry> parameters = null) where T : CCEffectInstance, new() {
            T effectInstance = new T();

            effectInstance.id = requestID;
            effectInstance.user = user;
            effectInstance.effect = effect;
            effectInstance.retryCount = 0;
            effectInstance.unscaledStartTime = Time.unscaledTime; // TODO add some delay?

            string effectID = effect.ID;
            CCEffectEntry effectEntry = ccEffectEntries[effectID];

            if (effectsByID[effectID] is CCEffectParameters) {
                if (parameters == null) {
                    CancelEffect(effectInstance);
                    return;
                }

                CCEffectInstanceParameters paramsInstance = effectInstance as CCEffectInstanceParameters;
                paramsInstance.AssignParameters(parameters);
            }

            else if (effectsByID[effectID] is CCEffectBidWar) {
                // Implement when Bid Wars are ready.
            }

            if (effectInstanceIDs.Contains(requestID))
                return;

            if (effectInstanceIDs.Count > 10)
                effectInstanceIDs.RemoveAt(0);

            effectInstanceIDs.Add(requestID);
            pendingQueue.Enqueue(effectInstance);
            OnEffectQueue?.Invoke(effectInstance);
        }

        private IEnumerator ResetTimedEffect(CCEffectInstanceTimed oldTimedEffect, CCEffectInstanceTimed newTimedEffect) {
            StopTimedEffect(oldTimedEffect);
            yield return new WaitForSeconds(0.1f);
            newTimedEffect.effect = oldTimedEffect.effect;
            newTimedEffect.unscaledTimeLeft = newTimedEffect.effect.Duration;
            StartEffect(newTimedEffect);
        }

        private void StopTimedEffect(CCEffectInstanceTimed effectInstance) {
            effectInstance.effect.OnStopEffect(effectInstance, true);
            OnEffectStop?.Invoke(effectInstance);
            RPC.TimedEnd(effectInstance);
        }

        private void DequeueEffectInstance(CCEffectInstance effectInstance, EffectResult result) {
            OnEffectDequeue?.Invoke(effectInstance.EffectID, result);
        }

        private bool StartEffect(CCEffectInstance effectInstance) {
            EffectResult result;
            bool dequeue = true;
            CCEffectBase effect = effectInstance.effect;

            CCEffectInstanceTimed timedEffectInstance = effectInstance as CCEffectInstanceTimed;

            if (timedEffectInstance != null) {
                timedEffectInstance.effect = effect as CCEffectTimed;

                if (timedEffectInstance != null && !timedEffectInstance.isActive) {
                    result = EffectResult.Retry;
                    RetryStartEffect(effect, effectInstance, ref dequeue, ref result);
                    FinishStartEffect(effect, effectInstance, dequeue);
                    return dequeue;
                }
            }

            result = effect.CanBeRan() ? effect.OnTriggerEffect(effectInstance) : EffectResult.Retry;

            Assert.AreEqual(effectInstance.effect, effect);

            switch (result) {
                default:
                    LogErrorFormat("Unhandled EffectResult.{0}", result);
                    break;
                case EffectResult.Failure:
                    DequeueEffectInstance(effectInstance, result);
                    RPC.FailTemporarly(effectInstance);
                    break;
                case EffectResult.Unavailable:
                    DequeueEffectInstance(effectInstance, result);
                    RPC.FailPermanently(effectInstance);
                    break;
                case EffectResult.Success:
                    timeUntilNextEffect = delayBetweenEffects;
                    DequeueEffectInstance(effectInstance, EffectResult.Success);
                    OnEffectTrigger?.Invoke(effectInstance);
                    RPC.Success(effectInstance);
                    break;
                case EffectResult.Running:
                    Assert.IsNotNull(timedEffectInstance);
                    timeUntilNextEffect = delayBetweenEffects;
                    runningEffects.Add(effectInstance.EffectID, timedEffectInstance);

                    DequeueEffectInstance(effectInstance, EffectResult.Success);
                    OnEffectStart?.Invoke(timedEffectInstance);
                    RPC.Success(effectInstance);
                    RPC.TimedBegin(timedEffectInstance);
                    break;
                case EffectResult.Queue:
                    Assert.IsNotNull(timedEffectInstance);
                    break;
                case EffectResult.Retry:
                    RetryStartEffect(effect, effectInstance, ref dequeue, ref result);
                    FinishStartEffect(effect, effectInstance, dequeue);
                    return dequeue;
            }

            FinishStartEffect(effect, effectInstance, dequeue);
            return dequeue;
        }

        private void RetryStartEffect(CCEffectBase effect, CCEffectInstance effectInstance, ref bool dequeue, ref EffectResult result) {
            effectInstance.retryCount++;

            if (effectInstance.retryCount > effect.MaxRetries) {
                result = EffectResult.Failure;
                CancelEffect(effectInstance);
            }
            else {
                effectInstance.unscaledStartTime = effect.RetryDelay + Time.unscaledTime;
                dequeue = false;
            }
        }

        private void FinishStartEffect(CCEffectBase effect, CCEffectInstance effectInstance, bool dequeue) {
            if (dequeue)
                effect.delayUntilUnscaledTime = effect.PendingDelay + Time.unscaledTime;
            else
                effect.delayUntilUnscaledTime = effectInstance.unscaledStartTime;
        }

        // Process an effect instance in the running list.
        private bool StopEffect(CCEffectInstanceTimed effectInstance) {
            Assert.IsNotNull(effectInstance);

            string id = effectInstance.EffectID;

            effectInstance.effect.OnStopEffect(effectInstance, true);
            OnEffectStop?.Invoke(effectInstance);
            RPC.TimedEnd(effectInstance);
            runningEffects.Remove(id);
            effectInstance = null;

            if (haltedTimers.ContainsKey(id) && haltedTimers[id].Count > 0) 
                StartEffect(haltedTimers[id].Dequeue());

            return true;
        }

        /// <summary>Cancels a received effect</summary>
        private void CancelEffect(CCEffectInstance effectInstance) {
            RPC.FailTemporarly(effectInstance);
            DequeueEffectInstance(effectInstance, EffectResult.Failure);
        }

        /// <summary>Forcefully terminates all pending and running effects.</summary>
        public void StopAllEffects() {
            foreach (string queueID in haltedTimers.Keys) {
                while (haltedTimers[queueID].Count > 0) {
                    OnEffectDequeue?.Invoke(queueID, EffectResult.Failure);
                    haltedTimers[queueID].Dequeue();
                }
            }

            haltedTimers.Clear();

            foreach (CCEffectInstance instance in pendingQueue) // Cancel the rest of the pending effects
                CancelEffect(instance);

            pendingQueue.Clear();

            List<CCEffectInstance> effectList = new List<CCEffectInstance>();

            foreach (CCEffectInstanceTimed instance in runningEffects.Values) // Stop all running timers
                effectList.Add(instance);

            foreach (CCEffectInstanceTimed effect in effectList)
                StopEffect(effect);

            runningEffects.Clear();
            effectInstanceIDs = new List<string>();
        }

#endregion

#region Queue Utilities

        private void RunQueue(Func<CCEffectInstanceTimed, bool> action) {
            foreach (CCEffectInstanceTimed instance in runningEffects.Values) {
                if (action(instance))
                    return;
            }
        }

#endregion

#region Pause/Resume Timed Effects
        /// <summary>Resume a timed effect.</summary>
        public static void ResumeEffect(CCEffectTimed effect, bool fromMenu = false) {
            if (instance == null)
                return;

            if (!instance.runningEffects.TryGetValue(effect.ID, out CCEffectInstanceTimed effectInstance))
                return;

            if (!effectInstance.isPaused)
                return;

            if (fromMenu)
                effectInstance.effect.ResumeFromMenu(effectInstance);

            effectInstance.effect.Resume(effectInstance);
            effectInstance.effect.OnResumeEffect();
            instance.OnEffectResume?.Invoke(effectInstance);
            RPC.TimedResume(effectInstance);
        }

        /// <summary>Pause a timed effect.</summary>
        public static void PauseEffect(CCEffectTimed effect, bool fromMenu = false) {
            if (!instance.runningEffects.TryGetValue(effect.ID, out CCEffectInstanceTimed effectInstance))
                return;

            if (effectInstance.isPaused)
                return;

            if (fromMenu)
                effectInstance.effect.PauseFromMenu(effectInstance);

            effectInstance.effect.Pause(effectInstance);
            effectInstance.effect.OnPauseEffect();
            instance.OnEffectPause?.Invoke(effectInstance);
            RPC.TimedPause(effectInstance);
        }

        /// <summary>Reset a timed effect.</summary>
        public static void ResetEffect(CCEffectTimed effect, bool fromMenu = false) {
            if (!instance.runningEffects.TryGetValue(effect.ID, out CCEffectInstanceTimed effectInstance))
                return;

            if (fromMenu)
                effectInstance.effect.ResetFromMenu(effectInstance);

            effectInstance.effect.OnResetEffect();
            instance.OnEffectReset?.Invoke(effectInstance);
            effectInstance.effect.Reset(effectInstance);
            effectInstance.effect.OnResetEffect();
            instance.OnEffectReset?.Invoke(effectInstance);
        }

        private static void UpdateTimerEffectsFromIdle() {
            if (instance == null || instance.runningEffects == null) 
                return;

            foreach (CCEffectInstanceTimed timedEffect in instance.runningEffects.Values) {
                if (!timedEffect.effect.ShouldBeRunning()) 
                    continue;

                if (!instance._paused) 
                    ResumeEffect(timedEffect.effect);
                else
                    PauseEffect(timedEffect.effect);
            }
        }

        private static void UpdateTimerEffectStatuses() {
            if (instance == null || instance.runningEffects == null || instance._paused) 
                return;

            foreach (CCEffectInstanceTimed timedEffect in instance.runningEffects.Values) {
                if (timedEffect.effect.ShouldBeRunning()) {
                    ResumeEffect(timedEffect.effect);
                }
                else {
                    PauseEffect(timedEffect.effect);
                }
            }
        }

#endregion

#region Querying & Actions

        /// <summary>Returns true if at least one timed effect is currently running.</summary>
        public bool HasRunningEffects() => runningEffects != null;
        /// <summary>Returns true if timed effect is running.</summary>
        public bool IsRunning(CCEffectTimed type) => RunFirst(type.ID, IsRunning);
        /// <summary>Stops a timer effect.</summary>
        public bool StopTimedEffect(CCEffectTimed type) => RunFirst(type.ID, StopEffect);
        /// <summary>Returns true if timed effect is paused.</summary>
        public bool IsPaused(CCEffectTimed type) => RunFirst(type.ID, IsPaused);

        private static bool IsRunning(CCEffectInstanceTimed effectInstance) => effectInstance.isActive;
        private static bool IsPaused(CCEffectInstanceTimed effectInstance) => effectInstance.isPaused;

        private bool RunFirst(string identifier, Func<CCEffectInstanceTimed, bool> action) {
            if (!runningEffects.ContainsKey(identifier))
                return false;

            return action(runningEffects[identifier]);
        }

#endregion

#region Debug

        public void LogFormat(string fmt, params object[] args) => Log(string.Format(fmt, args));
        public void LogWarningFormat(string fmt, params object[] args) => LogWarning(string.Format(fmt, args));
        public void LogErrorFormat(string fmt, params object[] args) => LogError(string.Format(fmt, args));

        private const string _ccPrefix = "[CC] ";

        public static void Log(object content) {
            if (instance != null && instance._debugLog)  {
                UnityEngine.Debug.Log(_ccPrefix + content.ToString());
            }
        }

        public static void LogWarning(object content) {
            if (instance != null && instance._debugWarning) {
                UnityEngine.Debug.LogWarning(_ccPrefix + content.ToString());
            }
        }

        public static void LogError(object content) {
            if (instance != null && instance._debugError) {
                UnityEngine.Debug.LogError(_ccPrefix + content.ToString());
            }
        }

        public static void LogException(Exception e) {
            if (instance != null && instance._debugExceptions) {
                UnityEngine.Debug.LogException(e);
            }
        }

#endregion

        const byte PROTOCOL_ENGINE = 0x11; // Unity3D

#if false
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX

#elif UNITY_STANDALONE_LINUX

#elif UNITY_STANDALONE_ANDROID

#elif UNITY_STANDALONE_IOS

#elif UNITY_STANDALONE_TVOS

#elif UNITY_PS4

#elif UNITY_XBOXONE

#elif UNITY_SWITCH

#elif UNITY_WEBGL

#elif UNITY_PS5

#else
#error "Unknown CrowdControl Platform"
#endif
#endif
    }
}
