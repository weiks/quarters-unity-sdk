using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
	public interface IPosBuilder
	{
		void PostBuildProcess(AppLinkingConfiguration configuration, string pathToBuiltProject);
	}


}