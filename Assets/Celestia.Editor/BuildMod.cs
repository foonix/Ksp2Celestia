using System.IO;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace Celestia.Editor
{
    public class BuildMod
    {
        [MenuItem("Tools/Celestia/Build (debug) and run")]
        public static void BuildDebugAndRun()
        {
            if (Build(true))
            {
                var tkSettings = ThunderKitSettings.GetOrCreateSettings<ThunderKitSettings>();
                System.Diagnostics.Process.Start(Path.Combine(tkSettings.GamePath, tkSettings.GameExecutable));
            }
        }

        [MenuItem("Tools/Celestia/Build (debug)")]
        public static void BuildDebug()
        {
            Build(true);
        }

        [MenuItem("Tools/Celestia/Build (release)")]
        public static void BuildRelease()
        {
            Build(false);
        }

        private static bool Build(bool debug)
        {
            var tkSettings = ThunderKitSettings.GetOrCreateSettings<ThunderKitSettings>();

            // config
            string modName = "Celestia";
            var pluginDir = Path.Combine(tkSettings.GamePath, "BepInEx", "plugins", modName);
            var bepInExPrepatchDir = Path.Combine(tkSettings.GamePath, "BepInEx", "patchers");
            var buildFolder = "build";
            var pluginSrcManagedAssemblies = new string[]
            {
                "Celestia",
            };

            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = new[] { "Assets/Scenes/test.unity" },
                locationPathName = $"{buildFolder}/{modName}.exe",
                target = BuildTarget.StandaloneWindows64,

                // use these options for the first build
                //buildPlayerOptions.options = BuildOptions.Development;

                // use these options for building scripts
                options = BuildOptions.BuildScriptsOnly | (debug ? BuildOptions.Development : 0)
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                // Copy Managed library
                foreach (var assmeblyName in pluginSrcManagedAssemblies)
                {
                    var managedSrc = Path.Combine(buildFolder, $"{modName}_Data/Managed/{assmeblyName}.dll");
                    var managedDest = Path.Combine(pluginDir, $"{assmeblyName}.dll");
                    CopyOverwrite(managedSrc, managedDest);

                    var managedPdbDest = managedDest.Replace(".dll", ".pdb");
                    if (debug)
                    {
                        var managedPdbSrc = managedSrc.Replace(".dll", ".pdb");
                        CopyOverwrite(managedPdbSrc, managedPdbDest);
                    }
                    else
                    {
                        File.Delete(managedPdbDest);
                    }
                }

                // Copy Burst library
                //var burstedSrc = Path.Combine(buildFolder, $"{modName}_Data/Plugins/x86_64/lib_burst_generated.dll");
                // If the file extension is .dll, SpaceWarp and BepInEx will log exceptions.
                //var burstedDest = Path.Combine(tmPluginDir, TurboModePlugin.burstCodeAssemblyName);

                //if (File.Exists(burstedSrc))
                //{
                //    CopyOverwrite(burstedSrc, burstedDest);

                //    var burstPdbDest = burstedDest.Replace(".dll", ".pdb");
                //    if (debug)
                //    {
                //        CopyOverwrite(burstedSrc.Replace(".dll", ".pdb"), burstPdbDest);
                //    }
                //    else
                //    {
                //        File.Delete(burstPdbDest);
                //    }
                //}

                // Copy BepInEx prepatch, which goes in a different directory.
                //var prepatchSrc = Path.Combine(buildFolder, $"{modName}_Data/Managed/{modName}.Prepatch.dll");
                //var prepatchDest = Path.Combine(bepInExPrepatchDir, $"{modName}.Prepatch.dll");
                //CopyOverwrite(prepatchSrc, prepatchDest);

                var addressablesDir = Path.Combine(pluginDir, "addressables");

                Debug.Log($"{modName} build complete");
                return true;
            }
            else
            {
                Debug.LogError($"{modName} build failed");
                return false;
            }
        }

        private static void CopyOverwrite(string src, string dest)
        {
            FileUtil.DeleteFileOrDirectory(dest);
            if (!File.Exists(dest))
                FileUtil.CopyFileOrDirectory(src, dest);
            else
                Debug.LogWarning($"Couldn't update manged dll, {dest} is it currently in use?");
        }
    }
}