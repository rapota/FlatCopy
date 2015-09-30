using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NLog;

namespace FileHeap
{
    public static class FileManagementFunctions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Establishes a hard link between an existing file and a new file. This function is only supported on the NTFS file system, and only for files, not directories.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="existingFileName"></param>
        /// <exception cref="Win32Exception">In case of error.</exception>
        public static void CreateHardLink(string fileName, string existingFileName)
        {
            if (!NativeMethods.CreateHardLink(fileName, existingFileName, IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            logger.Trace("Created hard link '{0}' for file at '{1}'.", fileName, existingFileName);
        }

        public static Task CreateHardLinkAsync(string fileName, string existingFileName)
        {
            object state = new Tuple<string, string>(fileName, existingFileName);
            return Task.Factory.StartNew(CreatingHardLink, state);
        }

        private static void CreatingHardLink(object state)
        {
            Tuple<string, string> files = (Tuple<string, string>)state;
            CreateHardLink(files.Item1, files.Item2);
        }

        private static class NativeMethods
        {
            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
        }
    }
}