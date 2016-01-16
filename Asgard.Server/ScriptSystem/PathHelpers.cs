using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.ScriptSystem
{
    public static class PathHelpers
    {
        static string _basePath;
        public static void SetBasePath(string path)
        {
            if (path == Path.GetFullPath(path))
            {
                _basePath = Path.GetDirectoryName(path);
            }
            else
            {
                var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                _basePath = Path.Combine(exePath, path);
                _basePath = Path.GetFullPath(_basePath);
            }
        }
        public static string Resolve(string file)
        {
            if (file.StartsWith(_basePath))
            {
                var fullFile = Path.GetFullPath(file);
                if (fullFile.StartsWith(_basePath))
                {
                    if (File.Exists(fullFile))
                    {
                        return fullFile;
                    }
                }
            }

            var newFile = Path.GetFullPath(Path.Combine(_basePath, file));
            if (newFile.StartsWith(_basePath))
            {
                return newFile;
            }

            return "";

        }
    }
}
