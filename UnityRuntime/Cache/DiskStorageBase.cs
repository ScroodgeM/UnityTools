using System;
using System.Threading;
using UnityEngine;

namespace UnityTools.UnityRuntime.Cache
{
    internal abstract class DiskStorageBase<T>
    {
        private enum State : byte
        {
            StopSignal = 0,
            Idle = 55,
            WriteDataScheduled = 75,
            WipeDataScheduled = 76,
        }

        private readonly DiskStorageSettings settings;
        private readonly string filePath;

        private object locker = new object();
        private State currentState = State.Idle;
        private DateTime nextWriteToDiskTime = DateTime.UtcNow;
        private T dataToWrite;

        internal DiskStorageBase(string cacheId, IUnityInvocations unityInvocations, DiskStorageSettings settings, string fileExtension)
        {
            this.settings = settings;

            string folderPath = Application.persistentDataPath;
            if (string.IsNullOrEmpty(settings.customSubfolder) == false)
            {
                folderPath = System.IO.Path.Combine(folderPath, settings.customSubfolder);
            }

            System.IO.Directory.CreateDirectory(folderPath);

            this.filePath = System.IO.Path.Combine(folderPath, $"{cacheId}.{fileExtension}");

            if (settings.useDedicatedThread == true)
            {
                new Thread(WriterThread).Start();

                unityInvocations.OnDestroyEvent += () =>
                {
                    lock (locker)
                    {
                        WriteDataToDiskNow();
                        currentState = State.StopSignal;
                    }
                };
            }
            else
            {
                unityInvocations.OnUpdateEvent += WriteDataToDisk;
                unityInvocations.OnApplicationPauseEvent += WriteDataToDiskNow;
                unityInvocations.OnDestroyEvent += WriteDataToDiskNow;
            }
        }

        internal bool TryLoad(out T value)
        {
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
            lock (locker)
            {
                dataToWrite = value;
                currentState = State.WriteDataScheduled;
            }
        }

        internal void Clear()
        {
            lock (locker)
            {
                dataToWrite = default;
                currentState = State.WipeDataScheduled;
            }
        }

        protected abstract T ReadDataFromDisk(string filePath);

        protected abstract void WriteDataToDisk(string filePath, T data);

        private void WriteDataToDisk()
        {
            if (currentState == State.Idle || nextWriteToDiskTime < DateTime.UtcNow)
            {
                return;
            }

            WriteDataToDiskNow();
            nextWriteToDiskTime = DateTime.UtcNow.AddSeconds(settings.writeToDiskInterval);
        }

        private void WriterThread()
        {
            while (currentState != State.StopSignal)
            {
                if (currentState == State.Idle)
                {
                    Thread.Sleep(settings.writeToDiskInterval * 1000);
                    continue;
                }

                lock (locker)
                {
                    WriteDataToDiskNow();
                }
            }
        }

        private void WriteDataToDiskNow()
        {
            switch (currentState)
            {
                case State.WipeDataScheduled:
                    System.IO.File.Delete(filePath);
                    currentState = State.Idle;
                    break;

                case State.WriteDataScheduled:
                    WriteDataToDisk(filePath, dataToWrite);
                    currentState = State.Idle;
                    break;
            }
        }
    }
}
