#region Using statements
using System;
using System.IO;
using System.Collections.Generic;
#endregion

namespace LTP.Interop.IO
{
	internal class LinuxLibraryResolver : ILibraryResolver
	{
		#region Constants
		private const string LIB_PATH = "/lib";
		private const string USR_LIB_PATH = "/usr/lib";
		#endregion

		#region Fields / Properties
		private static readonly string[] _environmentVariables;
		#endregion

		#region Constructors
		static LinuxLibraryResolver()
		{
			_environmentVariables = new string[]
			{
				"LD_LIBRARY_PATH",
				"PATH"
			};
		}
		#endregion

		#region Methods
		public string[][] Resolve( string name, LibrarySearchOptions searchOptions )
		{
			Stack<string[]> buffer = new Stack<string[]>();

			// /lib
			if( ( searchOptions & LibrarySearchOptions.IncludeSystemDirectory ) != 0 )
				buffer.Push( Directory.GetFiles( LIB_PATH, "*" + name + "*", SearchOption.TopDirectoryOnly ) );

			// /usr/lib
			if( ( searchOptions & LibrarySearchOptions.IncludeSystemDirectory ) != 0 )
				buffer.Push( Directory.GetFiles( USR_LIB_PATH, "*" + name + "*", SearchOption.TopDirectoryOnly ) );

			// Environment variables
			if( ( searchOptions & LibrarySearchOptions.IncludePathDirectories ) != 0 )
			{
				foreach( string environmentVariable in _environmentVariables )
				{
					string[] envPaths = Environment.GetEnvironmentVariable( environmentVariable ).Split( Path.PathSeparator );

					foreach( string envPath in envPaths )
					{
						try
						{
							if( !Directory.Exists( envPath ) )
								continue;

							buffer.Push( Directory.GetFiles( envPath, "*" + name + "*", SearchOption.TopDirectoryOnly ) );
						}
						catch( UnauthorizedAccessException )
						{ }
					}
				}
			}

			return buffer.ToArray();
		}
		#endregion
	}
}
