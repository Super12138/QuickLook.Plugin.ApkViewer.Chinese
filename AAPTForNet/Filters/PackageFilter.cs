﻿using AAPTForNet.Models;

namespace AAPTForNet.Filters {
    internal class PackageFilter : BaseFilter {

        private string[] segments = new string[] { };

        public override bool CanHandle(string msg) {
            return msg.StartsWith("package:");
        }

        public override void AddMessage(string msg) {
            segments = msg.Split(seperator);
        }

        public override ApkInfo GetAPK() {
            return new ApkInfo() {
                PackageName = getValueOrDefault("package"),
                VersionName = getValueOrDefault("versionName"),
                VersionCode = getValueOrDefault("versionCode"),
            };
        }

        public override void Clear() => segments = new string[] { };

        private string getValueOrDefault(string key) {
            string output = string.Empty;
            for (int i = 0; i < segments.Length; i++) {
                if (segments[i].Contains(key)) {    // Find key
                    output = segments[++i];         // Get value
                    break;
                }
            }
            return string.IsNullOrEmpty(output) ? defaultEmptyValue : output;
        }
    }
}
