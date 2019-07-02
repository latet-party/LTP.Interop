#region Using statements
using System;
using System.IO;
using System.Security;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using LTP.Interop.IO;
#endregion

namespace LTP.Interop.InteropServices
{
	/// <summary>
	/// 
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	public static class LibraryLoader
	{
		#region DllImport
		[DllImport( "kernel32", EntryPoint = "LoadLibrary" )]
		private static extern IntPtr LoadLibrary( [In] [MarshalAs( UnmanagedType.LPStr )] string lpFileName );

		[DllImport( "kernel32", EntryPoint = "GetProcAddress" )]
		private static extern IntPtr GetProcAddress( IntPtr hModule, [In] [MarshalAs( UnmanagedType.LPStr )] string lpProcName );

		[DllImport( "libdl", EntryPoint = "dlopen" )]
		private static extern IntPtr DlOpen( [In] [MarshalAs( UnmanagedType.LPStr )] string filename, int flags );

		[DllImport( "libdl", EntryPoint = "dlsym" )]
		private static extern IntPtr DlSym( IntPtr handle, [In] [MarshalAs( UnmanagedType.LPStr )] string symbol );

		#region dlopen flags
		private const int RTLD_NOW = 2;
		private const int RTLD_GLOBAL = 256;
		#endregion
		#endregion

		#region Fields / Properties
		private static Type _typeofDelegate = typeof( MulticastDelegate );
		#endregion

		#region Delegates
		public delegate IntPtr OpenHandler( string filename, int flags );
		public delegate IntPtr SymbolHandler( IntPtr handle, string symbol );
		#endregion

		#region Constructors
		static LibraryLoader()
		{
			if( PlatformInformation.Platform == OSPlatform.Windows )
			{
				Open += ( string filename, int flags ) => LoadLibrary( filename );
				Symbol += GetProcAddress;
			}
			else if( PlatformInformation.Platform == OSPlatform.Linux || PlatformInformation.Platform == OSPlatform.OSX )
			{
				Open += DlOpen;
				Symbol += DlSym;
			}
		}
		#endregion

		#region Methods
		public static OpenHandler Open { get; }
		public static SymbolHandler Symbol { get; }

		public static IntPtr Load( Type ofType, bool rtldGlobal = true )
		{
			object[] attributes = ofType.GetCustomAttributes( typeof( LibraryAttribute ), false );
			if( attributes.Length == 0 )
				throw new ArgumentNullException( "Type '" + ofType.Name + "' is missing LibraryAttribute." );

			#region Find library
			LibraryAttribute libAttribute = (LibraryAttribute)attributes[ 0 ];
			string filename = LibraryFinder.Resolve( libAttribute.Name, libAttribute.SearchOptions, libAttribute.CustomRoot );
			#endregion

			#region Open library
			IntPtr libraryPtr = IntPtr.Zero;

			if( !File.Exists( filename ) )
				throw new DllNotFoundException( filename );

			libraryPtr = Open( filename, rtldGlobal ? RTLD_NOW : RTLD_GLOBAL | RTLD_NOW );

#if DEBUG
			ConsoleColor oldFG = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine( filename );
			Console.WriteLine( "\t|" );
			Console.WriteLine( "\t-> (0x{0})", libraryPtr.ToString( "X16" ) );
			Console.WriteLine();
			Console.ForegroundColor = oldFG;
#endif
			#endregion

			CilHelper.LinkAllExternalMethods( ofType, libraryPtr );

			return libraryPtr;
		}
		#endregion
	}
}
