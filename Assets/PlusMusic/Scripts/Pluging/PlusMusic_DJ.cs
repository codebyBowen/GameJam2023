#region Usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using PlusMusic;
using UnityEngine.Audio;
#endregion


public class PlusMusic_DJ : MonoBehaviour
{
    #region Initialization
    //-----------------------------------------------
    // Private vars
    //-----------------------------------------------
    private string pluginVersion = "";
    private Dictionary<string, AudioClip> map_AudioClip = new Dictionary<string, AudioClip>();
    private Dictionary<string, string> map_AudioURLs = new Dictionary<string, string>();
    
    //private const int FILESNEEDED = 5; //default 7 arrangements for game jam, 9 total available
    private int num_arrangements_to_load = 0;

    private string previousTag = "backing_track";
    private bool licenseLimited;
    private AudioSource currentAudioSource;
    private bool isAudioSource1Playing = true;
    //private string musicDirectory = "Assets/Resources/PlusMusic/Music";
    private string next_tag = "backing_track";
    private string whichSoundtrack;
    private int loadedRemoteFiles = 0;
    private PlusMusicSettingsSo plusMusicAccount;
    private ArrangementURL arrangementURLFile;
    private TrackTimingsData trackTimingsDataHold;
    //private AuthenticationData authData;
    private ParentProjectData parentData;
    private Dictionary<string, AudioClip> map_StingerClip = new Dictionary<string, AudioClip>();
    private Dictionary<string, Coroutine> playingCoroutines = new Dictionary<string, Coroutine>();
    private float lastTime = 0.0f;
    private int loopCounter;

    //-----------------------------------------------
    // Public vars
    //-----------------------------------------------
    public enum PMTags
    {
        none = 0,
        high_backing,
        low_backing,
        backing_track,
        preview,
        victory,
        failure,
        highlight,
        lowlight,
        full_song
    };
    public enum PMTimings { beats, bars, now };

    public static PlusMusic_DJ Instance;
    public AudioSource theAudioSource;
    public AudioSource newAudio;
    public AudioSource stingerSource;
    public bool Persist = true;
    //public PlusMusicSoundtracking PlusMusicSoundtrackingWeb;
    [HideInInspector] public ServerArrangementsData serverArrangementsFile;
    [HideInInspector] public SoundtrackOptionData[] theSoundtrackOptions;
    public event Action<SoundtrackOptionData[]> OnSoundTrackOptionsReceived;
    //public GameObject loadingSpin;
    [HideInInspector] public PM_Settings settings;
    public TransitionInfo defaultTransition;
    public PMTags autoPlayArrangement = PMTags.backing_track;
    public float musicVolume = 1.0f;
    public event Action<float> LoadingProgress;
    public event Action<string> RealTimeDebug;
    public AudioMixer audioMixer;

    public string WhichSoundtrack { get => whichSoundtrack; set => whichSoundtrack = value; }
    public string PluginVersion { get => pluginVersion; }
    public SoundtrackOptionData[] TheSoundtrackOptions { get => theSoundtrackOptions; }

    private void Awake()
    {
        Instance = this;
        
        // Moved DontDestroyOnLoad() to Awake() to make sure we have a valid reference
        // for other classes in their Start() functions
        if (Persist)
        {
            DontDestroyOnLoad(Instance);
        }

        // Set current to first as default so set volume doesn't break if we're called 
        // by another class before Update()
        currentAudioSource = theAudioSource;
    }
    
    void Start()
    {
        Debug.Log("------------------ PlusMusicDJ ----------------------");
        Debug.Log("Start()");

        AudioClip[] stingers = Resources.LoadAll<AudioClip>("Stinger");
        plusMusicAccount = Resources.Load<PlusMusicSettingsSo>("PlusMusicSettingsSo");

        if (plusMusicAccount == null)
        {
            Debug.LogError("PlusMusic account is not configured!");
            RealTimeDebug?.Invoke("PlusMusic account is not configured!");
            return;
        }

        plusMusicAccount.SetPackageData();
        if (plusMusicAccount.PackageData != null && plusMusicAccount.PackageData.version != null)
        {
            pluginVersion = plusMusicAccount.PackageData.version;
        }
        else
        {
            Debug.LogError("package.json is missing");
        }

        for (int i = 0; i < stingers.Length; i++)
        {
            map_StingerClip.Add(stingers[i].name, stingers[i]);
        }

        playingCoroutines = new Dictionary<string, Coroutine>();
        playingCoroutines.Add("transitionCoroutine", null);
        playingCoroutines.Add("effectCoroutine", null);
        playingCoroutines.Add("getArragementCoroutine", null);
        playingCoroutines.Add("queueSong", null);
        licenseLimited = false;

        settings = new PM_Settings();

        // Get id and key from either the editor or the environment
        // NOTE: In production neither of these will be present so settings need to be loaded from a save file
        string defProjectId  = GetEnvVariable("PM_PROJECT", plusMusicAccount.ProjectId.ToString());
        string defProjectKey = GetEnvVariable("PM_API_KEY", plusMusicAccount.ApiKey);

        // If auto load is enabled and we have supplied credentials
        // we try to load the specified project data
        if (plusMusicAccount.AutoLoadProject)
            LoadProject(Int64.Parse(defProjectId), defProjectKey, plusMusicAccount.AutoPlayProject);
        else
            SetCurrentProject(Int64.Parse(defProjectId), defProjectKey, false);

        // Send a scene start ping back home
        SetPingBackInfo("Start of scene", settings.project_id, "", "", "", "", 0.0f, false);
    }

    /**
    * @brief Load an environment variable into a String
    * @param var_name
    * @param def_val
    * @return Return the env variable String or def_val("") if not found
    */
    public string GetEnvVariable(string var_name, string def_val)
    {
        string envVariable = Environment.GetEnvironmentVariable(var_name);

        if (!string.IsNullOrWhiteSpace(envVariable))
            return envVariable.Trim();

        return def_val;
    }

