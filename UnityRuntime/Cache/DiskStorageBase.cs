//this empty line for UTF-8 BOM header

using System;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal abstract class DiskStorageBase<T>
    {
        private const int IDLE_TIME_BEFORE_SAVE_TO_DISK = 5;
        private const int MAX_SECONDS_WITHOUT_SAVE_TO_DISK = 60;

        private readonly string cacheId;

        internal DiskStorageBase(string cacheId, IUnityInvocations unityInvocations)
        {
            this.cacheId = cacheId;

            unityInvocations.OnUpdateEvent += Update;
            unityInvocations.OnApplicationPauseEvent += TrySaveNow;
            unityInvocations.OnDestroyEvent += TrySaveNow;
        }

        private float? firstSaveCommandTime = null;
        private float? lastSaveCommandTime = null;

        private bool saveCandidateExists = false;
        private T saveCandidate;

        internal bool TryLoad(out T value)
        {
            string filePath = GetFilePath();

            if (System.IO.File.Exists(filePath) == false)
            {
                value = default;
                return false;
            }

            try
            {
                value = ReadDataFromDisk(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                value = default;
                return false;
            }
        }

        internal void Save(T value)
        {
            if (firstSaveCommandTime.HasValue == false)
            {
                firstSaveCommandTime = Time.unscaledTime;
            }

            lastSaveCommandTime = Time.unscaledTime;

            saveCandidateExists = true;
            saveCandidate = value;
        }

        internal void Clear()
        {
            if (firstSaveCommandTime.HasValue == false)
            {
                firstSaveCommandTime = Time.unscaledTime;
            }

            lastSaveCommandTime = Time.unscaledTime;

            saveCandidateExists = false;
            saveCandidate = default;
        }

        protected abstract string GetFileExtension();

        protected abstract T ReadDataFromDisk(string filePath);

        protected abstract void WriteDataToDisk(string filePath, T data);

        private void Update()
        {
            if (
                firstSaveCommandTime.HasValue && Time.unscaledTime > firstSaveCommandTime.Value + MAX_SECONDS_WITHOUT_SAVE_TO_DISK
                ||
                lastSaveCommandTime.HasValue && Time.unscaledTime > lastSaveCommandTime.Value + IDLE_TIME_BEFORE_SAVE_TO_DISK
            )
            {
                SaveNow();
            }
        }

        private void TrySaveNow()
        {
            if (firstSaveCommandTime.HasValue == true || lastSaveCommandTime.HasValue == true)
            {
                SaveNow();
            }
        }

        private void SaveNow()
        {
            string filePath = GetFilePath();

            if (saveCandidateExists == true)
            {
                WriteDataToDisk(filePath, saveCandidate);
                saveCandidate = default;
            }
            else
            {
                System.IO.File.Delete(filePath);
            }

            firstSaveCommandTime = null;
            lastSaveCommandTime = null;
        }

        private string GetFilePath() => System.IO.Path.Combine(Application.persistentDataPath, $"{cacheId}.{GetFileExtension()}");
    }
}
