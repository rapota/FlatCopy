using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FlatCopy.FileSystem;

internal static class FileSystemFunctions
{
    /// <summary>
    /// Establishes a hard link between an existing file and a new file. This function is only supported on the NTFS file system, and only for files, not directories.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="existingFileName"></param>
    /// <exception cref="Win32Exception">In case of error.</exception>
    public static void CreateHardLink(string fileName, string existingFileName)
    {
        if (!NativeMethods.CreateHardLink(fileName, existingFileName, nint.Zero))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private static class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, nint lpSecurityAttributes);
    }
}