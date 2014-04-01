using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace CmisSync.Lib.Storage
{
    class ExtendedAttributeReaderDos : IExtendedAttributeReader
    {
#if ! __MonoCS__
        [DllImport( "kernel32.dll", SetLastError=true )]
        private static extern IntPtr CreateFile( string fileName, FILE_ACCESS_RIGHTS access, FileShare share, int securityAttributes,
                                                FileMode creation, FILE_FLAGS flags, IntPtr templateFile );

        [DllImport( "kernel32.dll", SetLastError=true )]
        private static extern bool CloseHandle( IntPtr handle );

        [DllImport( "kernel32.dll", SetLastError=true )]
        private static extern bool DeleteFile( string fileName );

        private enum FILE_ACCESS_RIGHTS : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        private enum FILE_FLAGS : uint
        {
            WriteThrough = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x8000000,
            DeleteOnClose = 0x4000000,
            BackupSemantics = 0x2000000,
            PosixSemantics = 0x1000000,
            OpenReparsePoint = 0x200000,
            OpenNoRecall = 0x100000
        }
#endif

        public string GetExtendedAttribute (string path, string key)
        {
#if ! __MonoCS__
            if(String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }
            IntPtr fileHandle = CreateFile( String.Format("{0}:{1}", path, key), FILE_ACCESS_RIGHTS.GENERIC_READ, FileShare.Read, 0, FileMode.Open, 0, IntPtr.Zero );
            TextReader reader = new StreamReader( new FileStream( new SafeFileHandle( fileHandle, true ), FileAccess.Read ));

            string result = reader.ReadToEnd();
            reader.Close();
            CloseHandle( fileHandle );
            // int error = Marshal.GetLastWin32Error();
            return result;
#else
            throw new NotImplementedException();
#endif
        }

        public void SetExtendedAttribute (string path, string key, string value)
        {
#if ! __MonoCS__
            if(String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }
            IntPtr fileHandle = CreateFile( String.Format ("{0}:{1}", path, key), FILE_ACCESS_RIGHTS.GENERIC_WRITE, FileShare.Write, 0, FileMode.Create, 0, IntPtr.Zero );
            TextWriter writer = new StreamWriter( new FileStream( new SafeFileHandle( fileHandle, true ), FileAccess.Write ));
            writer.Write(value);
            writer.Close();
            CloseHandle( fileHandle );
#else
            throw new NotImplementedException();
#endif
        }

        public void RemoveExtendedAttribute (string path, string key)
        {
#if ! __MonoCS__
            if(String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }
            DeleteFile(String.Format("{0}:{1}", path, key));
#else
            throw new NotImplementedException();
#endif
        }

        public List<string> ListAttributeKeys (string path)
        {
            throw new NotImplementedException ();
        }

    }

}
