#region Using statements
using System;
using System.IO;
using System.Collections.Generic;
#endregion

namespace LTP.Interop.IO
{
	internal class WindowsLibraryResolver : ILibraryResolver
	{
		#region Fields / Properties
		private static readonly string[] _systemPaths;
		private static readonly string[] _environmentVariables;
		#endregion

		#region Constructors
		static WindowsLibraryResolver()
		{
			_environmentVariables = new string[]
			{
				"PATH"
			};
		}
		#endregion

		#region Methods
		public string[][] Resolve( string name, LibrarySearchOptions searchOptions )
		{
			Stack<string[]> buffer = new Stack<string[]>();

			// System directory
			if( ( searchOptions & LibrarySearchOptions.IncludeSystemDirectory ) != 0 )
				buffer.Push( Directory.GetFiles( Environment.SystemDirectory, "*" + name + "*", SearchOption.TopDirectoryOnly ) );

			// Windows folder
			if( ( searchOptions & LibrarySearchOptions.IncludeSystemDirectory ) != 0 )
				buffer.Push( Directory.GetFiles( Environment.GetFolderPath( Environment.SpecialFolder.Windows ), "*" + name + "*", SearchOption.TopDirectoryOnly ) );

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
