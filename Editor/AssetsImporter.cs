using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetsExportingHelpers;

namespace AssetsExporting
{

    public sealed class AssetsImporter
    {

        public string DestFilesDirectory { get; private set; }

        public AssetsImporter(string destFilesDirectory)
        {
            DestFilesDirectory = destFilesDirectory;
        }

        public void Import(string descriptorPath)
        {
            var assetDeserializer = new AssetDefinitionDeserializer(descriptorPath);
            var assets = assetDeserializer.Deserialize();

            var copyingHelper = new AssetsCopyingHelper(DestFilesDirectory);
            foreach(var assetDefinition in assets)
                copyingHelper.CopyAsset(assetDefinition.AssetPath);
        }

    }

}
