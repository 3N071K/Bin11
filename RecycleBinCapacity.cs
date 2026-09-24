using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Bin11
{
    internal static class RecycleBinCapacity
    {
        private const string BitBucketKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\BitBucket";

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetVolumeNameForVolumeMountPoint(
            string lpszVolumeMountPoint, StringBuilder lpszVolumeName, uint cchBufferLength);

        
        public static long GetTotalCapacityBytes()
        {
            long total = 0;

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady)
                    continue;

                long? configured = TryGetConfiguredCapacityBytes(drive.RootDirectory.FullName);
                total += configured ?? EstimateDefaultCapacityBytes(drive.TotalSize);
            }

            Logger.Log($"RecycleBinCapacity: суммарный лимит корзины = {total} байт.");
            return total;
        }

        private static long? TryGetConfiguredCapacityBytes(string driveRoot)
        {
            try
            {
                var sb = new StringBuilder(260);
                if (!GetVolumeNameForVolumeMountPoint(driveRoot, sb, (uint)sb.Capacity))
                    return null;

                
                string volumePath = sb.ToString();
                int start = volumePath.IndexOf('{');
                int end = volumePath.IndexOf('}');
                if (start < 0 || end < 0) return null;
                string guid = volumePath.Substring(start, end - start + 1);

                using var volumeKey = Registry.CurrentUser.OpenSubKey($@"{BitBucketKey}\Volume\{guid}");
                if (volumeKey == null) return null;

                
                if (volumeKey.GetValue("UseGlobalSetting") is int useGlobal && useGlobal != 0)
                {
                    using var globalKey = Registry.CurrentUser.OpenSubKey(BitBucketKey);
                    if (globalKey?.GetValue("MaxCapacity") is int globalMb)
                        return (long)globalMb * 1024 * 1024;
                    return null;
                }

                if (volumeKey.GetValue("MaxCapacity") is int maxMb)
                    return (long)maxMb * 1024 * 1024;
            }
            catch (Exception ex)
            {
                Logger.LogException("RecycleBinCapacity.TryGetConfiguredCapacityBytes", ex);
            }
            return null;
        }

        private static long EstimateDefaultCapacityBytes(long driveTotalSizeBytes)
        {
            const long fortyGb = 40L * 1024 * 1024 * 1024;
            const long eightGbCap = 8L * 1024 * 1024 * 1024;

            if (driveTotalSizeBytes <= fortyGb)
                return (long)(driveTotalSizeBytes * 0.10);

            long fivePercent = (long)(driveTotalSizeBytes * 0.05);
            return Math.Min(fivePercent, eightGbCap);
        }
    }
}
