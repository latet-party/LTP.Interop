#region Using statements
using System;

using LTP.Interop.IO;
#endregion

namespace LTP.Interop.InteropServices
{
	/// <summary>
	/// 
	/// </summary>
	public class LibraryAttribute : Attribute
	{
		#region Constants

		#endregion

		#region Fields / Properties
		public LibrarySearchOptions SearchOptions = LibrarySearchOptions.Default;
		public string CustomRoot = string.Empty;

		public string Name { get; }
		#endregion

		#region Constructors
		public LibraryAttribute( string name )
		{
			this.Name = name;
		}
		#endregion

		#region Methods

		#endregion
	}
}
