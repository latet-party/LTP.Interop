#region Using statements
using System;
using System.IO;
using System.Collections.Generic;
#endregion

namespace LTP.Interop.IO
{
	internal class OSXLibraryResolver : ILibraryResolver
	{
		#region Fields / Properties
		private static readonly string[] _environmentVariables;
		#endregion

		#region Constructors
		static OSXLibraryResolver()
		{
			_environmentVariables = new string[]
			{
				"DYLD_FRAMEWORK_PATH",
				"DYLD_LIBRARY_PATH",
				"DYLD_FALLBACK_FRAMEWORK_PATH",
				"DYLD_FALLBACK_LIBRARY_PATH"
			};
		}
		#endregion

		#region Methods
		public string[][] Resolve( string name, LibrarySearchOptions searchOptions )
		{
			Stack<string[]> buffer = new Stack<string[]>();

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
