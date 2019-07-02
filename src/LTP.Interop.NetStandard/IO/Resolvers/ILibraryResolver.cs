namespace LTP.Interop.IO
{
	internal interface ILibraryResolver
	{
		#region Methods
		string[][] Resolve( string name, LibrarySearchOptions searchOptions );
		#endregion
	}
}
