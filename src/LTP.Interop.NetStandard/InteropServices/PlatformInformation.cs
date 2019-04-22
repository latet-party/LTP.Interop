#region Using statements
using System.Runtime.InteropServices;
#endregion

namespace LTP.Interop.InteropServices
{
	/// <summary>
	/// 
	/// </summary>
	public static class PlatformInformation
	{
		#region Fields / Properties
		public static OSPlatform Platform;
		#endregion

		#region Constructors
		static PlatformInformation()
		{
			if( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
				Platform = OSPlatform.Windows;
			else if( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
				Platform = OSPlatform.Linux;
			else if( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
				Platform = OSPlatform.OSX;
			else
				Platform = OSPlatform.Create( "Unknown" );
		}
		#endregion
	}
}
