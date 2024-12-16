using System.Collections.ObjectModel;
using System.IO;

namespace 全面瑕疵检测.Models
{
    /// <summary>
    /// 系统磁盘模型
    /// </summary>
    internal class DiskModel
    {
        /// <summary>
        /// 磁盘名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 总容量
        /// </summary>
        public double TotalSpaceGB { get; set; }
        /// <summary>
        /// 已使用
        /// </summary>
        public double UsedSpaceGB { get; set; }
        /// <summary>
        /// 可使用
        /// </summary>
        public double FreeSpaceGB { get; set; }
        /// <summary>
        /// 可用占比
        /// </summary>
        public double UsedSpacePercentage => (UsedSpaceGB / TotalSpaceGB) * 100; // 新增属性


        /// <summary>
        /// 获取包含操作系统的磁盘信息
        /// </summary>
        /// <returns></returns>
        public static DriveInfo? GetSystemDriveName()
        {
            // 获取系统盘路径
            var systemDrivePath = Path.GetPathRoot(Environment.SystemDirectory);

            // 获取所有驱动器信息
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // 查找系统盘的 DriveInfo 对象
            DriveInfo? systemDrive = allDrives.FirstOrDefault(drive => drive.Name.Equals(systemDrivePath, StringComparison.OrdinalIgnoreCase));

            return systemDrive;

        }

        /// <summary>
        /// 获取除去操作系统盘外的可用容量最大的磁盘信息
        /// </summary>
        /// <returns></returns>
        public static DriveInfo? GetLargestAvailableDriveExcludingSystemDrive()
        {
            // 获取系统盘路径
            var systemDrivePath = Path.GetPathRoot(Environment.SystemDirectory);

            // 获取所有驱动器信息，排除系统盘
            DriveInfo? largestDrive = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && !drive.Name.Equals(systemDrivePath, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(drive => drive.TotalFreeSpace)
                .FirstOrDefault();

            return largestDrive;
        }

        /// <summary>
        /// 判断磁盘空间是否够用
        /// </summary>
        /// <param name="driveName">磁盘名称</param>
        /// <param name="requiredSpace">磁盘容量单位G</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool IsDiskSpaceSufficient(string driveName, long requiredSpace)
        {
            DriveInfo drive = new DriveInfo(driveName);

            if (!drive.IsReady)
            {
                throw new ArgumentException($"Drive {driveName} is not ready or does not exist.");
            }

            return drive.AvailableFreeSpace >= requiredSpace * (1024 * 1024 * 1024);
        }

        /// <summary>
        /// 获取所有系统的磁盘信息
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<DiskModel> GetAllDrives()
        {
            ObservableCollection<DiskModel> DiskDrives = new ObservableCollection<DiskModel>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    DiskDrives.Add(new DiskModel
                    {
                        Name = drive.Name,
                        TotalSpaceGB = Math.Round((double)drive.TotalSize / (1024 * 1024 * 1024), 2), // 转换为GB
                        UsedSpaceGB = Math.Round((double)(drive.TotalSize - drive.TotalFreeSpace) / (1024 * 1024 * 1024), 2),
                        FreeSpaceGB = Math.Round((double)drive.AvailableFreeSpace / (1024 * 1024 * 1024), 2)
                    });
                }
            }
            return DiskDrives;
        }

    }
}
