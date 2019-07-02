#region Using statements
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#endregion

namespace LTP.Interop.InteropServices
{
	/// <summary>
	/// 
	/// </summary>
	internal static class CilHelper
	{
		#region Fields / Properties
		private static Type _typeOfDelegate;
		#endregion

		#region Constructors
		static CilHelper()
		{
			_typeOfDelegate = typeof( MulticastDelegate );
		}
		#endregion

		#region Methods	
#if NETCOREAPP2_1 || NETCOREAPP2_0
		public static object GetDelegateForFunctionPointer( IntPtr ptr, Type ofType )
		{
			MethodInfo method = ofType.GetMethod( "Invoke" );

			ParameterInfo[] paramInfos = method.GetParameters();
			Type[] paramTypes = new Type[ paramInfos.Length ];

			for( int i = 0; i < paramTypes.Length; i++ )
				paramTypes[ i ] = paramInfos[ i ].ParameterType;

			DynamicMethod dyn = new DynamicMethod( method.Name, method.ReturnType, paramTypes );
			ILGenerator gen = dyn.GetILGenerator();

			for( int i = 0; i < paramTypes.Length; i++ )
				gen.Emit( OpCodes.Ldarg, i );

			gen.Emit( OpCodes.Ldc_I8, (long)ptr );
			gen.Emit( OpCodes.Conv_I );
			gen.EmitCalli( OpCodes.Calli, CallingConvention.Cdecl, method.ReturnType, paramTypes );
			gen.Emit( OpCodes.Ret );

			return dyn.CreateDelegate( ofType );
		}
#else
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static object GetDelegateForFunctionPointer( IntPtr ptr, Type ofType ) => Marshal.GetDelegateForFunctionPointer( ptr, ofType );
#endif
		public static void LinkAllExternalMethods( Type ofType, IntPtr libraryPtr )
		{
			FieldInfo[] fields = ofType.GetFields( BindingFlags.Public | BindingFlags.Static );

			foreach( FieldInfo fi in fields )
			{
				if( fi.FieldType.BaseType == _typeOfDelegate )
				{
					object[] attributes = fi.GetCustomAttributes( typeof( ExternalMethodAttribute ), false );
					if( attributes.Length == 0 )
						continue;

					ExternalMethodAttribute extmAttribute = (ExternalMethodAttribute)attributes[ 0 ];

					IntPtr ptr = LibraryLoader.Symbol( libraryPtr, extmAttribute.EntryPoint ?? fi.Name );

					if( ptr != IntPtr.Zero )
						fi.SetValue( null, GetDelegateForFunctionPointer( ptr, fi.FieldType ) );
					else
						Console.Error.WriteLine( $"Unable to find entrypoint '{fi.Name}' for '{ofType.Name}'." );
				}
			}
		}
		#endregion
	}
}
