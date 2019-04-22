#region Using statements
using System.Runtime.InteropServices;
#endregion

namespace LTP.Interop.InteropServices
{
	/// <summary>
	/// 
	/// </summary>
	public static unsafe class Memory
	{
		#region DllImport
		// msvcrt
		[DllImport( "msvcrt" )]
		private static extern void* msvcrt_memchr( void* ptr, int value, uint n );

		[DllImport( "msvcrt" )]
		private static extern int msvcrt_memcmp( void* ptr1, void* ptr2, uint n );

		[DllImport( "msvcrt" )]
		private static extern void msvcrt_memcpy( void* dest, void* src, uint n );

		[DllImport( "msvcrt" )]
		private static extern void msvcrt_memmove( void* dest, void* src, uint n );

		[DllImport( "msvcrt" )]
		private static extern void msvcrt_memset( void* s, byte c, uint n );

		// libc
		[DllImport( "libc" )]
		private static extern void* libc_memchr( void* ptr, int value, uint n );

		[DllImport( "libc" )]
		private static extern int libc_memcmp( void* ptr1, void* ptr2, uint n );

		[DllImport( "libc" )]
		private static extern void libc_memcpy( void* dest, void* src, uint n );

		[DllImport( "libc" )]
		private static extern void libc_memmove( void* dest, void* src, uint n );

		[DllImport( "libc" )]
		private static extern void libc_memset( void* ptr, byte chr, uint n );
		#endregion

		#region Delegates
		public delegate void* CharacterHandler( void* ptr, int value, uint n );
		public delegate int CompareHandler( void* ptr1, void* ptr2, uint n );
		public delegate void CopyHandler( void* dest, void* src, uint n );
		public delegate void MoveHandler( void* dest, void* src, uint n );
		public delegate void SetHandler( void* ptr, byte chr, uint n );
		#endregion

		#region Constructors
		static Memory()
		{
			OSPlatform platform = PlatformInformation.Platform;

			if( platform == OSPlatform.Windows )
			{
				Character += msvcrt_memchr;
				Compare += msvcrt_memcmp;
				Copy += msvcrt_memcpy;
				Move += msvcrt_memmove;
				Set += msvcrt_memset;
			}
			else if( platform == OSPlatform.Linux || platform == OSPlatform.OSX )
			{
				Character += libc_memchr;
				Compare += libc_memcmp;
				Copy += libc_memcpy;
				Move += libc_memmove;
				Set += libc_memset;
			}
		}
		#endregion

		#region Methods
		public static CharacterHandler Character;
		public static CompareHandler Compare;
		public static CopyHandler Copy;
		public static MoveHandler Move;
		public static SetHandler Set;
		#endregion
	}
}
