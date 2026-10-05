using UnityEngine;
using UnityEngine.Rendering;

namespace NecrocisEditor
{
    public static partial class GasSacSetup
    {
        public static string DetectPipeline()
        {
            var asset = GraphicsSettings.currentRenderPipeline;
            if (asset == null) return "BuiltIn";
            string name = asset.GetType().FullName ?? string.Empty;
            if (name.Contains("Universal")) return "URP";
            if (name.Contains("HighDefinition")) return "HDRP";
            return "Custom";
        }

        public static void Diagnose()
        {
            Debug.Log("[GasSac-I01A] Render pipeline: " + DetectPipeline() + ". New sprite scope only; existing XZ billboard camera is retained.");
        }
    }
}
