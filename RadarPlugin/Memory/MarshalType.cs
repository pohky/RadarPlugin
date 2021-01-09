using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable SwitchStatementMissingSomeCases

namespace RadarPlugin.Memory {
    internal static class MarshalType<T> {
        public static bool IsIntPtr { get; }
        public static Type RealType { get; }
        public static int Size { get; }
        public static TypeCode TypeCode { get; }
        public static bool CanBeStoredInRegisters { get; }

        static MarshalType() {
            IsIntPtr = typeof(T) == typeof(IntPtr);
            RealType = typeof(T);
            if (RealType.IsEnum)
                RealType = RealType.GetEnumUnderlyingType();
            TypeCode = Type.GetTypeCode(RealType);
            Size = TypeCode == TypeCode.Boolean ? 1 : Marshal.SizeOf(RealType);

            CanBeStoredInRegisters =
                IsIntPtr ||
                TypeCode == TypeCode.Boolean ||
                TypeCode == TypeCode.Byte ||
                TypeCode == TypeCode.Char ||
                TypeCode == TypeCode.Int16 ||
                TypeCode == TypeCode.Int32 ||
                TypeCode == TypeCode.Int64 ||
                TypeCode == TypeCode.Single ||
                TypeCode == TypeCode.UInt16 ||
                TypeCode == TypeCode.UInt32 ||
                TypeCode == TypeCode.UInt64 ||
                TypeCode == TypeCode.SByte;
        }

        [DebuggerStepThrough]
        public static byte[] ObjectToByteArray(T obj) {
            switch (TypeCode) {
                case TypeCode.Object:
                    if (IsIntPtr)
                        return Size == 4 ? BitConverter.GetBytes(((IntPtr)(object)obj).ToInt32()) : BitConverter.GetBytes(((IntPtr)(object)obj).ToInt64());

                    break;
                case TypeCode.Byte:
                    return new[] {(byte)(object)obj};
                case TypeCode.Boolean:
                    return BitConverter.GetBytes((bool)(object)obj);
                case TypeCode.Char:
                    return Encoding.UTF8.GetBytes(new[] {(char)(object)obj});
                case TypeCode.Double:
                    return BitConverter.GetBytes((double)(object)obj);
                case TypeCode.Int16:
                    return BitConverter.GetBytes((short)(object)obj);
                case TypeCode.Int32:
                    return BitConverter.GetBytes((int)(object)obj);
                case TypeCode.Int64:
                    return BitConverter.GetBytes((long)(object)obj);
                case TypeCode.Single:
                    return BitConverter.GetBytes((float)(object)obj);
                case TypeCode.UInt16:
                    return BitConverter.GetBytes((ushort)(object)obj);
                case TypeCode.UInt32:
                    return BitConverter.GetBytes((uint)(object)obj);
                case TypeCode.UInt64:
                    return BitConverter.GetBytes((ulong)(object)obj);
                case TypeCode.String:
                    throw new ArgumentException("This method doesn't support String");
            }

            using (var local = new LocalMemory(Size)) {
                local.Write(obj);
                return local.Read();
            }
        }

        [DebuggerStepThrough]
        public static T ByteArrayToObject(byte[] byteArray, int index = 0) {
            switch (TypeCode) {
                case TypeCode.Object:
                    if (IsIntPtr) {
                        switch (byteArray.Length) {
                            case 1:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(new byte[] {byteArray[index], 0x0, 0x0, 0x0}, index));
                            case 2:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(new byte[] {byteArray[index], byteArray[index + 1], 0x0, 0x0}, index));
                            case 4:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(byteArray, index));
                            case 8:
                                return (T)(object)new IntPtr(BitConverter.ToInt64(byteArray, index));
                        }
                    }

                    break;
                case TypeCode.Boolean:
                    return (T)(object)BitConverter.ToBoolean(byteArray, index);
                case TypeCode.Byte:
                    return (T)(object)byteArray[index];
                case TypeCode.Char:
                    return (T)(object)Encoding.UTF8.GetChars(byteArray)[index];
                case TypeCode.Double:
                    return (T)(object)BitConverter.ToDouble(byteArray, index);
                case TypeCode.Int16:
                    return (T)(object)BitConverter.ToInt16(byteArray, index);
                case TypeCode.Int32:
                    return (T)(object)BitConverter.ToInt32(byteArray, index);
                case TypeCode.Int64:
                    return (T)(object)BitConverter.ToInt64(byteArray, index);
                case TypeCode.Single:
                    return (T)(object)BitConverter.ToSingle(byteArray, index);
                case TypeCode.UInt16:
                    return (T)(object)BitConverter.ToUInt16(byteArray, index);
                case TypeCode.UInt32:
                    return (T)(object)BitConverter.ToUInt32(byteArray, index);
                case TypeCode.UInt64:
                    return (T)(object)BitConverter.ToUInt64(byteArray, index);
                case TypeCode.String:
                    throw new ArgumentException("This method doesn't support String");
            }

            using (var local = new LocalMemory(Size)) {
                local.Write(byteArray, index);
                return local.Read<T>();
            }
        }
    }

    internal sealed class LocalMemory : IDisposable {
        public IntPtr Address { get; private set; }
        public int Size { get; }

        public LocalMemory(int size) {
            Size = size;
            Address = Marshal.AllocHGlobal(Size);
        }

        #region Read

        public T Read<T>() {
            return (T)Marshal.PtrToStructure(Address, typeof(T));
        }

        public byte[] Read() {
            var bytes = new byte[Size];
            Marshal.Copy(Address, bytes, 0, Size);
            return bytes;
        }

        #endregion

        #region Write

        public void Write(byte[] data, int index = 0) {
            Marshal.Copy(data, index, Address, Size);
        }

        public void Write<T>(T data) {
            Marshal.StructureToPtr(data, Address, false);
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            Marshal.FreeHGlobal(Address);
            Address = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        ~LocalMemory() {
            Dispose();
        }

        #endregion
    }
}