    /**
    * @brief Make the supplied project the current
    * @param projectId
    * @param projectKey
    */
    private void SetCurrentProject(Int64 projectId, string projectKey, bool autoPlay)
    {
        Debug.LogFormat("SetCurrentProject()");

        // Set the credentials object values
        settings.target      = GetEnvVariable("PM_TARGET", "app");
        settings.username    = GetEnvVariable("PM_USER", "");
        settings.password    = GetEnvVariable("PM_PASS", "");
        settings.project_id  = projectId;
        settings.api_key     = projectKey;
        settings.auto_play   = autoPlay;
        settings.credentials = "";
        if ((!string.IsNullOrWhiteSpace(settings.username)) && (!string.IsNullOrWhiteSpace(settings.password)))
            settings.credentials = settings.username + ":" + settings.password + "@";
        settings.base_url = "https://" + settings.credentials + settings.target + ".plusmusic.ai/api/plugin/";

        if (plusMusicAccount.DebugMode)
        {
            Debug.LogFormat("> target      : {0}", settings.target);
            Debug.LogFormat("> username    : {0}", settings.username);
            Debug.LogFormat("> password    : {0}", settings.password);
            Debug.LogFormat("> project_id  : {0}", settings.project_id);
            Debug.LogFormat("> api_key     : {0}", settings.api_key);
            Debug.LogFormat("> auto_play   : {0}", settings.auto_play);
            Debug.LogFormat("> credentials : {0}", settings.credentials);
            Debug.LogFormat("> base_url    : {0}", settings.base_url);
        }
    }

    #endregion


    #region Basic Methods
    void Update()
    {

        if (isAudioSource1Playing)
        {
            currentAudioSource = theAudioSource;
        }
        else
        {
            currentAudioSource = newAudio;
        }
        if (currentAudioSource.clip != null)
        {
            RealTimeDebug?.Invoke("soundtrack " + WhichSoundtrack + "'s " + next_tag + " (length: " + currentAudioSource.clip.length + ") playing for " + currentAudioSource.time.ToString("0.00"));
        }

        //first frame of the loop
        if (lastTime > currentAudioSource.time)
        {
            loopCounter++;
        }
        lastTime = currentAudioSource.time;
    }

    private void LateUpdate()
    {
        if (licenseLimited)
        {
            //if (debugText != null) debugText.text = "SOUNDTRACK UNAVAILABLE";
            RealTimeDebug?.Invoke("SOUNDTRACK UNAVAILABLE");
            theAudioSource.Stop();
            newAudio.Stop();
        }
    }
    
    public void ChangeSoundtrack(string theSoundtrack)
    {
        LoadingProgress?.Invoke(0);
        WhichSoundtrack = theSoundtrack;
        RealTimeDebug?.Invoke("Setting soundtrack " + theSoundtrack);
        SelectRemoteSoundtrackByID(WhichSoundtrack);
    }

    public bool AllFilesLoaded()
    {
        if (loadedRemoteFiles >= num_arrangements_to_load)
        {
            return true;
        }
        else if (loadedRemoteFiles < num_arrangements_to_load)
        {
            int numNeeded = num_arrangements_to_load - loadedRemoteFiles;
            if (licenseLimited)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    #endregion
    

    #region Transition Methods
    
    private float GetNextClosestTime(float[] countsToUse)
    {
        float sendThis = 0.0f;
        foreach (float count in countsToUse)
        {
            if (currentAudioSource.time < count)
            {
                sendThis = count - currentAudioSource.time;
                return sendThis;
            }
        }
        return sendThis;
    }
    #endregion


    #region Web Calls

    /**
    * @brief It storage the data of the song, events and device
    * @param eventText - The type of pingback
    * @param pingProject - The project which is playing
    * @param pingSoundtrack - The sountrack which is playing
    * @param pingTag - The segment of the sountrack which is playing
    * @param pingTransitionType - The type of transition
    * @param pingTransitionTiming - The if it was useed a transition by timing
    * @param pingTransitionDelay - The delay that the transition requires
    * @param isUsingStinger - If it is using a stinger in the transition
    */
    private void SetPingBackInfo(
        string eventText, Int64 pingProject, string pingSoundtrack, string pingTag, 
        string pingTransitionType, string pingTransitionTiming, float pingTransitionDelay, bool isUsingStinger)
    {
        Debug.Log("SetPingBackInfo()");

        PingBack pingBackData = new PingBack()
        {
            os = SystemInfo.operatingSystem,
            event_text = eventText,
            device_id = SystemInfo.deviceUniqueIdentifier,
            in_editor = Application.isEditor,
            platform = "Unity",
            title = Application.productName,
            connected = Application.internetReachability.ToString(),
            is_using_stinger = isUsingStinger,
            project_id = ((0 == pingProject) ? -1 : pingProject),
            arrangement_id = (string.IsNullOrWhiteSpace(pingSoundtrack) ? -1 : Int64.Parse(pingSoundtrack)),
            arrangement_type = pingTag,
            transition_type = pingTransitionType,
            transition_timing = pingTransitionTiming,
            transition_delay = pingTransitionDelay,
            time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            //web_url = Application.absoluteURL,
            web_url = "",
            plugin_version = PluginVersion,
            play_id = ""
        };

        string sendingData = JsonUtility.ToJson(pingBackData);

        // NOTE: 'event' is a reserved keyword and can't be used in the PingBack data class
        // We use 'event_text' instead and replace it with 'event' in the data string
        // We also replace "" and ":-1" with null to indicate missing string and number values
        sendingData = "{\"ping_backs\":[" +
            sendingData.Replace("event_text", "event")
                .Replace("\"\"", "null")
                .Replace(": -1", ": null")
                .Replace(":-1", ":null") +
            "]}";

        StartCoroutine(UploadPingBack(sendingData));
    }
    
    IEnumerator UploadPingBack(string dataToSend)
    {
        //string finalURL = "https://app.plusmusic.ai/api/plugin/ping-backs";
        string finalURL = settings.base_url + "ping-backs";

        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("UploadPingBack(): finalURL = {0}", finalURL);
            Debug.LogFormat("> dataToSend = {0}", dataToSend);
        }

        var webRequest = new UnityWebRequest(finalURL, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(dataToSend);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        webRequest.SetRequestHeader("x-api-key", settings.api_key);

        yield return webRequest.SendWebRequest();
        string jsonString = webRequest.downloadHandler.text;

        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("> responseCode = {0}", webRequest.responseCode);
            Debug.LogFormat("> result = {0}", webRequest.result);
        }

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            RealTimeDebug?.Invoke("PingBack failed");
            Debug.LogError("PingBack failed: " + webRequest.error);
            if (plusMusicAccount.DebugMode)
                Debug.LogFormat("> response = {0}", jsonString);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            string friendlyTime = System.DateTime.UtcNow.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "_");
            FileStream file = File.Create(Application.persistentDataPath + "/playinfo" + friendlyTime + ".dat");
            SavePlayInfo data = new SavePlayInfo();

            data.savedData = dataToSend;
            binaryFormatter.Serialize(file, data);
            file.Close();
        }
        else
        {
            if (webRequest.responseCode == 200 || webRequest.responseCode == 201)
            {
                Debug.Log("PingBackInfo successfully sent");
                if (plusMusicAccount.LogServerResponses)
                    Debug.LogFormat("> response = {0}", jsonString);
            }
            else if (webRequest.responseCode == 403)
            {
                licenseLimited = true;
            }
            else
            {
                Debug.LogError("PingBack failed!");
                if (plusMusicAccount.DebugMode)
                    Debug.LogFormat("> response = {0}", jsonString);
            }
        }

