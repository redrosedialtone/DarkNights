using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Nebula.Systems;

namespace Dark
{
    public interface IJsonSaveUtility
    {
        string SaveToJson();
        string LoadFromJson(string JSON);
    }

    public class FileManager : IManager
    {
        #region Static
        private static FileManager instance;
        public static FileManager Get => instance;
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[FILESYS]");
        #endregion

        public static string JSONFileDirectory => ApplicationController.DataPath + "/JSON/";

        public bool Initialized => throw new NotImplementedException();

        public void Init()
        {
            log.Info("> File Manager Init.. <");
            instance = this;
        }

        public bool WriteToFile(string file, string dir, string fileName, string extension, bool overwrite)
        {
            string path = dir + fileName + extension;
            Stream Stream = new FileStream(dir, FileMode.CreateNew, FileAccess.Write, FileShare.Write);
            log.Info($"Saving {fileName} at {path}...");

            if (File.Exists(path))
            {
                if (overwrite)
                {
                    log.Info("<color=blue>File Already Exists! Overwritting...</color>");
                    File.Delete(path);
                }
                else
                {
                    log.Info("<color=red>File Already Exists! Aborting</color>");
                    return false;
                }
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(Stream))
                {
                    writer.Write(file);
                }
                return true;
            }
            catch (Exception)
            {
                throw new IOException("Failed to Serialize Path::" + path);
            }
        }

        public bool ReadFromFile(string dir, string fileName, string extension, out string JSON)
        {
            string path = dir + fileName + extension;
            log.Info($"<color=blue>Deserializing file at {path}</color>");
            if (!File.Exists(path))
            {
                log.Warn($"Invalid path::{path}");
                JSON = null;
                return false;
            }

            Stream Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                using(StreamReader reader = new StreamReader(Stream))
                {
                    JSON = reader.ReadToEnd();
                }
                return true;
            }
            catch (Exception)
            {
                throw new FileLoadException("Could not load object at::" + path);
            }
        }

        public string[] GetFilesAtDir(string path, string[] extensions)
        {
            List<string> fileNames = new List<string>();
            if (Directory.Exists(path))
            {
                var info = new DirectoryInfo(path);
                foreach (var file in info.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    if (IsValidFile(file.Name, extensions))
                    {
                        fileNames.Add(Path.GetFileNameWithoutExtension(file.Name));
                    }
                }
            }
            else
            {
                log.Error("Event Directory Missing - " + path);
            }
            return fileNames.ToArray();
        }

        private bool IsValidFile(string file, string[] extensions)
        {
            foreach (var extension in extensions)
            {
                log.Trace("Checking Extension::" + extension);
                if (extension == Path.GetExtension(file))
                {
                    return true;
                }
            }
            return false;
        }

        public bool DeleteFile(string dir, string fileName, string extension)
        {
            string path = dir + fileName + extension;
            if (File.Exists(path))
            {
                log.Trace("Deleting file::" + path);
                File.Delete(path);
                return true;
            }
            return false;
        }

        public string JSONSerialize(object graph)
        {
            log.Trace($"<color=blue>Serializing {graph}...</color>");
            string JSON = "";//REFACTOR:JsonUtility.ToJson(graph);
            return JSON;
        }

        public void OnInitialized()
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }
    }
}
