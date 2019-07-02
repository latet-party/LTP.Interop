#region Using statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using LTP.Interop.InteropServices;
using LTP.Interop.Diagnostics;
#endregion

namespace LTP.Interop.IO
{
	/// <summary>
	/// 
	/// </summary>
	public static class LibraryFinder
	{
		#region Enumerables
		private enum LibraryArchitecture : byte
		{
			i386 = 0,
			amd64 = 1
		}
		#endregion

		#region Structures
		private struct LibraryInformation
		{
			public bool Prefixed;
			public string Name;
			public short[] Version;
			public LibraryArchitecture Architecture;

			public string Path;
		}
		#endregion

		#region Constants
		private const string REGEX_TEMPLATE = @"(?i)(lib)?({0})[\.\-_]?([\d\.\-_]+?)?[\.\-_]?(x86|i386|32|32bit|x86\-64|x64|amd64|64|64bit|arm32|arm64)?\.(dll|so|dylib)[\.\-_]?([\d\.\-_]+)?";
		#endregion

		#region Fields / Properties
		private static string[][] _architectureKeywords;
		private static ILibraryResolver _osSpecificResolver;
		#endregion

		#region Constructors
		static LibraryFinder()
		{
			// _architectureKeywords
			_architectureKeywords = new string[][]
			{
				new string[] // i386
				{
					"x86",
					"i386",
					"32",
					"32bit",
					"arm32"
				},
				new string[] // amd64
				{
					"x86-64",
					"x64",
					"amd64",
					"64",
					"64bit",
					"arm64"
				}
			};

			// _osSpecificResolver
			OSPlatform platform = PlatformInformation.Platform;

			if( platform == OSPlatform.Windows )
				_osSpecificResolver = new WindowsLibraryResolver();
			else if( platform == OSPlatform.Linux )
				_osSpecificResolver = new LinuxLibraryResolver();
			else if( platform == OSPlatform.OSX )
				_osSpecificResolver = new OSXLibraryResolver();
		}
		#endregion

		#region Methods
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static LibraryArchitecture StringToArchitecture( string architectureString )
		{
			for( byte i = 0; i < _architectureKeywords.Length; i++ )
			{
				foreach( string keyword in _architectureKeywords[ i ] )
				{
					if( keyword.ToLower() == architectureString )
						return (LibraryArchitecture)i;
				}
			}

			return Environment.Is64BitProcess ? LibraryArchitecture.amd64 : LibraryArchitecture.i386;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static LibraryInformation[] FilterPaths( string name, string[] paths )
		{
			Regex regex = new Regex( string.Format( REGEX_TEMPLATE, name ) );
			Stack<LibraryInformation> buffer = new Stack<LibraryInformation>();

			foreach( string path in paths )
			{
				Match match = regex.Match( path );
				if( !match.Success )
					continue;
				if( Path.GetFileName( path ) != match.Value )
					continue;

				bool prefixed = match.Groups[ 1 ].Length > 0;

				string nameString = match.Groups[ 2 ].Value;

				string versionString =
					match.Groups[ 3 ].Length > 0 ? match.Groups[ 3 ].Value :
					match.Groups[ 6 ].Length > 0 ? match.Groups[ 6 ].Value :
					"0";

				string[] versionSplit = versionString.Split( '.', '-', '_' );
				short[] version = new short[ versionSplit.Length ];

				string architectureString = match.Groups[ 4 ].Value;
				LibraryArchitecture architecture = LibraryArchitecture.i386;

				if( architectureString == null )
				{
					if( Environment.Is64BitProcess )
						architecture = LibraryArchitecture.amd64;
				}
				else
					architecture = StringToArchitecture( architectureString );

				for( int i = 0; i < version.Length; i++ )
					short.TryParse( versionSplit[ i ], out version[ i ] );

				// Add to buffer
				buffer.Push( new LibraryInformation()
				{
					Prefixed = prefixed,
					Name = nameString,
					Version = version,
					Architecture = architecture,

					Path = path
				} );
			}

			return buffer.ToArray();
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsNewer( short[] lhs, short[] rhs )
		{
			int len = Math.Min( lhs.Length, rhs.Length );

			for( int i = 0; i < len; i++ )
			{
				if( rhs[ i ] > lhs[ i ] )
					return true;
				else if( rhs[ i ] != lhs[ i ] )
					return false;
			}

			if( rhs.Length > lhs.Length )
				return true;

			return false;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static LibraryInformation FilterByVersionAndArchitecture( params LibraryInformation[] libraries )
		{
			LibraryArchitecture targetArchictecture = Environment.Is64BitProcess ? LibraryArchitecture.amd64 : LibraryArchitecture.i386;
			LibraryInformation result = libraries[ 0 ];

			foreach( LibraryInformation library in libraries )
				if( IsNewer( result.Version, library.Version ) && library.Architecture == targetArchictecture )
					result = library;

			return result;
		}

		public static string Resolve( string name, LibrarySearchOptions searchOptions = LibrarySearchOptions.Default, string customRoot = "" )
		{
			SearchOption directorySearchOption = ( searchOptions & LibrarySearchOptions.IncludeSubDirectories ) != 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

			#region Custom root directory
			if( ( searchOptions & LibrarySearchOptions.IncludeCustomRootDirectory ) != 0 )
			{

				string[] paths = Directory.GetFiles( customRoot, "*" + name + "*", directorySearchOption );
				LibraryInformation[] libraries = FilterPaths( name, paths );

				if( libraries.Length > 0 )
				{
					foreach( LibraryInformation library in libraries )
						Debug.Log( "Found '" + Path.GetFileName( library.Path ) + "'" );

					LibraryInformation newest = FilterByVersionAndArchitecture( libraries );
					return newest.Path;
				}

			}
			#endregion

			#region Working directory
			if( ( searchOptions & LibrarySearchOptions.IncludeTopDirectory ) != 0 )
			{
				string[] paths = Directory.GetFiles( Directory.GetCurrentDirectory(), "*" + name + "*", directorySearchOption );
				LibraryInformation[] libraries = FilterPaths( name, paths );

				if( libraries.Length > 0 )
				{
					foreach( LibraryInformation library in libraries )
						Debug.Log( "Found '" + Path.GetFileName( library.Path ) + "'" );

					LibraryInformation newest = FilterByVersionAndArchitecture( libraries );
					return newest.Path;
				}
			}
			#endregion

			#region OS specific
			string[][] staggeredPaths = _osSpecificResolver.Resolve( name, searchOptions );

			foreach( string[] paths in staggeredPaths )
			{
				LibraryInformation[] libraries = FilterPaths( name, paths );

				if( libraries.Length > 0 )
				{
					foreach( LibraryInformation library in libraries )
						Debug.Log( "Found '" + Path.GetFileName( library.Path ) + "'" );

					LibraryInformation newest = FilterByVersionAndArchitecture( libraries );
					return newest.Path;
				}
			}
			#endregion

			// If all else fails
			return name;
		}
		#endregion
	}
}
