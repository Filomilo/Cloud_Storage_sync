namespace Cloud_Storage_Common
{
    public static class SharedData
    {
        public static string GetAppDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudDriveSync"
            );
        }
    }
}
