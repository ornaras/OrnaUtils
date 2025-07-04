using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OrnaUtils
{
    public class DirectoryWatcher
    {
        private List<string> parts = new List<string>();

        public DirectoryWatcher(Environment.SpecialFolder specialFolder) : 
            this(Environment.GetFolderPath(specialFolder)) { }

        public DirectoryWatcher(string path)
        {
            if(path is null) throw new ArgumentNullException("path");
            parts.AddRange(path.Split(Path.DirectorySeparatorChar));
        }

        public DirectoryWatcher(DirectoryWatcher source)
        {
            parts.AddRange(source.parts);
        }

        public DirectoryWatcher Add(params string[] parts)
        {
            this.parts.AddRange(parts);
            return this;
        }

        public override string ToString()
        {
            var path = new StringBuilder();
            foreach(var part in parts)
            {
                path.Append($"{part}{Path.DirectorySeparatorChar}");
                if (Directory.Exists(path.ToString())) continue;
                Directory.CreateDirectory(path.ToString());
            }
            return path.ToString();
        }

        public static explicit operator string(DirectoryWatcher watcher) =>
            watcher.ToString();
        public static explicit operator DirectoryWatcher(Environment.SpecialFolder specialFolder) =>
            new DirectoryWatcher(specialFolder);
        public static explicit operator DirectoryWatcher(string path) =>
            new DirectoryWatcher(path);
    }
}
