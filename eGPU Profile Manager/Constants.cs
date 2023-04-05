using System;
using System.IO;

namespace eGPU_Profile_Manager
{
    internal static class Constants
    {
        internal static readonly string ProfilesFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "eGPU Profiles");
    }
}
