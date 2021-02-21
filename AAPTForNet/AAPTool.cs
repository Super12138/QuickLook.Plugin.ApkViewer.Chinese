﻿using AAPTForNet.Models;

using System;
using System.Collections.Generic;
using System.IO;

namespace AAPTForNet {
    /// <summary>
    /// Android Assert Packing Tool for NET
    /// </summary>
    public class AAPTool : System.Diagnostics.Process {
        private enum DumpTypes {
            Manifest = 0,
            Resources = 1,
            ManifestTree = 2,
        }
                
        private static readonly string AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);

        protected AAPTool() {
            this.StartInfo.FileName = AppPath + @"\tool\aapt.exe";
            this.StartInfo.CreateNoWindow = true;
            this.StartInfo.UseShellExecute = false; // For read output data
            this.StartInfo.RedirectStandardError = true;
            this.StartInfo.RedirectStandardOutput = true;
            this.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding("utf-8");
        }

        protected new bool Start(string args) {
            this.StartInfo.Arguments = args;
            return base.Start();
        }
        
        private static DumpModel dump(
            string path, DumpTypes type, Func<string, int, bool> callback) {

            int index = 0;
            var terminated = false;
            var msg = string.Empty;
            var aapt = new AAPTool();
            var output = new List<string>();

            switch (type) {
                case DumpTypes.Manifest:
                    aapt.Start($"dump badging \"{path}\"");
                    break;
                case DumpTypes.Resources:
                    aapt.Start($"dump --values resources \"{path}\"");
                    break;
                case DumpTypes.ManifestTree:
                    aapt.Start($"dump xmltree \"{path}\" AndroidManifest.xml");
                    break;
                //default:
                //    return new DumpModel(path, false, output);
            }

            while (!aapt.StandardOutput.EndOfStream && !terminated) {
                msg = aapt.StandardOutput.ReadLine();
                
                if (callback(msg, index)) {
                    terminated = true;
                    try {
                        aapt.Kill();
                    }
                    catch { }
                }
                if (!terminated)
                    index++;
                output.Add(msg);
            }

            while (!aapt.StandardError.EndOfStream) {
                output.Add(aapt.StandardError.ReadLine());
            }

            try {
                aapt.WaitForExit();
                aapt.Close();
                aapt.Dispose();
            }
            catch { }
            
            return new DumpModel(path, output.Count > 2, output);
        }

        internal static DumpModel dumpManifest(string path) {
            return dump(path, DumpTypes.Manifest, (msg, i) => false);
        }

        internal static DumpModel dumpResources(string path, Func<string, int, bool> callback) {
            return dump(path, DumpTypes.Resources, callback);
        }

        internal static DumpModel dumpManifestTree(string path, Func<string, int, bool> callback) {
            return dump(path, DumpTypes.ManifestTree, callback);
        }

        /// <summary>
        /// Start point. Begin decompile apk to extract resources
        /// </summary>
        /// <param name="path">Absolute path to .apk file</param>
        /// <returns>Filled apk if dump process is not failed</returns>
        public static ApkInfo Decompile(string path) {
            var manifest = ApkExtractor.ExtractManifest(path);
            if (!manifest.isSuccess)
                return new ApkInfo();

            var largestIcon = ApkExtractor.ExtractLargestIcon(path);

            return ApkParser.Parse(manifest).megre(
                new ApkInfo() {
                    FullPath = path,
                    Icon = largestIcon,
                }
            );
        }
    }
}