        //webRequest.Dispose();
    }
    
    //IEnumerator GetRemoteArrangementsJSON(string soundtrack)
    IEnumerator LoadArrangements(Int64 soundtrackId)
    {
        Debug.LogFormat("LoadArrangements({0})", soundtrackId);

        num_arrangements_to_load = 0;

        //string finalURL = "https://app.plusmusic.ai/api/plugin/projects/" + soundtrack + "?plugin_version=" + PluginVersion;
        string finalURL = String.Format("{0}projects/{1}?plugin_version={2}", settings.base_url, soundtrackId, PluginVersion);
        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("LoadArrangements(): finalURL = {0}", finalURL);

        UnityWebRequest webRequest = UnityWebRequest.Get(finalURL);
        webRequest.SetRequestHeader("Accept", "application/json");
        webRequest.SetRequestHeader("x-api-key", settings.api_key);
        yield return webRequest.SendWebRequest();
        string jsonString = webRequest.downloadHandler.text;

        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("> responseCode = {0}", webRequest.responseCode);
            Debug.LogFormat("> result = {0}", webRequest.result);
        }

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Failed getting arragements: " + webRequest.error);
            if (plusMusicAccount.DebugMode)
                Debug.LogFormat("> response = {0}", jsonString);
            //webRequest.Dispose();
        }
        else if (webRequest.responseCode == 403)
        {
            licenseLimited = true;
            if (plusMusicAccount.LogServerResponses)
                Debug.LogFormat("> response = {0}", jsonString);
            //webRequest.Dispose();
        }
        else
        {
            if (plusMusicAccount.LogServerResponses)
                Debug.LogFormat("> response = {0}", jsonString);

            ServerArrangementsData serverArrangementsData = JsonUtility.FromJson<ServerArrangementsData>(jsonString);
            serverArrangementsFile = serverArrangementsData;

            Debug.Log("Loading arrangements for soundtrack: " + serverArrangementsFile.name);

            RealTimeDebug?.Invoke(
                "Loading soundtrack arrangements for: " + serverArrangementsFile.name + ", last modified " + serverArrangementsFile.updated_at);
            //webRequest.Dispose();
            SetAllTrackTimings(soundtrackId.ToString());
        }
    }
    
    private void SetAllTrackTimings(string soundtrack)
    {
        Debug.LogFormat("SetAllTrackTimings({0})", soundtrack);

        // Set number of arrangements to download
        num_arrangements_to_load = serverArrangementsFile.arrangements.Length;

        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("> num_arrangements_to_load = {0}", num_arrangements_to_load);

        trackTimingsDataHold = new TrackTimingsData();

        foreach (Arrangements arrangement in serverArrangementsFile.arrangements)
        {
            //Debug.LogFormat("$$$ arrangement.container.type_id = {0}", arrangement.container.type_id);

            switch (arrangement.container.type_id)
            {
                case 1:
                    {
                        trackTimingsDataHold.low_backing_bars  = arrangement.bars;
                        trackTimingsDataHold.low_backing_beats = arrangement.beats;
                    }
                    break;
                case 2:
                    {
                        trackTimingsDataHold.high_backing_bars  = arrangement.bars;
                        trackTimingsDataHold.high_backing_beats = arrangement.beats;
                    }
                    break;
                case 3:
                    {
                        trackTimingsDataHold.backing_track_bars  = arrangement.bars;
                        trackTimingsDataHold.backing_track_beats = arrangement.beats;
                    }
                    break;
                case 4:
                    {
                        trackTimingsDataHold.preview_bars  = arrangement.bars;
                        trackTimingsDataHold.preview_beats = arrangement.beats;
                    }
                    break;
                case 5:
                    {
                        trackTimingsDataHold.victory_bars  = arrangement.bars;
                        trackTimingsDataHold.victory_beats = arrangement.beats;
                    }
                    break;
                case 6:
                    {
                        trackTimingsDataHold.failure_bars  = arrangement.bars;
                        trackTimingsDataHold.failure_beats = arrangement.beats;
                    }
                    break;
                case 7:
                    {
                        trackTimingsDataHold.highlight_bars  = arrangement.bars;
                        trackTimingsDataHold.highlight_beats = arrangement.beats;
                    }
                    break;
                case 8:
                    {
                        trackTimingsDataHold.lowlight_bars  = arrangement.bars;
                        trackTimingsDataHold.lowlight_beats = arrangement.beats;
                    }
                    break;
                case 9:
                    {
                        trackTimingsDataHold.full_song_bars  = arrangement.bars;
                        trackTimingsDataHold.full_song_beats = arrangement.beats;
                    }
                    break;
                default:
                    Debug.LogError("SetAllTrackTimings(): Invalid Arrangement Type! "
                        + arrangement.container.type_id.ToString());
                    break;
            }
        }

        GetRemoteAvailableArrangements(soundtrack);
    }

    private void GetRemoteAvailableArrangements(string theSoundtrack)
    {
        Debug.LogFormat("GetRemoteAvailableArrangements({0})", theSoundtrack);

        StartCoroutine(CheckForAllURLsLoaded());

        foreach (Arrangements arrangement in serverArrangementsFile.arrangements)
        {
            //string streamURL = "https://app.plusmusic.ai/api/plugin/projects/" + theSoundtrack + "/arrangements/" + arrangement.id + "/stream?format=" + GetFileType();
            string streamURL = settings.base_url + "projects/" + theSoundtrack + "/arrangements/" +
                arrangement.id.ToString() + "/stream?format=ogg";

            //Debug.LogFormat("$$$ arrangement.container.type_id = {0}", arrangement.container.type_id);

            PMTags tagid = (PMTags)arrangement.container.type_id;
            //Debug.LogFormat("$$$ tagid = {0}", tagid.ToString());

            StartCoroutine(GetRemoteAudioClipURL(streamURL, tagid));
        }
    }
    
    IEnumerator GetRemoteAudioClipURL(string arrangementURL, PMTags arrangementType)//string arrangementClip)
    {
        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("GetRemoteAudioClipURL(): arrangementURL = {0}", arrangementURL);
            Debug.LogFormat("> arrangementType = {0}", arrangementType);
        }

        UnityWebRequest webRequest = UnityWebRequest.Get(arrangementURL);
        webRequest.SetRequestHeader("Accept", "application/json");
        webRequest.SetRequestHeader("x-api-key", settings.api_key);

        yield return webRequest.SendWebRequest();
        string jsonString = webRequest.downloadHandler.text;

        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("> responseCode = {0}", webRequest.responseCode);
            Debug.LogFormat("> result = {0}", webRequest.result);
        }

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            string errorMsg = String.Format($"'{0}' url not found!", arrangementType);
            Debug.LogError(errorMsg);
            RealTimeDebug.Invoke(errorMsg);

            if (plusMusicAccount.DebugMode)
                Debug.LogFormat("> response = {0}", jsonString);
        }
        else
        {
            if (plusMusicAccount.LogServerResponses)
                Debug.LogFormat("> response = {0}", jsonString);

            ArrangementURL arrangementURLData = JsonUtility.FromJson<ArrangementURL>(jsonString);

            // Sanity check, try to match returning type with requested type
            if ((int)arrangementType == arrangementURLData.arrangement_type_id)
            { 
                string arrangementStr = arrangementType.ToString();
                arrangementURLFile = arrangementURLData;
                if (!map_AudioURLs.ContainsKey(arrangementStr))
                {
                    map_AudioURLs.Add(arrangementStr, arrangementURLFile.arrangement_url);
                }
                else
                {
                    string warningMsg = String.Format(
                        $"'{0}' audioclip already added!", arrangementType);
                    Debug.LogWarning(warningMsg);
                    RealTimeDebug.Invoke(warningMsg);
                }
            }
            else
            {
                string errorMsg = String.Format(
                    $"Arrangement Type mismatch! {0} != {1}", (int)arrangementType, arrangementURLData.arrangement_type_id);
                Debug.LogError(errorMsg);
                RealTimeDebug.Invoke(errorMsg);
            }
        }

        //webRequest.Dispose();
    }

    private string GetFileType()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return "wav";
        }
        else
        {
            return "ogg";
        }
    }
    
    private AudioType GetAudioType()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return AudioType.WAV;
        }
        else
        {
            return AudioType.OGGVORBIS;
        }
    }

    IEnumerator CheckForAllURLsLoaded()
    {
        while (map_AudioURLs.Count < num_arrangements_to_load)
        {
            int numNeeded = num_arrangements_to_load - map_AudioURLs.Count;
            yield return null;
        }

        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("All {0} arrangement URLs loaded, proceeding to load arrangements ...", num_arrangements_to_load);

        if (playingCoroutines.ContainsKey("transitionCoroutine") && playingCoroutines["transitionCoroutine"] != null) 
        {
            StopCoroutine(playingCoroutines["transitionCoroutine"]); 
        }

        StartCoroutine(CheckForAllFilesLoaded());
        LoadAllArrangements();
    }
    
    IEnumerator CheckForAllFilesLoaded()
    {
        while (loadedRemoteFiles < num_arrangements_to_load)
        {
            int numNeeded = num_arrangements_to_load - loadedRemoteFiles;
            LoadingProgress?.Invoke((float)loadedRemoteFiles / num_arrangements_to_load);
            yield return null;
        }

        Debug.LogFormat("All {0} arrangements loaded", num_arrangements_to_load);

        LoadingProgress?.Invoke((float)loadedRemoteFiles / num_arrangements_to_load);

        RealTimeDebug?.Invoke("all arrangements loaded");
        if (playingCoroutines.ContainsKey("transitionCoroutine") && playingCoroutines["transitionCoroutine"] != null) 
        { 
            StopCoroutine(playingCoroutines["transitionCoroutine"]); 
        }

        if (plusMusicAccount.AutoPlayProject)
        {
            if (plusMusicAccount.DebugMode)
                Debug.LogFormat("> autoPlayArrangement = {0}", autoPlayArrangement);

            PlayArrangement(
                new TransitionInfo(autoPlayArrangement, 1.0f, PMTimings.bars, "", true)
            );
        }
    }
    
    private void LoadAllArrangements()
    {
        foreach (string thekey in map_AudioURLs.Keys)
        {
            string arrangementURL = map_AudioURLs[thekey];
            //Debug.Log("arrangementURL = " + arrangementURL);
            if (plusMusicAccount.DebugMode)
                Debug.Log("arrangementName = " + thekey);
            StartCoroutine(GetRemoteAudioClip(arrangementURL, thekey));
            //StartCoroutine(StreamWAV(arrangementURL, thekey));
        }
    }

    IEnumerator GetRemoteAudioClip(string songURL, string arrangementClip)
    {
        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("GetRemoteAudioClip(): songURL = {0}", songURL);

        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(songURL, GetAudioType());
        yield return webRequest.SendWebRequest();

        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("> responseCode = {0}", webRequest.responseCode);
            Debug.LogFormat("> result = {0}", webRequest.result);
        }

        if (webRequest.isDone)
        {
            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"Error Downloading {arrangementClip}");
                if (plusMusicAccount.DebugMode)
                {
                    string jsonString = webRequest.downloadHandler.text;
                    Debug.LogFormat("> response = {0}", jsonString);
                }
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(webRequest);
                SetAudioClip(myClip, arrangementClip);
            }
        }

        //webRequest.Dispose();
    }
    
    private void SetAudioClip(AudioClip audioClip, string name)
    {
        map_AudioClip.Add(name, audioClip);
        if (name == "low_backing")
        {
            theAudioSource.clip = audioClip;
        }
        RealTimeDebug?.Invoke($"Loaded {name}");
        if (plusMusicAccount.DebugMode)
            Debug.Log($"Loaded {name}");
        loadedRemoteFiles += 1;
    }

    IEnumerator RunSetupSoundtrackOptions(string finalURL, Int64 projectId, string projectKey, bool autoPlay)
    {
        RealTimeDebug?.Invoke("Setting up soundtrack options for menu");

        UnityWebRequest webRequest = UnityWebRequest.Get(finalURL);
        webRequest.SetRequestHeader("Accept", "application/json");
        webRequest.SetRequestHeader("x-api-key", settings.api_key);

        yield return webRequest.SendWebRequest();
        string jsonString = webRequest.downloadHandler.text;

        if (plusMusicAccount.DebugMode)
        { 
            Debug.LogFormat("> responseCode = {0}", webRequest.responseCode);
            Debug.LogFormat("> result = {0}", webRequest.result);
        }

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Failed soundtrack options setup: " + webRequest.error);
            RealTimeDebug?.Invoke("Failed soundtrack options setup");

            if (plusMusicAccount.DebugMode)
                Debug.LogFormat("> response = {0}", jsonString);
            //webRequest.Dispose();
        }
        else
        {
            if (plusMusicAccount.LogServerResponses)
                Debug.LogFormat("> response = {0}", jsonString);

            parentData = JsonUtility.FromJson<ParentProjectData>(jsonString);
            if (parentData.parent_id != null)
            {
                theSoundtrackOptions = SoundtrackOptionsForMenu();
                OnSoundTrackOptionsReceived?.Invoke(TheSoundtrackOptions);
                if (TheSoundtrackOptions != null && TheSoundtrackOptions.Length > 0) { 
                    SelectRemoteSoundtrackByID(TheSoundtrackOptions[0].id); 
                }
                RealTimeDebug?.Invoke("Successfully setup soundtrack options");
                DisplaySoundtrackOptions();
                //webRequest.Dispose();
            }
            else
            {
                //webRequest.Dispose();
                LoadProject(Int64.Parse(parentData.parent_id), projectKey, autoPlay);
            }
        }
    }

    public void DisplaySoundtrackOptions()
    {
        foreach (SoundtrackOptionData option in TheSoundtrackOptions)
        {
            if (plusMusicAccount.DebugMode)
            { 
                Debug.Log("soundtrack option id = " + option.id);
                Debug.Log("soundtrack option name = " + option.name);
                Debug.Log("soundtrack option is_licensed = " + option.is_licensed);
            }
        }
    }
 
    private SoundtrackOptionData[] SoundtrackOptionsForMenu()
    {
        SoundtrackOptionData[] theOptions = new SoundtrackOptionData[parentData.children_projects.Length];
        int loopIndexCounter = 0;
        foreach (ChildProjectData children in parentData.children_projects)
        {
            SoundtrackOptionData thisOption = new SoundtrackOptionData();
            thisOption.name = children.name;
            thisOption.id = children.id;
            thisOption.is_licensed = children.is_licensed;
            theOptions[loopIndexCounter] = thisOption;
            loopIndexCounter++;
        }
        return theOptions;
    }
    #endregion
    
    private bool HasReturning(PMTags tag)
    {
        return tag switch
        {
            PMTags.highlight => true,
            PMTags.victory => true,
            PMTags.failure => true,
            _ => false,
        };
    }
    
    private PMTags GetReturningSong(PMTags tag)
    {
        switch (tag)
        {
            case PMTags.highlight:
                return PMTags.low_backing;
            case PMTags.victory:
                return PMTags.none;
            case PMTags.failure:
                return PMTags.none;

        }
        return PMTags.none;
    }

    private IEnumerator PlaySoundPM_curve(TransitionInfo transition)
    {
        float delay = GetTimeForTransition(transition.timing);

        SetPingBackInfo(
            "transition", Int64.Parse(WhichSoundtrack), "", transition.tag.ToString(), 
            "CurveTransition", transition.timing.ToString(), delay, !string.IsNullOrEmpty(transition.stingerId)
        );

        if (playingCoroutines.ContainsKey("queueSong") && playingCoroutines["queueSong"] != null) 
        { 
            StopCoroutine(playingCoroutines["queueSong"]); 
        }
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(transition.stingerId)) 
            PlayStinger(transition.stingerId);

        if (HasReturning(transition.tag)) 
        { 
            playingCoroutines["queueSong"] = StartCoroutine(PlayNextQueueSong(transition.tag, GetReturningSong(transition.tag))); 
        }

        AudioClip nextAudioClip = null;
        if (map_AudioClip.ContainsKey(transition.tag.ToString())) 
        { 
            nextAudioClip = map_AudioClip[transition.tag.ToString()]; 
        }
        if (isAudioSource1Playing)
        {
            StartCoroutine(CurveTransitionFade(transition.curve, nextAudioClip, transition.durationTransition, newAudio, theAudioSource));
        }
        else
        {
            StartCoroutine(CurveTransitionFade(transition.curve, nextAudioClip, transition.durationTransition, theAudioSource, newAudio));
        }
        isAudioSource1Playing = !isAudioSource1Playing;
    }
    
    private IEnumerator PlayNextQueueSong(PMTags currentTag, PMTags nextTag)
    {
        if (map_AudioClip[currentTag.ToString()].length - defaultTransition.durationTransition > 0)
        {
            yield return new WaitForSeconds(map_AudioClip[currentTag.ToString()].length - defaultTransition.durationTransition);
            TransitionInfo transition = new TransitionInfo(nextTag)
            {
                timing = PMTimings.now
            };
            if (nextTag == PMTags.none) { transition.stingerId = ""; }

            PlayArrangement(transition);
        }
    }
    
    private float GetTimeForTransition(PMTimings theTiming)
    {
        return theTiming switch
        {
            PMTimings.beats => TimeNextBeat(),
            PMTimings.bars => TimeNextBar(),
            PMTimings.now => 0.0f,
            _ => 0.0f,
        };
    }
    
    IEnumerator CurveTransitionFade(AnimationCurve curve, AudioClip passedNextTrack, float duration, AudioSource audioSourceIn, AudioSource audioSourceDown)
    {
        audioSourceIn.clip = passedNextTrack;
        audioSourceIn.Play();
        float journey = 0.0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = percent;
            if (curve != null && curve.keys.Length >= 1) { curvePercent = curve.Evaluate(percent); }
            audioSourceIn.volume = curvePercent * musicVolume;
            audioSourceDown.volume = musicVolume * (1 - curvePercent);
            yield return null;
        }
    }
    
    IEnumerator CurveTransitionFadeAny(AnimationCurve curve, AudioClip passedNextTrack, float duration, AudioSource audioSourceIn)
    {
        audioSourceIn.clip = passedNextTrack;
        audioSourceIn.Play();
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = curve.Evaluate(percent);
            audioSourceIn.volume = curvePercent * musicVolume;
            yield return null;
        }
    }
    
    private void PlayStinger(string stingerId)
    {
        if (!map_StingerClip.ContainsKey(stingerId)) return;

        stingerSource.clip = map_StingerClip[stingerId];

        stingerSource.Play();
    }
    
    public void SetLowPassFilter(bool value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("CutOff", value ? 800f : 22000f);
        }
    }

    public void SetMixerSetting(string name, float value, float duration = 0)
    {
        if (audioMixer == null) 
        { 
            Debug.LogError("No audio mixer assigned in PlusMusic DJ!"); 
            return; 
        }

        if (duration == 0)
        {
            audioMixer.SetFloat(name, value);
        }
        else
        {
            audioMixer.GetFloat(name, out float initialValue);
            if (playingCoroutines.ContainsKey("effectCoroutine") && playingCoroutines["effectCoroutine"] != null) 
            { 
                StopCoroutine(playingCoroutines["effectCoroutine"]); 
            }
            playingCoroutines["effectCoroutine"] = StartCoroutine(SetMixerSettingProgressive(name, initialValue, value, duration));
        }
    }
    
    private IEnumerator SetMixerSettingProgressive(string name, float initialValue, float value, float duration = 0)
    {
        if (audioMixer != null)
        {
            for (float i = 0; i < duration; i += 0.1f)
            {
                yield return new WaitForSeconds(0.1f);
                audioMixer.SetFloat(name, Mathf.Lerp(initialValue, value, i / duration));
            }
            yield return new WaitForSeconds(0.1f);
            audioMixer.SetFloat(name, value);
        }
    }


    #region Public Plugin API functions
    //-----------------------------------------------
    // Public Plugin API functions
    //-----------------------------------------------

    /**
    * @brief [asynchronous] Get the project details json from the PlusMusic Web Api and set the global project variables
    * @param projectId
    * @param projectKey
    * @param autoPlay
    */
    public void LoadProject(Int64 projectId, string projectKey, bool autoPlay)
    {
        Debug.LogFormat("LoadProject({0})", projectId);

        SetCurrentProject(projectId, projectKey, autoPlay);

        //string finalURL = "https://app.plusmusic.ai/api/plugin/projects/" + soundtrackSelection + "/hierarchy" + "?plugin_version=" + PluginVersion;
        string finalURL = String.Format("{0}projects/{1}/hierarchy?plugin_version={2}", settings.base_url, projectId, PluginVersion);
        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("LoadProject(): finalURL = {0}", finalURL);

        StartCoroutine(RunSetupSoundtrackOptions(finalURL, projectId, projectKey, autoPlay));
    }

    /**
    * @brief [asynchronous] Load the soundtrack and all arrangements from the PlusMusic Web Api
    * @param soundtrackId
    */
    public void LoadSoundtrack(Int64 soundtrackId)
    {
        Debug.LogFormat("LoadSoundtrack({0})", soundtrackId);

        if (playingCoroutines.ContainsKey("getArragementCoroutine") && playingCoroutines["getArragementCoroutine"] != null)
        {
            Debug.LogWarning("LoadSoundtrack(): Already loading another track! Stopping previous load ...");
            StopCoroutine(playingCoroutines["getArragementCoroutine"]);
        }

        WhichSoundtrack = soundtrackId.ToString();
        loadedRemoteFiles = 0;

        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("> whichSoundtrack = {0}", WhichSoundtrack);

        map_AudioClip.Clear();
        map_AudioURLs.Clear();
        serverArrangementsFile = new ServerArrangementsData();
        arrangementURLFile     = new ArrangementURL();
        trackTimingsDataHold   = new TrackTimingsData();
        if (!licenseLimited)
        {
            playingCoroutines["getArragementCoroutine"] = StartCoroutine(LoadArrangements(soundtrackId));
        }
    }

    /**
    * @brief [asynchronous] Play an arrangement from a loaded soundtrack
    * @param arrangementType
    */
    public void PlayArrangement(TransitionInfo transitionInfo)
    {
        Debug.LogFormat("PlayArrangement({0})", transitionInfo.tag);

        next_tag = transitionInfo.tag.ToString();
        if (!transitionInfo.canTransitionToItself && transitionInfo.tag.ToString() == previousTag) { return; }
        loopCounter = 0;
        previousTag = next_tag;
        if (playingCoroutines.ContainsKey("transitionCoroutine") && playingCoroutines["transitionCoroutine"] != null) 
        { 
            StopCoroutine(playingCoroutines["transitionCoroutine"]); 
        }

        playingCoroutines["transitionCoroutine"] = StartCoroutine(PlaySoundPM_curve(transitionInfo));
    }

    /**
    * @brief Set the volume for the soundtrack playback (0.0 - 1.0)
    * @param value
    */
    public void SetVolume(float volume)
    {
        Debug.LogFormat("SetVolume({0})", volume);

        // Clamp volume
        if (volume <= 0.0f)
            volume = 0.001f;
        if (volume > 1.0f)
            volume = 1.0f;

        musicVolume = volume;

        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("> musicVolume = {0}", musicVolume);

        // Added null checks
        if (null != stingerSource)
            stingerSource.volume = volume;

        if (null != currentAudioSource)
            currentAudioSource.volume = volume;
    }

    /**
    * @brief Set mute true/false
    * @param value
    */
    public void SetMute(bool value)
    {
        Debug.LogFormat("SetMute({0})", value);

        float muteVal = (value ? 0.001f : musicVolume);

        // Added null checks
        if (null != theAudioSource)
            theAudioSource.mute = value;

        if (null != newAudio)
            newAudio.mute = value;

        if (null != stingerSource)
            stingerSource.mute = value;

    }

    /**
    * @brief Return true if the arrangement is loopable
    * @param arrangementType
    * @return true/false
    */
    public bool GetIsLoopable(PMTags arrangementType)
    {
        Debug.LogFormat("GetIsLoopable({0})", arrangementType);

        switch (arrangementType)
        {
            case PMTags.highlight:
                return false;
            case PMTags.failure:
                return false;
            case PMTags.victory:
                return false;
        }

        return true;
    }

    /**
    * @brief Returns the time of the next bar of the song
    * @return The next bar time
    */
    public float TimeNextBar()
    {
        float[] barCountsToUse;

        if (trackTimingsDataHold != null)
        {
            switch (previousTag)
            {
                case "high_backing": barCountsToUse = trackTimingsDataHold.high_backing_bars; break;
                case "low_backing": barCountsToUse = trackTimingsDataHold.low_backing_bars; break;
                case "backing_track": barCountsToUse = trackTimingsDataHold.backing_track_bars; break;
                case "preview": barCountsToUse = trackTimingsDataHold.preview_bars; break;
                case "victory": barCountsToUse = trackTimingsDataHold.victory_bars; break;
                case "failure": barCountsToUse = trackTimingsDataHold.failure_bars; break;
                case "highlight": barCountsToUse = trackTimingsDataHold.highlight_bars; break;
                case "lowlight": barCountsToUse = trackTimingsDataHold.lowlight_bars; break;
                case "full_song": barCountsToUse = trackTimingsDataHold.full_song_bars; break;
                default:
                    {
                        return 0.0f;
                    }
            }
        }
        else
        {
            return 0.0f;
        }

        float valueToSend = GetNextClosestTime(barCountsToUse);

        return valueToSend;
    }

    /**
    * @brief Returns the time of the next beat of the song
    * @return The next beat time
    */
    public float TimeNextBeat()
    {
        float[] beatCountsToUse;

        if (trackTimingsDataHold != null)
        {
            switch (previousTag)
            {
                case "high_backing": beatCountsToUse = trackTimingsDataHold.high_backing_beats; break;
                case "low_backing": beatCountsToUse = trackTimingsDataHold.low_backing_beats; break;
                case "backing_track": beatCountsToUse = trackTimingsDataHold.backing_track_beats; break;
                case "preview": beatCountsToUse = trackTimingsDataHold.preview_beats; break;
                case "victory": beatCountsToUse = trackTimingsDataHold.victory_beats; break;
                case "failure": beatCountsToUse = trackTimingsDataHold.failure_beats; break;
                case "highlight": beatCountsToUse = trackTimingsDataHold.highlight_beats; break;
                case "lowlight": beatCountsToUse = trackTimingsDataHold.lowlight_beats; break;
                case "full_song": beatCountsToUse = trackTimingsDataHold.full_song_beats; break;
                default:
                {
                    return 0.0f;
                }
            }
        }
        else
        {
            return 0.0f;
        }

        float valueToSend = GetNextClosestTime(beatCountsToUse);

        return valueToSend;
    }

    #endregion


    #region DEPRECATED Legacy functions
    //-----------------------------------------------
    // DEPRECATED Legacy functions
    // 
    // NOTE: The current demos have hardcoded references to these old functions
    //-----------------------------------------------

    /**
    * @deprecated Use LoadSoundtrack() instead
    * @brief Change the soundtrack
    * @param theSoundtrack
    */
    public void SelectRemoteSoundtrackByID(string theSoundtrack)
    {
        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("SelectRemoteSoundtrackByID({0})", theSoundtrack);

        LoadSoundtrack(Int64.Parse(theSoundtrack));
    }

    /**
    * @deprecated Use PlayArrangement() instead
    * @brief Play sound by type
    * @param transitionInfo
    */
    public void PlaySoundPM(TransitionInfo transitionInfo)
    {
        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("PlaySoundPM({0})", transitionInfo.tag);

        PlayArrangement(transitionInfo);
    }

    /**
    * @deprecated Use SetVolume() instead
    * @brief Set the music volume
    * @param value
    */
    public void SetMusicVolume(float value)
    {
        if (plusMusicAccount.DebugMode)
            Debug.LogFormat("SetMusicVolume({0})", value);

        SetVolume(value);
    }

    #endregion
}







