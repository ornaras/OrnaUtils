namespace System.IO
{
    public static class PathExtensions
    {
        public static string AsRequiredDirectory(this string path)
        {
            if (!Directory.Exists(path))
            {
                var index = path.LastIndexOf(Path.DirectorySeparatorChar);
                AsRequiredDirectory(path.Substring(0, index));
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
