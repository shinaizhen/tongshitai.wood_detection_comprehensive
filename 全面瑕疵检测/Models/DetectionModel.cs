using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using 全面瑕疵检测.Services;

namespace 全面瑕疵检测.Models
{
    internal partial class DetectionModel:ObservableObject
    {
        public static string[]? Classes = GetClasses();

        static string[]? GetClasses()
        {
            string classesPath = "Resources/classes.txt";
            if (!File.Exists(classesPath))
            {
                NlogService.Error("classes.txt not found");
                return null;
            }
            string[] classes = File.ReadAllLines(classesPath);
            return classes;
        }

        [ObservableProperty]
        private int iD;
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private bool isOK;
    }
}