#region Data Classes

namespace PlusMusic
{
    [System.Serializable]
    public class TransitionInfo
    {
        public PlusMusic_DJ.PMTags tag       = PlusMusic_DJ.PMTags.low_backing;
        public float durationTransition      = 1.0f;
        public PlusMusic_DJ.PMTimings timing = PlusMusic_DJ.PMTimings.bars;
        public string stingerId              = "";
        public bool canTransitionToItself    = true;
        public AnimationCurve curve;

        public TransitionInfo(PlusMusic_DJ.PMTags tag)
        {
            this.tag = tag;

            if (PlusMusic_DJ.Instance != null)
            {
                this.durationTransition = PlusMusic_DJ.Instance.defaultTransition.durationTransition;
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.timing = PlusMusic_DJ.Instance.defaultTransition.timing;
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, float durationTransition)
        {
            this.tag = tag;
            this.durationTransition = durationTransition;
            if (PlusMusic_DJ.Instance != null)
            {
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.timing = PlusMusic_DJ.Instance.defaultTransition.timing;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, bool canTransitionToItself)
        {
            this.tag = tag;
            this.canTransitionToItself = canTransitionToItself;
            if (PlusMusic_DJ.Instance != null)
            {
                this.durationTransition = PlusMusic_DJ.Instance.defaultTransition.durationTransition;
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.timing = PlusMusic_DJ.Instance.defaultTransition.timing;
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, float durationTransition, PlusMusic_DJ.PMTimings timing)
        {
            this.tag = tag;
            this.durationTransition = durationTransition;
            this.timing = timing;
            if (PlusMusic_DJ.Instance != null)
            {
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, float durationTransition, AnimationCurve curve)
        {
            this.tag = tag;
            this.durationTransition = durationTransition;
            this.curve = curve;
            if (PlusMusic_DJ.Instance != null)
            {
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.timing = PlusMusic_DJ.Instance.defaultTransition.timing;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, PlusMusic_DJ.PMTimings timing)
        {
            this.tag = tag;
            this.timing = timing;
            if (PlusMusic_DJ.Instance != null)
            {
                this.durationTransition = PlusMusic_DJ.Instance.defaultTransition.durationTransition;
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, AnimationCurve curve)
        {
            this.tag = tag;
            this.curve = curve;
            if (PlusMusic_DJ.Instance != null)
            {
                this.durationTransition = PlusMusic_DJ.Instance.defaultTransition.durationTransition;
                this.stingerId = PlusMusic_DJ.Instance.defaultTransition.stingerId;
                this.timing = PlusMusic_DJ.Instance.defaultTransition.timing;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, string stingerId)
        {
            this.tag = tag;
            this.stingerId = stingerId;
            if (PlusMusic_DJ.Instance != null)
            {
                this.durationTransition = PlusMusic_DJ.Instance.defaultTransition.durationTransition;
                this.timing = PlusMusic_DJ.Instance.defaultTransition.timing;
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }
        
        public TransitionInfo(PlusMusic_DJ.PMTags tag, float durationTransition, PlusMusic_DJ.PMTimings timing, string stingerId)
        {
            this.tag = tag;
            this.durationTransition = durationTransition;
            this.stingerId = stingerId;
            this.timing = timing;
            if (PlusMusic_DJ.Instance != null)
            {
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
                this.canTransitionToItself = PlusMusic_DJ.Instance.defaultTransition.canTransitionToItself;
            }
        }

        public TransitionInfo(PlusMusic_DJ.PMTags tag, float durationTransition, PlusMusic_DJ.PMTimings timing, string stingerId, bool canTransitionToItself)
        {
            this.durationTransition = durationTransition;
            this.stingerId = stingerId;
            this.tag = tag;
            this.timing = timing;
            this.canTransitionToItself = canTransitionToItself;
            if (PlusMusic_DJ.Instance != null)
            {
                this.curve = PlusMusic_DJ.Instance.defaultTransition.curve;
            }
        }

        public TransitionInfo(PlusMusic_DJ.PMTags tag, float durationTransition, PlusMusic_DJ.PMTimings timing, string stingerId, bool canTransitionToItself, AnimationCurve curve)
        {
            this.durationTransition = durationTransition;
            this.stingerId = stingerId;
            this.tag = tag;
            this.timing = timing;
            this.curve = curve;
            this.canTransitionToItself = canTransitionToItself;
        }
    }
    
    [System.Serializable]
    public class TrackTimingsData
    {
        public float[] victory_beats;
        public float[] victory_bars;
        public float[] low_backing_beats;
        public float[] low_backing_bars;
        public float[] high_backing_beats;
        public float[] high_backing_bars;
        public float[] backing_track_beats;
        public float[] backing_track_bars;
        public float[] failure_beats;
        public float[] failure_bars;
        public float[] highlight_beats;
        public float[] highlight_bars;
        public float[] lowlight_beats;
        public float[] lowlight_bars;
        public float[] preview_bars;
        public float[] preview_beats;
        public float[] full_song_bars;
        public float[] full_song_beats;
    }
    
    [System.Serializable]
    class SavePlayInfo
    {
        public string savedData;
    }
    
    [System.Serializable]
    public class ServerArrangementsData
    {
        public Int64  id;
        public string name;
        public string type_id;
        public Int64  parent_id;
        public Int64  creator_id;
        public bool   is_licensed;
        public string licensed_by;
        public string licensed_at;
        public string deleted_at;
        public string created_at;
        public string updated_at;
        public Arrangements[] arrangements;
        public string message;
    }
    
    [System.Serializable]
    public class Arrangements
    {
        public Int64   id;
        public Int64   container_id;
        public Int64   created_by_user_id;
        public int     version;
        public Int64[] song_clips;
        public Int64   pipeline_run_id;
        public bool    is_instrumental;
        public string  created_at;
        public string  updated_at;
        public string  deleted_at;
        public float[] beats;
        public float[] bars;
        public Pivot   pivot;
        public Container container;
    }
    
    [System.Serializable]
    public class Pivot
    {
        public int    project_id;
        public int    arrangement_id;
        public string created_at;
        public string updated_at;
        public int    is_active;
    }
    
    [System.Serializable]
    public class Container
    {
        public int    id;
        public string name;
        public int    type_id;
        public int    based_on_container_id;
        public int    is_template;
        public string created_at;
        public string updated_at;
    }
    
    [System.Serializable]
    public class ArrangementURL
    {
        public string message;
        public string arrangement_type;
        public int    arrangement_type_id;
        public string arrangement_url;
    }
    
    [System.Serializable]
    public class AuthenticationData
    {
        public string access_token;
        public string expires_in;
        public string first_name;
        public string last_name;
        public string email;
    }
    
    [System.Serializable]
    public class ParentProjectData
    {
        public string id;
        public string name;
        public string type_id;
        public string parent_id;
        public string template_project_id;
        public string creator_id;
        public bool   is_licensed;
        public string licensed_by;
        public string licensed_at;
        public string deleted_at;
        public string created_at;
        public string updated_at;
        public string last_compilation_requested_at;
        public string root_project_id;
        public ChildProjectData[] children_projects;
    }
    
    [System.Serializable]
    public class ChildProjectData
    {
        public string id, name;
        public string type_id;
        public string parent_id;
        public string template_project_id;
        public string creator_id;
        public string licensed_by;
        public string licensed_at;
        public string deleted_at;
        public string created_at;
        public string updated_at;
        public string last_compilation_requested_at;
        public string root_project_id;
        public bool is_licensed;
    }
    
    [System.Serializable]
    public class SoundtrackOptionData
    {
        public string id;
        public string name;
        public bool   is_licensed;
    }
    
    [Serializable]
    public class PackageData
    {
        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public string unityRelease;
        public string documentationUrl;
        public string changelogUrl;
        public string licensesUrl;
        public string[] dependencies;
        public string[] keywords;
        public PackageAuthor author;
    }
    
    [Serializable]
    public class PackageAuthor
    {
        public string name;
        public string email;
        public string url;
    }

    [Serializable]
    public class PingBack
    {
        public string os;
        public string event_text;
        public string device_id;
        public bool   in_editor;
        public string platform;
        public string title;
        public string connected;
        public bool   is_using_stinger;
        public Int64  project_id;
        public Int64  arrangement_id;
        public string arrangement_type;
        public string transition_type;
        public string transition_timing;
        public float  transition_delay;
        public string time;
        public string web_url;
        public string plugin_version;
        public string play_id;
    }

    // PlusMusic settings
    [Serializable]
    public class PM_Settings
    {
        public string target;
        public string username;
        public string password;
        public Int64  project_id;
        public string api_key;
        public bool   auto_play;
        public string credentials;
        public string base_url;
    }

    #endregion
}
