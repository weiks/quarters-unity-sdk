using UnityEditor;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.PreBuild
{
   
    public interface IPreBuild
    {
        void OnPreprocessBuild(AppLinkingConfiguration configurations, string path);
    }
}