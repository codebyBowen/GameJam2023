using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PlusMusic
{
    public class PlusMusicSettingsSo : ScriptableObject
    {
        [SerializeField] private Int64  _projectId;
        [SerializeField] private string _apiKey;
        [SerializeField] private float  _musicVolume;
        [SerializeField] private bool   _autoLoadProject;
        [SerializeField] private bool   _autoPlayProject;
        [SerializeField] private bool   _debugMode;
        [SerializeField] private bool   _logServerResponses;

        public TextAsset jsonPackage;
        [SerializeField] private PackageData packageData;

        public Int64  ProjectId          => _projectId;
        public string ApiKey             => _apiKey;
        public float  MusicVolume        => _musicVolume;
        public bool   AutoLoadProject    => _autoLoadProject;
        public bool   AutoPlayProject    => _autoPlayProject;
        public bool   DebugMode          => _debugMode;
        public bool   LogServerResponses => _logServerResponses;

        public PackageData PackageData => packageData;

        public void SaveData(
            Int64 projectId, string apiKey,
            float musicVolume, bool autoLoadProject, bool autoPlayProject, bool debugMode, bool logServerResponses)
        {
            _projectId          = projectId;
            _apiKey             = apiKey;
            _musicVolume        = musicVolume;
            _autoLoadProject    = autoLoadProject;
            _autoPlayProject    = autoPlayProject;
            _debugMode          = debugMode;
            _logServerResponses = logServerResponses;
        }

        public void SetPackageData()
        {
            if (jsonPackage != null)
            {
                packageData = JsonUtility.FromJson<PackageData>(jsonPackage.text);
            }
        }
    }

}
