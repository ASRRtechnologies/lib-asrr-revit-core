﻿using System.Collections.Generic;
using System.IO;
using ASRR.Revit.Core.Exporter.GLTF.Model;
using dracowrapper;

namespace ASRR.Revit.Core.Exporter.GLTF.Export
{
    public static class Draco
    {
        public static void Compress(Preferences preferences)
        {
            List<string> files = new List<string>();
            string fileToCompress;
            string fileToCompressTemp;

            if (preferences.format == FormatEnum.gltf)
            {
                fileToCompress = string.Concat(preferences.path, ".gltf");
                fileToCompressTemp = string.Concat(preferences.path, "Temp.gltf");

                files.Add(string.Concat(preferences.path, ".bin"));
                files.Add(fileToCompress);
            }
            else
            {
                fileToCompress = string.Concat(preferences.path, ".glb");
                fileToCompressTemp = string.Concat(preferences.path, "Temp.glb");
                files.Add(fileToCompress);
            }

            var decoder = new GltfDecoder();
            var res = decoder.DecodeFromFileToScene(fileToCompress);
            var scene = res.Value();
            DracoCompressionOptions options = new DracoCompressionOptions();
            SceneUtils.SetDracoCompressionOptions(options, scene);
            var encoder = new GltfEncoder();
            encoder.EncodeSceneToFile(scene, fileToCompressTemp);

            files.ForEach(x => File.Delete(x));
            File.Move(fileToCompressTemp, fileToCompress);

            if (preferences.format == FormatEnum.gltf)
            {
                string binToReplace = fileToCompressTemp.Replace(".gltf", ".bin");
                string binFinalName = fileToCompressTemp.Replace("Temp.gltf", ".bin");
                File.Move(binToReplace, binFinalName);

                string text = File.ReadAllText(fileToCompress);
                text = text.Replace(Path.GetFileName(binToReplace), Path.GetFileName(binFinalName));
                File.WriteAllText(fileToCompress, text);
            }
        }
    }
}
