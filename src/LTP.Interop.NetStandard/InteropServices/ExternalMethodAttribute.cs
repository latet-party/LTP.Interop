#region Using statements
using System;
#endregion

namespace LTP.Interop.InteropServices
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = false )]
	public class ExternalMethodAttribute : Attribute
	{
		#region Constants

		#endregion

		#region Fields / Properties
		public string EntryPoint;
		#endregion

		#region Constructors
		public ExternalMethodAttribute() { }
		#endregion

		#region Methods

		#endregion
	}
}
