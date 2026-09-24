using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bin11
{
    
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SHQUERYRBINFO
        {
            public int cbSize;
            public long i64Size;      // суммарный занятый объём в байтах
            public long i64NumItems;  // количество объектов в корзине
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHQueryRecycleBin(string? pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

        [Flags]
        public enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, RecycleFlags dwFlags);

        
        public static (long sizeBytes, long itemCount) GetTotalRecycleBinUsage()
        {
            long totalSize = 0;
            long totalItems = 0;

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady)
                    continue;

                var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf<SHQUERYRBINFO>() };
                int hr = SHQueryRecycleBin(drive.Name, ref info);
                if (hr == 0) // S_OK
                {
                    totalSize += info.i64Size;
                    totalItems += info.i64NumItems;
                }
            }

            return (totalSize, totalItems);
        }

        
        public static void EmptyAll(bool askConfirmation)
        {
            var flags = RecycleFlags.SHERB_NOSOUND;
            if (!askConfirmation)
                flags |= RecycleFlags.SHERB_NOCONFIRMATION;

            // pszRootPath = null -> очищает корзины на всех дисках сразу.
            SHEmptyRecycleBin(IntPtr.Zero, null, flags);
        }
    }
}
