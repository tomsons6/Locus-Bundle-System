using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BundleSystem
{
    public class AssetbundleBuildProcessors : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => -999;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = AssetbundleBuildSettings.EditorInstance;
            //no instance found
            if (settings == null) return;
            if (Directory.Exists(AssetbundleBuildSettings.LocalBundleRuntimePath)) Directory.Delete(AssetbundleBuildSettings.LocalBundleRuntimePath, true);
            if (!Directory.Exists(Application.streamingAssetsPath)) Directory.CreateDirectory(Application.streamingAssetsPath);


            var localBundleSourcePath = Utility.CombinePath(settings.LocalOutputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
            if(!Directory.Exists(localBundleSourcePath))
            {
                if(Application.isBatchMode)
                {
                    Debug.LogWarning("Missing built local bundle directory, Locus bundle system won't work properly.");
                    return; //we can't build now as it's in batchmode
                }
                else
                {
                    var buildNow = EditorUtility.DisplayDialog("LocusBundleSystem", "Warning - Missing built local bundle directory, would you like to build now?", "Yes", "Not now");
                    if(!buildNow) return; //user declined
                    AssetbundleBuilder.BuildAssetBundles(BuildType.Local);
                }
            }

            //FileUtil.CopyFileOrDirectory(Utility.CombinePath(settings.LocalOutputPath, EditorUserBuildSettings.activeBuildTarget.ToString()), AssetbundleBuildSettings.LocalBundleRuntimePath);
            //AssetDatabase.Refresh();
            string StartDirectory = Utility.CombinePath(settings.LocalOutputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
            Debug.Log(Directory.GetFiles(StartDirectory));
            foreach (string fileName in Directory.GetFiles(StartDirectory))
            {
                Debug.Log(fileName);
            }
            if (!Directory.Exists(AssetbundleBuildSettings.LocalBundleRuntimePath))
            {
                Directory.CreateDirectory(AssetbundleBuildSettings.LocalBundleRuntimePath);
            }

#if BUILD_OCULUS
           ChangeBundle("OculusBundle", StartDirectory, AssetbundleBuildSettings.LocalBundleRuntimePath);
#endif

            CopyNecesseryFiles(StartDirectory, AssetbundleBuildSettings.LocalBundleRuntimePath);
            //FileUtil.CopyFileOrDirectory(Utility.CombinePath(settings.LocalOutputPath, EditorUserBuildSettings.activeBuildTarget.ToString()), AssetbundleBuildSettings.LocalBundleRuntimePath);
            AssetDatabase.Refresh();
        }
        void CopyNecesseryFiles(string source, string dest)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
            File.Copy(source + "/buildlogtep.json", dest + "/buildlogtep.json", true);
            File.Copy(source + "/BundleBuildLog.txt", dest + "/BundleBuildLog.txt", true);
            File.Copy(source + "/Manifest.json", dest + "/Manifest.json", true);

        }
        void ChangeBundle(string BundleName, string source, string dest)
        {
            var settingInstance = AssetbundleBuildSettings.EditorInstance;
            foreach (BundleSetting Bundle in settingInstance.BundleSettings)
            {
                if (Bundle.BundleName == BundleName)
                {
                    Bundle.IncludedInPlayer = true;
                }
                else
                {
                    Bundle.IncludedInPlayer = false;
                }
            }

            if (!Application.isBatchMode)
            {
                AssetbundleBuilder.BuildAssetBundles(settingInstance,BuildType.Local);
                //EditorUtility.DisplayDialog("Building bundle", "Yes", "Not now");
            }
            File.Copy(source + "/" + BundleName,dest + "/" + BundleName, true);
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            if(FileUtil.DeleteFileOrDirectory(AssetbundleBuildSettings.LocalBundleRuntimePath))
            {
                AssetDatabase.Refresh();
            }
        }
    }
}
