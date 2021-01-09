using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RadarPlugin.Memory {
    public class ProcessMemory {
        public readonly IntPtr Handle;

        public ProcessMemory() {
            Handle = Imports.GetCurrentProcess();
        }

        private bool ReadBytes(IntPtr address, int count, out byte[] buffer) {
            buffer = new byte[count <= 0 ? 0 : count];
            if (IsInvalid(address) || count <= 0)
                return false;
            return Imports.ReadProcessMemory(Handle, address, buffer, buffer.Length, out _);
        }

        private bool WriteBytes(IntPtr address, byte[] buffer) {
            if (IsInvalid(address))
                return false;
            return Imports.WriteProcessMemory(Handle, address, buffer, buffer.Length, out _);
        }

        public T Read<T>(IntPtr address) where T : struct {
            if (IsInvalid(address) || !ReadBytes(address, MarshalType<T>.Size, out var buffer))
                return default;
            return MarshalType<T>.ByteArrayToObject(buffer);
        }

        public T[] Read<T>(IntPtr address, int count) where T : struct {
            var size = MarshalType<T>.Size;
            var result = new T[count];
            for (var i = 0; i < count; i++)
                result[i] = Read<T>(address + i * size);
            return result;
        }

        public bool Write<T>(IntPtr address, T obj) where T : struct {
            return WriteBytes(address, MarshalType<T>.ObjectToByteArray(obj));
        }

        public bool Write<T>(IntPtr address, T[] objArray) where T : struct {
            if (objArray == null || objArray.Length == 0)
                return false;
            var size = MarshalType<T>.Size;
            for (var i = 0; i < objArray.Length; i++)
                if (!Write(address + i * size, objArray[i]))
                    return false;
            return true;
        }

        public string ReadString(IntPtr address, int maxLength = 256) {
            if (!ReadBytes(address, maxLength, out var buffer))
                return string.Empty;
            var data = Encoding.UTF8.GetString(buffer);
            var eosPos = data.IndexOf('\0');
            return eosPos == -1 ? data : data.Substring(0, eosPos);
        }

        public bool WriteString(IntPtr address, string str) {
            if (string.IsNullOrEmpty(str))
                return true;
            return WriteBytes(address, Encoding.UTF8.GetBytes(str + "\0"));
        }

        private bool IsInvalid(IntPtr address) {
            return address == IntPtr.Zero || address.ToInt64() < 10_000;
        }

        private static class Imports {
            [DllImport("kernel32", SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

            [DllImport("kernel32", SetLastError = false)]
            public static extern IntPtr GetCurrentProcess();
        }
    }
}