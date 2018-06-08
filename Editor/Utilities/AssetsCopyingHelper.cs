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

        protected sealed class DuplicatedAssetsBehaviour
        {

            public string DuplicationFilesDirectory = "";

            private sealed class DuplicatedBehaviourDecision
            {
                private int? m_DecisionID;

                public readonly string DuplicationFilesDirectory;

                public DuplicatedBehaviourDecision(string _Extension, string _DuplicationFilesDirectory)
                {
                    DuplicationFilesDirectory = _DuplicationFilesDirectory;
                }

                public void Execute(string duplicatedFilePath, ref ExpectedDuplicationHandling result)
                {
                    var duplicatedFileName = Path.GetFileName(duplicatedFilePath);

                    if(!m_DecisionID.HasValue)
                        m_DecisionID = EditorUtility.DisplayDialogComplex("Asset duplication detected!",
                                                          "File:  " + duplicatedFileName + "  is duplicated.\n" + 
                                                          "Would you like to move it to the new directory, keep it at previous directory (skip importing this asset), or move it to shared duplications directory?",
                                                          "New", "Previous", "Shared duplications");

                    switch(m_DecisionID.Value)
                    {
                    case 0: // "Move to new directory"
                        UnityEditor.AssetDatabase.DeleteAsset(duplicatedFilePath);
                        break;
                    case 1:// "Keep asset at previous directory"
                        result.ShouldCopyFileNormal = false;
                        break;
                    case 2:// "Move to shared duplications directory"
                        UnityEditor.AssetDatabase.DeleteAsset(duplicatedFilePath);
                        result.DestDirectoryOverride = DuplicationFilesDirectory;
                        break;
                    default:
                        Debug.LogError("[AssetsCopyingHelper] Duplications resolving error! Incorrect decision result!");
                        break;
                    }
                }
            }

            static Dictionary<string, DuplicatedBehaviourDecision> s_SavedExtensionsDecisions = new Dictionary<string, DuplicatedBehaviourDecision>();

            public class ExpectedDuplicationHandling
            {
                public bool ShouldCopyFileNormal = true;
                public string DestDirectoryOverride = "";
            }

            public ExpectedDuplicationHandling NotifyDuplications(string sourcePath, List<string> duplicatedFilePaths)
            {
                string filename = Path.GetFileName(sourcePath);
                string extension = Path.GetExtension(filename);
                ExpectedDuplicationHandling result = new ExpectedDuplicationHandling();

                if(duplicatedFilePaths.Count == 1)
                {
                    var duplicatedFilePath = duplicatedFilePaths[0];

                    DuplicatedBehaviourDecision decision = null;
                    if(s_SavedExtensionsDecisions.ContainsKey(extension))
                        decision = s_SavedExtensionsDecisions[extension];
                    else
                        decision = new DuplicatedBehaviourDecision(extension, DuplicationFilesDirectory);

                    decision.Execute(duplicatedFilePath, ref result);

                    if(!s_SavedExtensionsDecisions.ContainsKey(extension) && 
                       EditorUtility.DisplayDialog("Remember decision", 
                                                   "Do you want to remember this decision for further \"*" + extension + "\"duplications resolving?", 
                                                   "Yes", "No"))
                    {
                        s_SavedExtensionsDecisions.Add(extension, decision);
                    }
                }
                else if(duplicatedFilePaths.Count > 1)
                {
                    Debug.LogError("[AssetsCopying] Multiple assets with the same guid detected! Skipping a file (" + sourcePath + ")! Assets:");

                    foreach(var asset in duplicatedFilePaths)
                        Debug.LogError("[AssetsCopying] Guid duplication: " + asset);

                    result.ShouldCopyFileNormal = false;
                }

                return result;
            }
        }

        protected DuplicatedAssetsBehaviour m_DuplicationsSolver = new DuplicatedAssetsBehaviour();

        public string DestAssetsPath { get; set; }

        private string m_DuplicationsFolderName = "";
        public string DuplicationsFolderName
        {
            get { return m_DuplicationsFolderName; }
            set
            {
                if(m_DuplicationsFolderName.Equals(value))
                    return;

                m_DuplicationsFolderName = value;

                var allDirectoriesPath = DestAssetsPath;
                allDirectoriesPath = allDirectoriesPath.Substring(0, allDirectoriesPath.LastIndexOf("/"));

                m_DuplicationsSolver.DuplicationFilesDirectory = allDirectoriesPath + "/" + value;
            }
        }

        public AssetsCopyingHelper(string destAssetsPath, string duplicationsFolderName = "Common")
        {
            DestAssetsPath = destAssetsPath;
            DuplicationsFolderName = duplicationsFolderName;

            BuildAssetsRecords();
        }

        protected Dictionary<string, HashSet<string>> m_AssetPaths = new Dictionary<string, HashSet<string>>();

        protected virtual void BuildAssetsRecords()
        {
            m_AssetPaths.Clear();

            UnityEditor.AssetDatabase.Refresh();
            var allAssetPaths = UnityEditor.AssetDatabase.GetAllAssetPaths();
            allAssetPaths = allAssetPaths.Select(x => x.ToLower()).ToArray();

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

        protected virtual DuplicatedAssetsBehaviour.ExpectedDuplicationHandling ResolveDuplications(string filename, string sourcePath)
        {
            var duplicatedFiles = m_AssetPaths[filename];
            var matchingFiles = duplicatedFiles.Where(duplictedFilePath => DoesFilesMatch(sourcePath, duplictedFilePath)).ToList();

            var duplicationsSolvingResult = m_DuplicationsSolver.NotifyDuplications(sourcePath, matchingFiles);

            return duplicationsSolvingResult;
        }

        public void CopyAsset(string sourcePath)
        {
            string caseSensitiveFilename = Path.GetFileName(sourcePath);
            sourcePath = sourcePath.ToLower();
            string filename = Path.GetFileName(sourcePath);
            string destPath = DestAssetsPath + "/" + caseSensitiveFilename;

            if(m_AssetPaths.ContainsKey(filename))
            {
                var duplicationsSolvingResult = ResolveDuplications(filename, sourcePath);
                if(!duplicationsSolvingResult.ShouldCopyFileNormal)
                    return;

                if(!duplicationsSolvingResult.DestDirectoryOverride.Equals(""))
                    destPath = duplicationsSolvingResult.DestDirectoryOverride + "/" + caseSensitiveFilename;
            }

            UnityEditor.AssetDatabase.StartAssetEditing();
            try { CopyAssetFile(sourcePath, destPath); }
            finally { UnityEditor.AssetDatabase.StopAssetEditing(); }
        }

        protected virtual void CopyAssetFile(string sourcePath, string destPath)
        {
            PrepareDirectoryToFileCopying(ref sourcePath, false);

            if(!File.Exists(sourcePath))
                return;

            PrepareDirectoryToFileCopying(ref destPath, true);

            try { FileUtil.CopyFileOrDirectory(sourcePath, destPath); }
            catch(Exception) { }

            try { FileUtil.CopyFileOrDirectory(GetMetaFilePath(sourcePath), GetMetaFilePath(destPath)); }
            catch(Exception) { }
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

        protected virtual bool DoesFilesMatch(string lhvAbsolutePath, string rhvAbsolutePath)
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
