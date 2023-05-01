using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace WarpWorld.CrowdControl  
{
    /// <summary>
    /// The Crowd Control client instance. Handles communications with the server and triggering effects.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Crowd Control/Crowd Control Manager")]
    [RequireComponent(typeof(CCEffectEntries))]
    public sealed class CrowdControl : MonoBehaviour
    {
        #region Configuration

        [Tooltip("Name of the game")]
        [SerializeField] string _gameName = "Unity Demo";
        [Tooltip("Unique game key provided by Warp World.")]
        [SerializeField] string _gameKey;
        [Tooltip("Whether to use the Staging Server or production server.")]
        [SerializeField] bool _staging = false;
        [Tooltip("Don't destroy this game object when changing scenes.")]
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] public CCEffectEntries ccEffectEntries;
        [SerializeField] private BroadcasterType _broadcasterType;

        [Space]

        [Tooltip("How many times to attempt reconnecting? (-1 for unlimited)")]
        [SerializeField] private short _reconnectRetryCount = -1;
        [Tooltip("How many seconds to wait until trying to automatically reconnect again")]
        [SerializeField] private float _reconnectRetryDelay = 5;

        [Header("Visuals")]
#pragma warning disable 0649, 1591
        [Tooltip("User icon displayed while fetching the Twitch profile.")]
        [SerializeField] private Sprite _tempUserIcon;
        [Tooltip("User icon displayed for effects pooled by the crowd.")]
        [SerializeField] private Sprite _crowdUserIcon;

#pragma warning disable 0169
        [Tooltip("User icon displayed for profiles which failed to load.")] // Not done yet
        [SerializeField] private Sprite _errorUserIcon;
        [SerializeField] private Sprite _loadingIcon; // TODO used here? 
#pragma warning restore 0169

        [Tooltip("Background color for temporary user icons.")]
        [SerializeField] private Color _tempUserColor = new Color(0, 0, 0, .6f);
        [Tooltip("Background color for crowd user icons.")]
        [SerializeField] private Color _crowdUserColor = new Color(.094117f, .466666f, .937254f);
        [Tooltip("Background color for error user icons.")]
        [SerializeField] private Color _errorUserColor = new Color(.77647f, 0, 0);
        [Tooltip("Time to wait after triggering an effect before attempting to trigger another.")]
        [Range(0, 10)] public float delayBetweenEffects = .5f;

        [Header("Debug Outputs")]
        [SerializeField] bool _debugLog = true;
        [SerializeField] bool _debugWarning = true;
        [SerializeField] bool _debugError = true;
        [SerializeField] bool _debugExceptions = true;

        private SocketProvider _socketProvider;
        private ushort _port = 27442;
        private string _sslAddress = "gamesocket.crowdcontrol.live";
        private string _sslStagingAddress = "staging-gamesocket.crowdcontrol.live";
        private ulong _deviceFingerprint;
        private uint _blockID = 1;
        private string _token = "";
        private bool _disconnectedFromDisable = false;
        private System.Diagnostics.Stopwatch jsonStopwatch;

#pragma warning restore 0649, 1591

        #endregion

        #region State

        /// <summary>Singleton instance. Will be <see langword="null"/> if the behaviour isn't in the scene.</summary>
        public static CrowdControl instance { get; private set; }

        /// <summary>Reference to the test user object. Used to dispatch local effects.</summary>
        public static TwitchUser testUser { get; private set; }

        /// <summary>Reference to the crowd user object. Used to dispatch pooled effects.</summary>
        public static TwitchUser crowdUser { get; private set; }

        /// <summary>Reference to the crowd user object. Used to dispatch effects with an unknown contributor.</summary>
        public static TwitchUser anonymousUser { get; private set; }

        /// <summary>Reference to the streamer user object.</summary>
        public static TwitchUser streamerUser { get; private set; }

        private Queue<CCEffectInstance> pendingQueue;
        private Queue<PendingMessage> _pendingMessages = new Queue<PendingMessage>();
        private Dictionary<string, Queue<uint>> haltedTimers;
        private Dictionary<string, CCEffectInstanceTimed> runningEffects = new Dictionary<string, CCEffectInstanceTimed>(); // Timed effects currently running.
        private readonly Dictionary<string, TwitchUser> twitchUsers = new Dictionary<string, TwitchUser>();
        private Dictionary<string, CCGeneric> generics = new Dictionary<string, CCGeneric>();
        private Dictionary<string, CCEffectBase> effectsByID = new Dictionary<string, CCEffectBase>();

        private float timeUntilNextEffect;
        private float timeToNextPing = float.MaxValue; // When to send the next ping message.
        private float timeToTimeout = float.MaxValue; // When to consider the server connection as timed out.

        private byte[] _sendBuffer;
        private byte _recvSize;
        private ushort _sendSize;
        private short _currentRetryCount;
        private bool _duplicatedInstance = false;
        private bool _paused = false;
        private bool _adjustPauseTime = false;
        private bool _disconnectFromTimeout = false;

        public bool StagingServer { get { return _staging; } }

        public string CurrentToken { get { return PlayerPrefs.GetString($"CCToken{_gameKey}{_staging}", string.Empty); } }

        /// <summary>Did we start a session?</summary>
        public bool isAuthenticated { get; private set; }

        /// <summary>Whether the connection to the server is currently initializing.</summary>
        public bool isConnecting { get; private set; }

        /// <summary>Are you connected or not</summary>
        public bool isConnected => _socketProvider != null && _socketProvider.Connected;

        /// <summary>The latest disconnect occured due to an error.</summary>
        public bool disconnectedFromError { get; private set; }
        #endregion

        #region Events

        /// <summary>Invoked when attempting a connection to the Crowd Control server.</summary>
        public event Action OnConnecting;
        /// <summary>Invoked when the connection to the Crowd Control server has failed.</summary>
        public event Action<SocketError> OnConnectionError;
        /// <summary></summary>
        public event Action OnAuthenticated;
        /// <summary>Invoked when successfully connected to the Crowd Control server.</summary>
        public event Action OnConnected;
        /// <summary>Invoked when disconnected from the Crowd Control server.</summary>
        public event Action OnDisconnected;

        /// <summary>Invoked when an effect is scheduled for execution.</summary>
        public event Action<CCEffectInstance> OnEffectQueue;
        /// <summary>Invoked when an effect leaves the scheduling queue.</summary>
        public event Action<string, EffectResult> OnEffectDequeue;
        /// <summary>Invoked when an important message needs to be displayed.</summary>
        public event Action<string, float, Sprite> OnDisplayMessage;
        /// <summary>Invoked when the token input field is displayed.</summary>
        public event Action<bool> OnToggleTokenView;
        /// <summary>Invoked when you attempt to connect without any temporary token.</summary>
        public event Action OnNoToken;
        /// <summary>Invoked when you submit your temporary token.</summary>
        public event Action OnSubmitTempToken;
        /// <summary>Invoked when you attempt to connect with an incorrect temporary token.</summary>
        public event Action OnTempTokenFailure;

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

        #region Unity Component Life Cycle

        void Awake()
        {
            if (instance != null)
            {
                _duplicatedInstance = true;
                Destroy(gameObject);
                return;
            }

            if (_dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

            ccEffectEntries = gameObject.GetComponent<CCEffectEntries>();

            crowdUser = new TwitchUser
            {
                id = 1,
                name = "the_crowd",
                displayName = "The Crowd",
                profileIcon = _crowdUserIcon,
                profileIconColor = _crowdUserColor
            };

            anonymousUser = new TwitchUser
            {
                id = 2,
                name = "anonymous",
                displayName = "User",
                profileIcon = _tempUserIcon,
                profileIconColor = _tempUserColor
            };

            twitchUsers.Add(crowdUser.name, crowdUser);
            twitchUsers.Add(anonymousUser.name, anonymousUser);

            Assert.IsNull(instance);
            instance = this;

            pendingQueue = new Queue<CCEffectInstance>();
            haltedTimers = new Dictionary<string, Queue<uint>>();
            jsonStopwatch = new System.Diagnostics.Stopwatch();
        }

        void OnEnable()
        {
            if (!_disconnectedFromDisable || isConnecting)
            {
                return;
            }

            Connect();
            _disconnectedFromDisable = false;
        }

        void OnDisable()
        {
            if (_duplicatedInstance)
            {
                return;
            }

            StopAllCoroutines();
            StopAllEffects();
            Disconnect();
            _disconnectedFromDisable = true;
        }

        void OnDestroy()
        {
            if (_duplicatedInstance)
            {
                return;
            }

            //Disconnect();

            if (_socketProvider != null)
            {
                CCMessageDisconnect cCMessageDisconnect = new CCMessageDisconnect(_blockID++);
                _socketProvider.QuickSend(cCMessageDisconnect.ByteStream);
                _socketProvider.Dispose();
            }

            Assert.IsNotNull(instance);
            instance = null;

            testUser = null;
            crowdUser = null;

            pendingQueue = null;
            runningEffects = null;
            haltedTimers = null;

            twitchUsers.Clear();
            effectsByID.Clear();
        }

        void OnApplicationPause(bool paused)
        {
            _paused = paused;

            if (paused)
            {
                _adjustPauseTime = true;
            }
        }

        void Update()
        {
            // Handle connection timeout and reconnects.
            float now = Time.unscaledTime;

            if (_adjustPauseTime)
            {
                UpdateTimerEffectsFromIdle();

                if (!_paused)
                {
                    timeToNextPing = now + Protocol.PING_INTERVAL;
                    timeToTimeout = now + Protocol.PING_INTERVAL * 2;
                    _adjustPauseTime = false;
                }
            }

            if (_disconnectFromTimeout)
            {
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

            // Receive messages from the server.
            else if (isConnected)
            {
                try
                {
                    var received = 0;

                    if (received > 0)
                    {

                    }
                }
                catch (SocketException e)
                {
                    LogException(e);
                    Disconnect(true);
                }
            }

            UpdateTimerEffectStatuses();

            // Process effects.
            timeUntilNextEffect -= Time.unscaledDeltaTime;
            RunQueue(TryStop);

            HandlePending();

            if (jsonStopwatch.IsRunning && jsonStopwatch.Elapsed.TotalSeconds > 3) {
                SendJSONMenu();
                jsonStopwatch.Stop();
            }

            // Send messages to the server.
            if (isConnected)
            {
                if (_pendingMessages.Count > 0)
                {
                    ProcessMsg(_pendingMessages.Dequeue());
                }

                if (timeToNextPing <= now)
                {
                    Assert.IsFalse(isConnecting);
                    Send(new CCMessagePing(_blockID++));
                    timeToNextPing = now + Protocol.PING_INTERVAL;
                    timeToTimeout = Time.unscaledTime + Protocol.PING_INTERVAL * 2;
                }
            }
        }

        void ReceivedBytes(byte[] recv)
        {
            var offset = 0;
            var available = _recvSize + recv.Length;

            while (available >= Protocol.FRAME_SIZE)
            {
                var size = (recv[offset] << 8) | recv[offset + 1];

                if (available < size)
                {
                    // TODO receive more than recv.Length

                    if (_recvSize != 0)
                        Buffer.BlockCopy(recv, offset, recv, 0, available);

                    _recvSize = Convert.ToByte(available);

                    break;
                }

                _pendingMessages.Enqueue(new PendingMessage(Protocol.SplitByteArray(recv, offset, size), recv[offset + 2], size - Protocol.FRAME_SIZE));

                offset += size;
                available -= size;
            }
        }

        #endregion

        #region Client

        private void HandlePending()
        {
            if (pendingQueue.Count == 0)
                return;

            CCEffectInstance currentPending = pendingQueue.Dequeue();

            if (IsRunning(currentPending))
            {
                string id = currentPending.effectKey;
                if (!haltedTimers.ContainsKey(id))
                    haltedTimers.Add(id, new Queue<uint>());

                haltedTimers[id].Enqueue(currentPending.id);
                return;
            }

            if (TryStart(currentPending))
                return;

            pendingQueue.Enqueue(currentPending);
        }

        private bool TryStart(CCEffectInstance effectInstance)
        {
            if (timeUntilNextEffect > 0) return false;

            var now = Time.unscaledTime;
            if (effectInstance.effect.delayUntilUnscaledTime > now) return false;

            return effectInstance.unscaledStartTime <= now && StartEffect(effectInstance);
        }

        private bool IsRunning(CCEffectInstance effectInstance)
        {
            if (!(effectInstance is CCEffectInstanceTimed))
                return false;

            return runningEffects.ContainsKey(effectInstance.effectKey);
        }

        private bool TryStop(CCEffectInstanceTimed effectInstance)
        {
            if (effectInstance.isPaused)
                return false;

            effectInstance.unscaledTimeLeft -= Time.unscaledDeltaTime;
            return effectInstance.unscaledTimeLeft <= 0 && StopEffect(effectInstance, true);
        }

        private void ConnectError()
        {
            if (_reconnectRetryCount != 0)
            {
                if (_reconnectRetryCount == -1 || ++_currentRetryCount < _reconnectRetryCount)
                {
                    isConnecting = true;
                    timeToNextPing = Time.unscaledTime + _reconnectRetryDelay;
                    return;
                }
            }
            isConnecting = false;
        }

        private void ConnectError(Socket socket, SocketError error, Exception e)
        {
            socket.Close();
            OnConnectionError?.Invoke(error);
            LogException(e); // TODO
            ConnectError();
        }

        private void Send(CCRequest message)
        {
            Task.Factory.StartNew(() => _socketProvider.Send(message.ByteStream));
        }

        private async void DisconnectAndDisposeSocket(CCRequest message)
        {
            await _socketProvider.Send(message.ByteStream);
            _socketProvider.Dispose();
            _socketProvider = null;
        }

        /// <summary>
        /// Gets the JSON Manifest of your effect pack.
        /// </summary>
        public string GetJSONManifest() {
            CCJsonBlock jsonBlock = new CCJsonBlock(_gameName, effectsByID, ccEffectEntries);
            return jsonBlock.jsonString;
        }

        /// <summary>
        /// Connects to the Crowd Control server.
        /// </summary>
        public void Connect() {
            if (isConnected) {
                LogError("User is already connected.");
                return;
            }

            if ((_socketProvider != null && _socketProvider._socket != null) || isConnecting) throw new InvalidOperationException();

            _currentRetryCount = 0;
            timeToNextPing = float.MaxValue;
            _token = PlayerPrefs.GetString($"CCToken{_gameKey}{_staging}", string.Empty);

            ConnectSocket();
        }

        private void SendGenericTest(string key, Dictionary<string, string>[] parameters) {
            
        }

        private void DisconnectedSocket()
        {
            _disconnectFromTimeout = true;
        }

        private async void ConnectSocket()
        {
            isConnecting = true;
            OnConnecting?.Invoke();

            _socketProvider = new SocketProvider();
            _socketProvider.OnMessageReceived += ReceivedBytes;
            _socketProvider.OnDisconnected += DisconnectedSocket;
            await _socketProvider.Connect(_staging ? _sslStagingAddress : _sslAddress, _port);

            timeToNextPing = Time.unscaledTime + Protocol.PING_INTERVAL;
            timeToTimeout = timeToNextPing + Protocol.PING_INTERVAL;

            _deviceFingerprint = Utils.Randomulong();
            CCMessageVersion cCMessageVersion = new CCMessageVersion(_blockID++, Protocol.VERSION, _gameKey, _deviceFingerprint);
            Send(cCMessageVersion);
        }

        /// <summary>
        /// Disconnects from the Crowd Control server.
        /// </summary>
        public void Disconnect() => Disconnect(false);

        private void Disconnect(bool fromError)
        {
            Log("Disconnect");

            if (_socketProvider != null && _socketProvider._stream != null)
            {
                if (!fromError)
                {
                    CCMessageDisconnect cCMessageDisconnect = new CCMessageDisconnect(_blockID++);
                    DisconnectAndDisposeSocket(cCMessageDisconnect);
                }
                else
                {
                    _socketProvider.Dispose();
                    _socketProvider = null;
                }
            }
            if (fromError)
            {
                ConnectError();
                timeToNextPing = Time.unscaledTime + Protocol.PING_INTERVAL;
            }
            else
            {
                timeToNextPing = float.MaxValue;
            }

            timeToTimeout = float.MaxValue;
            isConnecting = false;
            OnDisconnected?.Invoke();
            isAuthenticated = false;
            disconnectedFromError = fromError;
        }

        public void UpdateEffect(string effectID, Protocol.EffectState effectState, uint callbackID = 0, ushort payload = 0)
        {
            if (effectState == Protocol.EffectState.AvailableForOrder || effectState == Protocol.EffectState.UnavailableForOrder ||
                effectState == Protocol.EffectState.VisibleOnMenu || effectState == Protocol.EffectState.HiddenOnMenu)
            {
                CCMessageEffectUpdate effectUpdate = new CCMessageEffectUpdate(_blockID++, effectID, effectState, payload);
                Send(effectUpdate);
                return;
            }

            CCMessageEffectRequest messageEffectRequest = new CCMessageEffectRequest(_blockID++, callbackID, effectState, payload);
            Send(messageEffectRequest);
        }

        private void UpdateEffect(CCEffectInstance instance, Protocol.EffectState effectState, ushort payload = 0)
        {
            if (instance.isTest)
                return;

            UpdateEffect(instance.effectKey, effectState, instance.id, payload);
        }

        private bool EffectIsBidWar(string effectID) {
            return effectsByID.ContainsKey(effectID) && (effectsByID[effectID] is CCEffectBidWar);
        }

        private void EffectSuccess(CCEffectInstance instance, byte delay = 0)
        {
            if (EffectIsBidWar(instance.effectKey))
                UpdateEffect(instance, Protocol.EffectState.BidWarSuccess);
            else
                UpdateEffect(instance, Protocol.EffectState.Success, delay);
        }

        private void EffectFailure(CCEffectInstance instance)
        {
            UpdateEffect(instance, EffectIsBidWar(instance.effectKey) ? Protocol.EffectState.BidWarFailure : Protocol.EffectState.TemporaryFailure);
        }

        private void EffectDelay(string effectID, byte delay = 5)
        {
            if (EffectIsBidWar(effectID))
                UpdateEffect(effectID, Protocol.EffectState.BidWarDelay);
            else
                UpdateEffect(effectID, Protocol.EffectState.ExactDelay);
        }

        private void SetTimedEffectState(CCEffectInstanceTimed instance, bool begin)
        {
            UpdateEffect(instance, begin ? Protocol.EffectState.TimedEffectBegin : Protocol.EffectState.TimedEffectEnd, Convert.ToUInt16(instance.effect.duration));
        }

        /// <summary> Check if the effect is registered already or not. </summary>
        public bool EffectIsRegistered(CCEffectBase effectBase) {
            return effectsByID.ContainsKey(effectBase.effectKey);
        }

        public void RegisterGeneric(CCGeneric generic) {
            generics.Add(generic.Name, generic);
        }

        public void ReRegisterEffects() {
            if (!string.IsNullOrEmpty(_gameKey)) {
                LogError("Re-Registering effects only works with the test game ID (92)");
                return;
            }

            ccEffectEntries.PrivateResetDictionary();

            foreach (string effectBaseID in effectsByID.Keys) { 
                RegisterEffect(effectsByID[effectBaseID], true); 
            }
        }

        /// <summary> Registers this effect during runtime. </summary>
        public void RegisterEffect(CCEffectBase effectBase, bool silent = false) {
            ccEffectEntries.PrivateAddEffect(effectBase);

            if (!effectsByID.ContainsKey(effectBase.effectKey)) {
                effectsByID.Add(effectBase.effectKey, effectBase);
                effectBase.RegisterParameters(ccEffectEntries);
            }

            if (!silent) {
                Log("Registered Effect ID " + effectBase.effectKey);
            }

            jsonStopwatch.Reset();
            jsonStopwatch.Start();
        }

        /// <summary> Toggles whether an effect can currently be sold during this session. </summary>
        public void ToggleEffectSellable(string effectID, bool sellable)
        {
            UpdateEffect(effectID, sellable ? Protocol.EffectState.AvailableForOrder : Protocol.EffectState.UnavailableForOrder);
        }

        /// <summary> Toggles whether an effect is visible in the menu during this session. </summary>
        public void ToggleEffectVisible(string effectID, bool visible)
        {
            UpdateEffect(effectID, visible ? Protocol.EffectState.VisibleOnMenu : Protocol.EffectState.HiddenOnMenu);
        }

        public void ClearSavedTokens() {
            PlayerPrefs.SetString($"CCToken{_gameKey}False", string.Empty);
            PlayerPrefs.SetString($"CCToken{_gameKey}True", string.Empty);

            Log("Saved Tokens Cleared.");
        }

        private void Send(byte value) => Protocol.Write(_sendBuffer, ref _sendSize, value);
        private void Send(ushort value) => Protocol.Write(_sendBuffer, ref _sendSize, value);
        private void Send(uint value) => Protocol.Write(_sendBuffer, ref _sendSize, value);
        private void Send(ulong value) => Protocol.Write(_sendBuffer, ref _sendSize, value);

        private void Send(UUID value) => value.Write(_sendBuffer, ref _sendSize);
        private void Send(string value) => Protocol.Write(_sendBuffer, ref _sendSize, value, value.Length);

        private void HelloMessage(byte[] bytes)
        {
            CCMessageVersion messageVersion = new CCMessageVersion(bytes);

            if (messageVersion.response == CCMessageVersion.ResponseType.Success)
            {
                if (string.IsNullOrEmpty(_token))
                {
                    OnToggleTokenView?.Invoke(true); // We don't have a token.
                    OnNoToken?.Invoke();
                    return;
                }

                UUID uuid = new UUID();
                uuid.FromString(_token);

                CCMessageTokenHandshake tokenHandshake = new CCMessageTokenHandshake(_blockID++, uuid);
                Send(tokenHandshake);
                OnConnected?.Invoke();
            }

            Log(messageVersion.response.ToString());
        }

        private void TokenAquisition(byte[] bytes)
        {
            CCMessageTokenAquisition getTokenMessage = new CCMessageTokenAquisition(bytes);

            if (getTokenMessage.greeting != Greeting.Success)
            {
                LogError(getTokenMessage.greeting.ToString());
                OnToggleTokenView?.Invoke(true);
                OnNoToken?.Invoke();
                return;
            }

            CCMessageTokenHandshake tokenHandshake = new CCMessageTokenHandshake(_blockID++, getTokenMessage.uniqueToken);
            Send(tokenHandshake);

            PlayerPrefs.SetString($"CCToken{_gameKey}{_staging}", getTokenMessage.uniqueToken.ToString());
        }

        /// <summary> Submits Temporary Token to the server. </summary>
        public void SubmitTempToken(string token)
        {
            OnToggleTokenView?.Invoke(false);
            OnSubmitTempToken?.Invoke();
            CCMessageTokenAquisition getTokenMessage = new CCMessageTokenAquisition(_blockID++, token);
            Send(getTokenMessage);
        }

        private void TokenHandshake(byte[] bytes)
        {
            CCMessageTokenHandshake tokenHandShake = new CCMessageTokenHandshake(bytes);

            if (tokenHandShake.greeting != Greeting.Success)
            {
                LogError(tokenHandShake.greeting.ToString());
                OnToggleTokenView?.Invoke(true);
                OnTempTokenFailure?.Invoke();
                return;
            }

            streamerUser = new TwitchUser
            {
                name = tokenHandShake.streamerName,
                displayName = tokenHandShake.streamerName,
                profileIconUrl = tokenHandShake.streamerIconURL
            };

            if (!twitchUsers.ContainsKey(streamerUser.name))
            {
                twitchUsers.Add(streamerUser.name, streamerUser);
            }

            _broadcasterType = tokenHandShake.broadcasterType;

            StartCoroutine(DisplayMessageWithIcon(tokenHandShake.streamerName + " started the Crowd Control Session!"));
            isAuthenticated = true;
            OnAuthenticated?.Invoke();

            jsonStopwatch.Reset();
            jsonStopwatch.Start();
        }

        public void SendJSONMenu() {
            if (!string.IsNullOrEmpty(_gameKey) || !isAuthenticated) // No Game
                return;

            CCJsonBlock jsonBlock = new CCJsonBlock(_gameName, effectsByID, ccEffectEntries);
            jsonBlock.CreateByteArray(_blockID++);
            Send(jsonBlock);

            /*Task.Factory.StartNew(() => _socketProvider.Send(jsonBlock.ByteStream)).ContinueWith(
                antecedent => {
                    jsonBlock.CreateByteArray(_blockID++, 1);
                    Send(jsonBlock);
                }
            );*/
        }

        private IEnumerator DownloadPlayerSprite(TwitchUser user)
        {
            user.profileIcon = _tempUserIcon;
            user.profileIconColor = _tempUserColor;

            if (string.IsNullOrEmpty(user.profileIconUrl))
                yield break;

            WWW www = new WWW(user.profileIconUrl);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
                user.profileIcon = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);
        }

        private IEnumerator DisplayMessageWithIcon(string message, float displayTime = 5.0f)
        {
            yield return new WaitUntil(() => Application.isPlaying);
            yield return StartCoroutine(DownloadPlayerSprite(streamerUser));
            OnDisplayMessage?.Invoke(message, displayTime, streamerUser.profileIcon);
        }

        private void BlockError(byte[] bytes)
        {
            CCMessageBlockError blockError = new CCMessageBlockError(bytes);
            LogError("Block: " + blockError.blockID);
        }

        private void BroadcastedMessage(byte[] bytes)
        {
            CCMessageUserMessage message = new CCMessageUserMessage(bytes);

            OnDisplayMessage?.Invoke(message.receivedMessage, 5.0f, null);
        }

        private void TriggerGeneric(byte [] bytes) {
            CCMessageGeneric messageGeneric = new CCMessageGeneric(bytes);

            foreach (string s in generics.Keys) {
                if (string.Equals(s, messageGeneric.genericName)) {
                    generics[s].Apply(messageGeneric);
                    return;
                }
            }

            LogError("Generic Class " + messageGeneric.genericName + " not found!");
        }

        private void TriggerEffect(byte[] bytes)
        {
            CCMessageEffectRequest effect = new CCMessageEffectRequest(bytes);

            CCEffectBase ccEffect = null;

            if (!effectsByID.ContainsKey(effect.effectID))
            {
                foreach (CCEffectBase baseEffect in effectsByID.Values)
                {
                    if (baseEffect.HasParameterID(effect.effectID))
                    {
                        ccEffect = baseEffect;
                        effect.parameters = effect.effectID + ", " + effect.parameters;
                        break;
                    }
                }

                if (ccEffect == null)
                {
                    LogError("Invalid effect identifier '{0}'.", effect.effectID);
                    return;
                }
            }
            else
            {
                ccEffect = effectsByID[effect.effectID];
            }

            if (ccEffect is CCEffectTimed)
            {
                (ccEffect as CCEffectTimed).SetDuration(effect.durationTime);
            }

            QueueEffect(ccEffect, effect.viewers, effect.blockID, effect.parameters);
        }

        private void ProcessMsg(PendingMessage message)
        {
            byte[] bytes = message.Bytes;
            MessageType messageType = (MessageType)message.MsgType;

            Log("Received message type {0} of size {1}", messageType, message.Size);

            switch (messageType) 
            {
                case MessageType.Version:
                    HelloMessage(bytes);
                    break;
                case MessageType.TokenAquisition:
                    TokenAquisition(bytes); 
                    break;
                case MessageType.TokenHandshake:
                    TokenHandshake(bytes);
                    break;
                case MessageType.BlockError:
                    BlockError(bytes);
                    break;
                case MessageType.UserMessage:
                    BroadcastedMessage(bytes);
                    break;
                case MessageType.EffectRequest:
                    TriggerEffect(bytes);
                    break;
                case MessageType.Generic:
                    TriggerGeneric(bytes);
                    break;
            }
        }

        #endregion

        #region Effect Handlings

        /// <summary>Test a generic being sent to the server.</summary>
        public void SendGenericTest(CCGeneric generic) {
            if (!isActiveAndEnabled) return;
            CCMessageGeneric messageGeneric = new CCMessageGeneric(_blockID++, generic.Name, generic.Data());
            Send(messageGeneric);
        }

        /// <summary>Test a generic locally.</summary>
        public void GetGenericTest(CCGeneric generic) {
            CCMessageGeneric testGeneric = new CCMessageGeneric(_blockID++, generic.Name, generic.Data());
            TriggerGeneric(testGeneric.ByteStream);
        }

        /// <summary>Test an effect locally. Its events won't be sent to the server.</summary>
        public void TestEffect(CCEffectBase effect)
        {
            if (!isActiveAndEnabled) return;

            if (testUser == null)
            {
                testUser = new TwitchUser
                {
                    id = Convert.ToUInt64(UnityEngine.Random.Range(1, 100000)),
                    name = "test_user",
                    displayName = "Test_User",
                    profileIcon = _tempUserIcon,
                    profileIconColor = _tempUserColor
                };

                twitchUsers.Add(testUser.displayName, testUser);
            }

            StartCoroutine(WaitForEffectListToLoad(effect));
        }

        private IEnumerator WaitForEffectListToLoad(CCEffectBase effect)
        {
            while (effectsByID.Count == 0)
            {
                yield return new WaitForSeconds(1.0f);
            }

            if (effectsByID.ContainsKey(effect.effectKey))
            {
                SendCCEffectLocally(effect, testUser);
                yield break;
            }

            LogError("Invalid effect identifier '{0}'.", effect.effectKey);
        }

        private void SendCCEffectLocally(CCEffectBase effect, TwitchUser twitchUser)
        {
            QueueEffect(effect, new CCMessageEffectRequest.Viewer[] { new CCMessageEffectRequest.Viewer(twitchUser.name) }, _blockID++, effect.Params(), true);
        }

        // Allocates an effect instance and add it to the pending list.
        private void QueueEffect(CCEffectBase effect, CCMessageEffectRequest.Viewer[] viewers, uint blockID, string parameters = null, bool test = false)
        {
            Assert.IsTrue(isActiveAndEnabled);
            StartCoroutine(DownloadUserInfo(effect, viewers, blockID, parameters, test));
        }

        private IEnumerator DownloadUserInfo(CCEffectBase effect, CCMessageEffectRequest.Viewer[] viewers, uint receivedBlockID, string parameters = null, bool test = false)
        {
            TwitchUser displayUser;

            if (viewers != null)
            {
                if (viewers.Length == 0 && effect is CCEffectBidWar)
                {
                    displayUser = crowdUser;
                }
                else
                {
                    foreach (CCMessageEffectRequest.Viewer viewer in viewers)
                    {
                        if (twitchUsers.ContainsKey(viewer.displayName))
                        {
                            continue;
                        }

                        TwitchUser user = new TwitchUser();
                        twitchUsers.Add(viewer.displayName, user);
                        user.displayName = viewer.displayName;
                        user.name = viewer.displayName;
                        user.profileIconUrl = viewer.iconURL;
                        yield return StartCoroutine(DownloadPlayerSprite(user));
                        twitchUsers[user.name] = user; 
                    } 

                    if (viewers.Length >= 2 || effect is CCEffectBidWar)
                    {
                        displayUser = crowdUser;
                    }
                    else
                    {
                        displayUser = twitchUsers[viewers[0].displayName];
                    }
                }
            }
            else
            {
                displayUser = anonymousUser;
            }

            if (effect is CCEffectTimed)
                CreateEffectInstance<CCEffectInstanceTimed>(displayUser, effect as CCEffectTimed, receivedBlockID, parameters, test);
            else if (effect is CCEffectParameters)
                CreateEffectInstance<CCEffectInstanceParameters>(displayUser, effect as CCEffectParameters, receivedBlockID, parameters, test);
            else if (effect is CCEffectBidWar)
                CreateEffectInstance<CCEffectInstanceBidWar>(displayUser, effect as CCEffectBidWar, receivedBlockID, parameters, test);
            else
                CreateEffectInstance<CCEffectInstance>(displayUser, effect, receivedBlockID, parameters, test);
        }

        private void CreateEffectInstance<T>(TwitchUser user, CCEffectBase effect, uint blockID, string parameters, bool test) where T : CCEffectInstance, new()
        {
            T effectInstance = new T();

            effectInstance.id = blockID;
            effectInstance.user = user;
            effectInstance.effect = effect;
            effectInstance.retryCount = 0;
            effectInstance.unscaledStartTime = Time.unscaledTime; // TODO add some delay?
            effectInstance.isTest = test;

            if (effect is CCEffectParameters)
            {
                CCEffectParameters paramInstance = effect as CCEffectParameters;

                if (parameters.Contains(","))
                    paramInstance.AssignParameters(parameters.Split(','));
                else
                    paramInstance.AssignParameters(new string[] { parameters });
            }

            string effectID = effect.effectKey;
            CCEffectEntry effectEntry = ccEffectEntries[effectID];

            if (effectsByID[effectID] is CCEffectParameters)
            {
                if (string.IsNullOrEmpty(parameters))
                {
                    CancelEffect(effectInstance);
                    return;
                }

                CCEffectInstanceParameters paramsInstance = effectInstance as CCEffectInstanceParameters;
                paramsInstance.AssignParameters(parameters);
                (effectInstance as CCEffectInstanceParameters).AssignParameters(parameters);
            }

            else if (effectsByID[effectID] is CCEffectBidWar)
            {
                if (string.IsNullOrEmpty(parameters))
                { 
                    CancelEffect(effectInstance);    
                    return;
                }

                string[] splitParams = parameters.Split(','); 

                if (splitParams.Length < 2 || splitParams[1] == " ") {
                    CancelEffect(effectInstance);
                    return;
                } 

                CCEffectInstanceBidWar bidWarInstance = effectInstance as CCEffectInstanceBidWar;
                bidWarInstance.Init(splitParams[0], Convert.ToUInt32(splitParams[1]));

                if (bidWarInstance.BidAmount == 0 || !(effectsByID[effectID] as CCEffectBidWar).PlaceBid(bidWarInstance.BidKey, bidWarInstance.BidAmount))
                {
                    return;
                }
            }

            pendingQueue.Enqueue(effectInstance);
            OnEffectQueue?.Invoke(effectInstance);
        }

        private IEnumerator StartTimedEffect(CCEffectInstance effectInstance)
        {
            EffectSuccess(effectInstance);
            yield return new WaitForSeconds(0.1f);
            SetTimedEffectState(effectInstance as CCEffectInstanceTimed, true);
        }

        private void DequeueEffectInstance(CCEffectInstance effectInstance, EffectResult result)
        {
            OnEffectDequeue?.Invoke(effectInstance.effectKey, result);
        }

        // Process an effect instance in the pending list.
        private bool StartEffect(CCEffectInstance effectInstance)
        {
            EffectResult result;
            bool dequeue = true;
            bool isTest = effectInstance.isTest;
            CCEffectBase effect = effectInstance.effect;
            uint id = effectInstance.id;

            CCEffectInstanceTimed timedEffectInstance = effectInstance as CCEffectInstanceTimed;

            if (timedEffectInstance != null)
            {
                timedEffectInstance.effect = effect as CCEffectTimed;

                if (timedEffectInstance != null && !timedEffectInstance.isActive)
                {
                    result = EffectResult.Retry;
                    goto Retry;
                }
            }

            if (effect.CanBeRan())
            {
                result = effect.OnTriggerEffect(effectInstance);
            }
            else
            {
                result = EffectResult.Retry;
            }

            Assert.AreEqual(effectInstance.effect, effect);

            switch (result)
            {
                default:
                    LogError("Unhandled EffectResult.{0}", result);
                    break;
                case EffectResult.Failure:
                    DequeueEffectInstance(effectInstance, result);
                    UpdateEffect(effectInstance, Protocol.EffectState.TemporaryFailure);
                    break;
                case EffectResult.Unavailable:
                    DequeueEffectInstance(effectInstance, result);
                    UpdateEffect(effectInstance, Protocol.EffectState.UnavailableForOrder);
                    break;

                // Effect instance triggered successfully.
                case EffectResult.Success:
                    timeUntilNextEffect = delayBetweenEffects;
                    DequeueEffectInstance(effectInstance, EffectResult.Success);
                    OnEffectTrigger?.Invoke(effectInstance);
                    EffectSuccess(effectInstance);
                    break;

                // Move from the pending list to the running list.
                case EffectResult.Running:
                    Assert.IsNotNull(timedEffectInstance);
                    timeUntilNextEffect = delayBetweenEffects;

                    runningEffects.Add(effectInstance.effectKey, timedEffectInstance);
                    DequeueEffectInstance(effectInstance, EffectResult.Success);
                    OnEffectStart?.Invoke(timedEffectInstance);
                    StartCoroutine(StartTimedEffect(effectInstance));
                    break;

                // Moved from the pending list to the behaviour's internal queue.
                case EffectResult.Queue:
                    Assert.IsNotNull(timedEffectInstance);
                    break;

                // Leave in the pending list unless the instance reached max retries.
                case EffectResult.Retry:
                    goto Retry;
            }
            goto Done;

        Retry:
            effectInstance.retryCount++;
            if (effectInstance.retryCount > effect.maxRetries)
            {
                result = EffectResult.Failure;
                CancelEffect(effectInstance);
            }
            else
            {
                effectInstance.unscaledStartTime = effect.retryDelay + Time.unscaledTime;
                dequeue = false;
            }

        Done:
            if (dequeue)
                effect.delayUntilUnscaledTime = effect.pendingDelay + Time.unscaledTime;
            else
                effect.delayUntilUnscaledTime = effectInstance.unscaledStartTime;

            return dequeue;
        }

        private IEnumerator ResetTimedEffect(CCEffectInstanceTimed effectInstance, uint newID)
        {
            ResetEffect(effectInstance.effect);

            SetTimedEffectState(effectInstance, false);
            yield return new WaitForSeconds(0.1f);
            effectInstance.id = newID;
            UpdateEffect(effectInstance, Protocol.EffectState.Success);
            yield return new WaitForSeconds(0.1f);
            SetTimedEffectState(effectInstance, true);
        }

        // Process an effect instance in the running list.
        private bool StopEffect(CCEffectInstanceTimed effectInstance, bool removeFromList, bool force = false)
        {
            Assert.IsNotNull(effectInstance);

            if (effectInstance.unscaledTimeLeft > 0.0f && !force)
            {
                // TODO force stop after maxRetries?
                effectInstance.unscaledEndTime = effectInstance.effect.retryDelay + Time.unscaledTime;
                effectInstance.unscaledTimeLeft = effectInstance.effect.retryDelay;

                if (removeFromList)
                {
                    SetTimedEffectState(effectInstance, false);
                    effectInstance.effect.OnStopEffect(effectInstance, false);
                }

                return false;
            }

            string id = effectInstance.effectKey;

            if (removeFromList)
            {
                if (haltedTimers.ContainsKey(id) && haltedTimers[id].Count > 0)
                {
                    DequeueEffectInstance(effectInstance, EffectResult.Success);
                    StartCoroutine(ResetTimedEffect(effectInstance, haltedTimers[id].Dequeue()));

                    return false;
                }
            }

            effectInstance.effect.OnStopEffect(effectInstance, force);
            OnEffectStop?.Invoke(effectInstance);
            SetTimedEffectState(effectInstance, false);

            if (removeFromList)
            {
                runningEffects.Remove(id);
                effectInstance = null;
            }

            return true;
        }

        /// <summary>Cancels a received effect</summary>
        public void CancelEffect(CCEffectInstance effectInstance)
        {
            EffectFailure(effectInstance);
            DequeueEffectInstance(effectInstance, EffectResult.Failure);
        }

        /// <summary>Forcefully terminates all pending and running effects.</summary>
        public void StopAllEffects()
        {
            foreach (string queueID in haltedTimers.Keys)
            {
                while (haltedTimers[queueID].Count > 0)
                {
                    OnEffectDequeue?.Invoke(queueID, EffectResult.Failure);
                    haltedTimers[queueID].Dequeue();
                }
            }

            haltedTimers.Clear();

            foreach (CCEffectInstance instance in pendingQueue) // Cancel the rest of the pending effects
                CancelEffect(instance);

            pendingQueue.Clear();

            foreach (CCEffectInstanceTimed instance in runningEffects.Values) // Stop all running timers
                StopEffect(instance, false, true);

            runningEffects.Clear();
        }

        #endregion

        #region Linked List Utilities

        // Runs the action on every effect in the list and removes entries where the action returns true.
        private void RunQueue(Func<CCEffectInstanceTimed, bool> action)
        {
            foreach (CCEffectInstanceTimed instance in runningEffects.Values)
            {
                if (action(instance))
                    return;
            }
        }

        #endregion

        #region Pause/Resume Timed Effects

        /// <summary>Resume a timed effect.</summary>
        public static void EnableEffect(CCEffectTimed effect) => RunEffect(effect, Resume);
        /// <summary>Disable a timed effect.</summary>
        public static void DisableEffect(CCEffectTimed effect) => RunEffect(effect, Pause);
        /// <summary>Reset a timed effect.</summary>
        public static void ResetEffect(CCEffectTimed effect) => RunEffect(effect, Reset);

        /// <summary>Pauses a timer effect.</summary>
        public static void Pause(CCEffectInstanceTimed effectInstance)
        {
            if (effectInstance.isPaused)
            {
                return;
            }

            effectInstance.effect.Pause(effectInstance);
            effectInstance.effect.OnPauseEffect();
            instance.OnEffectPause?.Invoke(effectInstance);
            instance.UpdateEffect(effectInstance, Protocol.EffectState.TimedPause, 0);
        }

        /// <summary>Resumes a timer command</summary>
        public static void Resume(CCEffectInstanceTimed effectInstance)
        {
            if (!effectInstance.isPaused)
            {
                return;
            }

            effectInstance.effect.Resume(effectInstance);
            effectInstance.effect.OnResumeEffect();
            instance.OnEffectResume?.Invoke(effectInstance);
            instance.UpdateEffect(effectInstance, Protocol.EffectState.TimedResume, Convert.ToUInt16(effectInstance.unscaledTimeLeft));
        }

        /// <summary>Resets a timer command</summary>
        public static void Reset(CCEffectInstanceTimed effectInstance)
        {
            effectInstance.effect.Reset(effectInstance);
            effectInstance.effect.OnResetEffect();
            instance.OnEffectReset?.Invoke(effectInstance);
            instance.UpdateEffect(effectInstance, Protocol.EffectState.TimedResume, Convert.ToUInt16(effectInstance.unscaledTimeLeft));
        }

        private static void UpdateTimerEffectsFromIdle()
        {
            if (instance == null || instance.runningEffects == null)
            {
                return;
            }

            foreach (CCEffectInstanceTimed timedEffect in instance.runningEffects.Values)
            {
                if (!timedEffect.effect.ShouldBeRunning())
                {
                    continue;
                }

                if (!instance._paused)
                {
                    EnableEffect(timedEffect.effect);
                }
                else
                {
                    DisableEffect(timedEffect.effect);
                }
            }
        }

        /// <summary>Checks all running timer effects to see if they should be running or not. </summary>
        private static void UpdateTimerEffectStatuses()
        {
            if (instance == null || instance.runningEffects == null || instance._paused)
            {
                return;
            }

            foreach (CCEffectInstanceTimed timedEffect in instance.runningEffects.Values)
            {
                if (timedEffect.effect.ShouldBeRunning())
                {
                    EnableEffect(timedEffect.effect);
                }
                else
                {
                    DisableEffect(timedEffect.effect);
                }
            }
        }

        // Runs the action on every running effect instance matching the given effect behaviour.
        static void RunEffect(CCEffectTimed effect, Action<CCEffectInstanceTimed> action)
        {
            var self = instance;
            if (self == null || self.runningEffects == null) return;

            if (self.runningEffects.ContainsKey(effect.effectKey))
                action(self.runningEffects[effect.effectKey]);
        }

        #endregion

        #region Querying & Actions

        /// <summary>Returns true if at least one timed effect is currently running.</summary>
        public bool HasRunningEffects() => runningEffects != null;

        /// <summary>Returns true if timed effect is running.</summary>
        public bool IsRunning(CCEffectTimed type) => RunFirst(type.effectKey, IsRunning);
        /// <summary>Stops a timer effect.</summary>
        public bool StopOne(CCEffectTimed type) => RunFirst(type.effectKey, StopOne);
        /// <summary>Returns true if timed effect is paused.</summary>
        public bool IsPaused(CCEffectTimed type) => RunFirst(type.effectKey, IsPaused);

        static bool IsRunning(CCEffectInstanceTimed effectInstance) => effectInstance.isActive;
        static bool IsPaused(CCEffectInstanceTimed effectInstance) => effectInstance.isPaused;

        bool StopOne(CCEffectInstanceTimed effectInstance) => StopEffect(effectInstance, true, true);

        bool RunFirst(string identifier, Func<CCEffectInstanceTimed, bool> action)
        {
            if (!runningEffects.ContainsKey(identifier))
                return false;

            return action(runningEffects[identifier]);
        }

        #endregion

        #region Debug

        public void Log(string fmt, params object[] args) => Log(string.Format(fmt, args));
        public void LogWarning(string fmt, params object[] args) => LogWarning(string.Format(fmt, args));
        public void LogError(string fmt, params object[] args) => LogError(string.Format(fmt, args));

        private const string _ccPrefix = "[CC] ";

        public static void Log(string content)
        {
            if (instance != null && instance._debugLog)
            {
                Debug.Log(_ccPrefix + content);
            }
        }

        public static void LogWarning(string content)
        {
            if (instance != null && instance._debugWarning)
            {
                Debug.LogWarning(_ccPrefix + content);
            }
        }

        public static void LogError(string content)
        {
            if (instance != null && instance._debugError)
            {
                Debug.LogError(_ccPrefix + content);
            }
        }

        public static void LogException(Exception e)
        {
            if (instance != null && instance._debugExceptions)
            {
                Debug.LogException(e);
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
