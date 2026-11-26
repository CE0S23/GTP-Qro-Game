using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GTP.EditorTools
{
    /// <summary>
    /// Compilaciones de Windows 64 bits. Menú GTP > Compilar … o por línea de comandos:
    /// Unity.exe -batchmode -projectPath "Game/GTP Qro" -executeMethod GTP.EditorTools.BuildTools.BuildReleaseBatch -quit
    /// - Release: Builds/Windows/  (la que se publica en itch.io).
    /// - Desarrollo: Builds/Windows_Dev/ (permite conectar el Profiler y correr la sonda -gtpPerf).
    /// </summary>
    public static class BuildTools
    {
        public const string ExeName = "GTP QroCITY stories.exe";

        [MenuItem("GTP/Compilar Windows (release)")]
        public static void BuildReleaseMenu() => Build(false);

        [MenuItem("GTP/Compilar Windows (desarrollo + Profiler)")]
        public static void BuildDevMenu() => Build(true);

        public static void BuildReleaseBatch() => BatchBuild(false);
        public static void BuildDevBatch() => BatchBuild(true);

        private static void BatchBuild(bool development)
        {
            try
            {
                EditorApplication.Exit(Build(development) ? 0 : 1);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        private static bool Build(bool development)
        {
            string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds",
                development ? "Windows_Dev" : "Windows");
            Directory.CreateDirectory(folder);

            PlayerSettings.productName = "GTP QroCITY stories";
            PlayerSettings.companyName = "UTEQ - Equipo GTP";

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(folder, ExeName),
                target = BuildTarget.StandaloneWindows64,
                options = development ? BuildOptions.Development : BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log("[GTP] Compilación " + (development ? "desarrollo" : "release") + ": " + summary.result +
                      " · " + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB · " + summary.totalErrors + " errores · " +
                      options.locationPathName);
            return summary.result == BuildResult.Succeeded;
        }
    }
}
