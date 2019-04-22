#region Using statements

#endregion

namespace LTP.Interop.IO
{
	/// <summary>
	/// 
	/// </summary>
	public enum LibrarySearchOptions : byte
	{
		IncludeTopDirectory = 1,
		IncludeSubDirectories = 2,
		IncludeSystemDirectory = 4,
		IncludePathDirectories = 8,
		IncludeCustomRootDirectory = 16,

		Default = 15
	}
}
