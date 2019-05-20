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
		[DllImport( "msvcrt", EntryPoint = "memchr" )]
		private static extern void* MSVCRT_MemoryCharacter( void* ptr, int value, uint n );

		[DllImport( "msvcrt", EntryPoint = "memcmp" )]
		private static extern int MSVCRT_MemoryCompare( void* ptr1, void* ptr2, uint n );

		[DllImport( "msvcrt", EntryPoint = "memcpy" )]
		private static extern void MSVCRT_MemoryCopy( void* dest, void* src, uint n );

		[DllImport( "msvcrt", EntryPoint = "memmove" )]
		private static extern void MSVCRT_MemoryMove( void* dest, void* src, uint n );

		[DllImport( "msvcrt", EntryPoint = "memset" )]
		private static extern void MSVCRT_MemorySet( void* s, byte c, uint n );

		// libc
		[DllImport( "libc", EntryPoint = "memchr" )]
		private static extern void* LibC_MemoryCharacter( void* ptr, int value, uint n );

		[DllImport( "libc", EntryPoint = "memcmp" )]
		private static extern int LibC_MemoryCompare( void* ptr1, void* ptr2, uint n );

		[DllImport( "libc", EntryPoint = "memcpy" )]
		private static extern void LibC_MemoryCopy( void* dest, void* src, uint n );

		[DllImport( "libc", EntryPoint = "memmove" )]
		private static extern void LibC_MemoryMove( void* dest, void* src, uint n );

		[DllImport( "libc", EntryPoint = "memset" )]
		private static extern void LibC_MemorySet( void* ptr, byte chr, uint n );
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
				Character += MSVCRT_MemoryCharacter;
				Compare += MSVCRT_MemoryCompare;
				Copy += MSVCRT_MemoryCopy;
				Move += MSVCRT_MemoryMove;
				Set += MSVCRT_MemorySet;
			}
			else if( platform == OSPlatform.Linux || platform == OSPlatform.OSX )
			{
				Character += LibC_MemoryCharacter;
				Compare += LibC_MemoryCompare;
				Copy += LibC_MemoryCopy;
				Move += LibC_MemoryMove;
				Set += LibC_MemorySet;
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
