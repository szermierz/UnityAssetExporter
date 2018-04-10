using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetsExportingHelpers
{

    public class AssetsCopyingHelper
    {

        public string DestAssetsPath { get; set; }

        public AssetsCopyingHelper(string destAssetsPath)
        {
            DestAssetsPath = destAssetsPath;

            BuildAssetsRecords();
        }

        protected Dictionary<string, HashSet<string>> m_AssetPaths = new Dictionary<string, HashSet<string>>();

        protected virtual void BuildAssetsRecords()
        {
            m_AssetPaths.Clear();

            var allAssetPaths = UnityEditor.AssetDatabase.GetAllAssetPaths();
            foreach(var path in allAssetPaths)
            {
                if(Path.GetExtension(path).Equals(""))
                    continue;

                var filename = Path.GetFileName(path);

                if(!m_AssetPaths.ContainsKey(filename))
                    m_AssetPaths.Add(filename, new HashSet<string>());

                var paths = m_AssetPaths[filename];
                paths.Add(path);
            }
        }

        public void CopyAsset(string sourcePath)
        {
            string filename = Path.GetFileName(sourcePath);

            if(m_AssetPaths.ContainsKey(filename))
            {
                var duplicatedFiles = m_AssetPaths[filename];
                var matchingFiles = duplicatedFiles.Where(duplictedFilePath => DoesFilesMatch(sourcePath, duplictedFilePath)).ToList();

                if(matchingFiles.Count == 1)
                {
                    var duplicatedFilePath = matchingFiles[0];

                    bool moveToNewDir = EditorUtility.DisplayDialog("Asset duplication detected!", filename, "Move to new directory", "Keep previous directory");
                    if(moveToNewDir)
                    {
                        UnityEditor.AssetDatabase.StartAssetEditing();
                        UnityEditor.AssetDatabase.DeleteAsset(duplicatedFilePath);
                    }
                    else//"Keep asset at previous directory"
                    {
                        return;
                    }
                }
                else if(matchingFiles.Count > 1)
                {
                    Debug.LogError("[AssetsCopying] Multiple assets with the same guid detected! Skipping a file (" + sourcePath + ")! Assets:");

                    foreach(var asset in matchingFiles)
                        Debug.LogError("[AssetsCopying] Guid duplication: " + asset);

                    return;
                }
            }

            string destPath = DestAssetsPath + "/" + filename;

            UnityEditor.AssetDatabase.StartAssetEditing();
            try
            {
                CopyAssetFile(sourcePath, destPath);
            }
            finally
            {
                UnityEditor.AssetDatabase.StopAssetEditing();
            }
        }

        protected virtual void CopyAssetFile(string sourcePath, string destPath)
        {
            PrepareDirectoryToFileCopying(ref sourcePath, false);
            PrepareDirectoryToFileCopying(ref destPath, true);

            if(!File.Exists(sourcePath))
                return;

            FileUtil.CopyFileOrDirectory(sourcePath, destPath);
            FileUtil.CopyFileOrDirectory(GetMetaFilePath(sourcePath), GetMetaFilePath(destPath));
        }

        protected virtual void PrepareDirectoryToFileCopying(ref string path, bool ensureDirectoryExists)
        {
            path = path.Replace('\\', '/');

            var pathDirectories = path.Split('/');
            if(pathDirectories.Length < 2)
                return;

            if(!ensureDirectoryExists)
                return;

            var subPath = pathDirectories[0];
            for(int i = 1; i < pathDirectories.Length - 1; ++i)
            {
                subPath += "/" + pathDirectories[i];

                if(File.Exists(subPath) || Directory.Exists(subPath))
                    continue;

                Directory.CreateDirectory(subPath);
            }
        }

        public virtual bool DoesFilesMatch(string lhvAbsolutePath, string rhvAbsolutePath)
        {
            //Func<string, string> computeFileHash = delegate(string filePath)
            //{
            //    FileStream file = File.OpenRead(filePath);
            //    return BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(file));
            //};
             
            Func<string, string> getGuid = delegate(string filePath)
            {

                //return UnityEditor.AssetDatabase.AssetPathToGUID(filePath); // no use due to files contained outside of Assets folder

                var metaFilePath = GetMetaFilePath(filePath);
                string[] metaFileContent = new string[0];
                try
                { metaFileContent = File.ReadAllLines(metaFilePath); }
                catch(Exception)
                { }
                 
                foreach(var line in metaFileContent)
                {
                    var trimmedLine = line.Trim();

                    if(!trimmedLine.StartsWith("guid:"))
                        continue;

                    var guid = trimmedLine.Substring(("guid:").Length).Trim();
                    return guid;
                }

                return "";
            };

            //if(!computeFileHash(lhvAbsolutePath).Equals(computeFileHash(rhvAbsolutePath)))
            //    return false;

            if(!getGuid(lhvAbsolutePath).Equals(getGuid(rhvAbsolutePath)))
                return false;

            return true;
        }

        private string GetMetaFilePath(string filePath)
        {
            return filePath + ".meta";
        }


    }

}
