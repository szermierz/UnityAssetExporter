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
                File.Copy(sourcePath, destPath);
                File.Copy(GetMetaFilePath(sourcePath), GetMetaFilePath(destPath));
            }
            catch(Exception)
            { }
            UnityEditor.AssetDatabase.StopAssetEditing();
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
            return Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + ".meta";
        }


    }

}
