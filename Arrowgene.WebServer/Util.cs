using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Arrowgene.WebServer
{
    internal class Util
    {
        private static readonly Random RandomNum = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (RandomNum)
            {
                return RandomNum.Next(min, max);
            }
        }

        public static long GetUnixTime(DateTime dateTime)
        {
            return ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
        }

        public static string PathDifferenceEnd(string directoryInfo1, string directoryInfo2, bool unRoot)
        {
            return PathDifference(new DirectoryInfo(directoryInfo1), new DirectoryInfo(directoryInfo2), unRoot);
        }

        public static string PathDifferenceEnd(FileSystemInfo directoryInfo1, FileSystemInfo directoryInfo2,
            bool unRoot)
        {
            string result;
            if (directoryInfo1.FullName == directoryInfo2.FullName)
                result = directoryInfo1.FullName;
            else if (directoryInfo1.FullName.EndsWith(directoryInfo2.FullName))
                result = directoryInfo1.FullName.Split(new[] {directoryInfo2.FullName},
                    StringSplitOptions.RemoveEmptyEntries)[0];
            else if (directoryInfo2.FullName.EndsWith(directoryInfo1.FullName))
                result = directoryInfo2.FullName.Split(new[] {directoryInfo1.FullName},
                    StringSplitOptions.RemoveEmptyEntries)[0];
            else
                result = "";

            if (unRoot) result = UnRootPath(result);

            return result;
        }


        public static string PathDifference(string directoryInfo1, string directoryInfo2, bool unRoot)
        {
            return PathDifference(new DirectoryInfo(directoryInfo1), new DirectoryInfo(directoryInfo2), unRoot);
        }

        public static string PathDifference(FileSystemInfo directoryInfo1, FileSystemInfo directoryInfo2, bool unRoot)
        {
            string result;
            if (directoryInfo1.FullName == directoryInfo2.FullName)
                result = "";
            else if (directoryInfo1.FullName.StartsWith(directoryInfo2.FullName))
                result = directoryInfo1.FullName.Split(new[] {directoryInfo2.FullName},
                    StringSplitOptions.RemoveEmptyEntries)[0];
            else if (directoryInfo2.FullName.StartsWith(directoryInfo1.FullName))
                result = directoryInfo2.FullName.Split(new[] {directoryInfo1.FullName},
                    StringSplitOptions.RemoveEmptyEntries)[0];
            else
                result = "";

            if (unRoot) result = UnRootPath(result);

            return result;
        }

        public static string UnRootPath(string path)
        {
            // https://stackoverflow.com/questions/53102/why-does-path-combine-not-properly-concatenate-filenames-that-start-with-path-di
            if (Path.IsPathRooted(path))
            {
                path = path.TrimStart(Path.DirectorySeparatorChar);
                path = path.TrimStart(Path.AltDirectorySeparatorChar);
            }

            return path;
        }

        public static AssemblyName GetAssemblyName(string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName();
                if (assemblyName.Name == name) return assemblyName;
            }

            return null;
        }

        public static Version GetAssemblyVersion(string name)
        {
            var assemblyName = GetAssemblyName(name);
            if (assemblyName != null) return assemblyName.Version;

            return null;
        }

        public static string GetAssemblyVersionString(string name)
        {
            var version = GetAssemblyVersion(name);
            if (version != null) return version.ToString();

            return null;
        }

        public static byte[] ReadFile(string source)
        {
            if (!File.Exists(source))
                throw new Exception(string.Format("'{0}' does not exist or is not a file", source));

            return File.ReadAllBytes(source);
        }

        public static string ReadFileText(string source)
        {
            if (!File.Exists(source)) throw new Exception($"'{source}' does not exist or is not a file");

            return File.ReadAllText(source);
        }

        public static void WriteFile(byte[] content, string destination)
        {
            if (content != null)
                File.WriteAllBytes(destination, content);
            else
                throw new Exception($"Content of '{destination}' is null");
        }

        public static List<FileInfo> GetFiles(DirectoryInfo directoryInfo, string[] extensions, bool recursive)
        {
            if (recursive)
            {
                var filteredFiles = GetFiles(directoryInfo, extensions);
                var directoryInfos = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
                foreach (var dInfo in directoryInfos)
                {
                    var files = GetFiles(dInfo, extensions, true);
                    filteredFiles.AddRange(files);
                }

                return filteredFiles;
            }

            return GetFiles(directoryInfo, extensions);
        }

        public static List<FileInfo> GetFiles(DirectoryInfo directoryInfo, string[] extensions)
        {
            var filteredFiles = new List<FileInfo>();
            var files = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
                if (extensions != null)
                {
                    foreach (var extension in extensions)
                        if (file.Extension.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            filteredFiles.Add(file);
                            break;
                        }
                }
                else
                {
                    filteredFiles.Add(file);
                }

            return filteredFiles;
        }

        public static List<DirectoryInfo> GetFolders(DirectoryInfo directoryInfo, string[] extensions, bool recursive)
        {
            if (recursive)
            {
                var result = new List<DirectoryInfo>();
                var filteredDirectories = GetFolders(directoryInfo, extensions);
                result.AddRange(filteredDirectories);
                foreach (var directory in filteredDirectories)
                {
                    var directories = GetFolders(directory, extensions, true);
                    result.AddRange(directories);
                }

                return result;
            }

            return GetFolders(directoryInfo, extensions);
        }

        public static List<DirectoryInfo> GetFolders(DirectoryInfo directoryInfo, string[] extensions)
        {
            var filteredDirectories = new List<DirectoryInfo>();
            var directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (var directory in directories)
                if (extensions != null)
                {
                    foreach (var extension in extensions)
                        if (directory.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            filteredDirectories.Add(directory);
                            break;
                        }
                }
                else
                {
                    filteredDirectories.Add(directory);
                }

            return filteredDirectories;
        }


        public static DirectoryInfo EnsureDirectory(string directory)
        {
            return Directory.CreateDirectory(directory);
        }

        /// <summary>
        ///     The directory of the executing assembly.
        ///     This might not be the location where the .dll files are located.
        /// </summary>
        /// <returns></returns>
        public static string ExecutingDirectory()
        {
            var path = Assembly.GetEntryAssembly().CodeBase;
            var uri = new Uri(path);
            var directory = Path.GetDirectoryName(uri.LocalPath);
            return directory;
        }

        /// <summary>
        ///     The relative directory of the executing assembly.
        ///     This might not be the location where the .dll files are located.
        /// </summary>
        public static string RelativeExecutingDirectory()
        {
            return RelativeDirectory(Environment.CurrentDirectory, ExecutingDirectory());
        }

        /// <summary>
        ///     Directory of Common.dll
        ///     This is expected to contain ressource files.
        /// </summary>
        public static string CommonDirectory()
        {
            var location = typeof(Util).GetTypeInfo().Assembly.Location;
            var uri = new Uri(location);
            var directory = Path.GetDirectoryName(uri.LocalPath);
            return directory;
        }

        /// <summary>
        ///     Relative Directory of Common.dll.
        ///     This is expected to contain ressource files.
        /// </summary>
        public static string RelativeCommonDirectory()
        {
            return RelativeDirectory(Environment.CurrentDirectory, CommonDirectory());
        }

        public static string CreateMd5(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));

            return sb.ToString().ToLower();
        }

        public static string RelativeDirectory(string fromDirectory, string toDirectory)
        {
            return RelativeDirectory(fromDirectory, toDirectory, toDirectory, Path.DirectorySeparatorChar);
        }

        public static string RelativeDirectory(string fromDirectory, string toDirectory, string defaultDirectory)
        {
            return RelativeDirectory(fromDirectory, toDirectory, defaultDirectory, Path.DirectorySeparatorChar);
        }

        /// <summary>
        ///     Returns a directory that is relative.
        /// </summary>
        /// <param name="fromDirectory">The directory to navigate from.</param>
        /// <param name="toDirectory">The directory to reach.</param>
        /// <param name="defaultDirectory">A directory to return on failure.</param>
        /// <param name="directorySeparator"></param>
        /// <returns>The relative directory or the defaultDirectory on failure.</returns>
        public static string RelativeDirectory(string fromDirectory, string toDirectory, string defaultDirectory,
            char directorySeparator)
        {
            string result;

            if (fromDirectory.EndsWith("\\") || fromDirectory.EndsWith("/"))
                fromDirectory = fromDirectory.Remove(fromDirectory.Length - 1);

            if (toDirectory.EndsWith("\\") || toDirectory.EndsWith("/"))
                toDirectory = toDirectory.Remove(toDirectory.Length - 1);

            if (toDirectory.StartsWith(fromDirectory))
            {
                result = toDirectory.Substring(fromDirectory.Length);
                if (result.StartsWith("\\") || result.StartsWith("/")) result = result.Substring(1, result.Length - 1);

                if (result != "") result += directorySeparator;
            }
            else
            {
                var fromDirs = fromDirectory.Split(':', '\\', '/');
                var toDirs = toDirectory.Split(':', '\\', '/');
                if (fromDirs.Length <= 0 || toDirs.Length <= 0 || fromDirs[0] != toDirs[0]) return defaultDirectory;

                var offset = 1;
                for (; offset < fromDirs.Length; offset++)
                {
                    if (toDirs.Length <= offset) break;

                    if (fromDirs[offset] != toDirs[offset]) break;
                }

                var relativeBuilder = new StringBuilder();
                for (var i = 0; i < fromDirs.Length - offset; i++)
                {
                    relativeBuilder.Append("..");
                    relativeBuilder.Append(directorySeparator);
                }

                for (var i = offset; i < toDirs.Length - 1; i++)
                {
                    relativeBuilder.Append(toDirs[i]);
                    relativeBuilder.Append(directorySeparator);
                }

                result = relativeBuilder.ToString();
            }

            result = DirectorySeparator(result, directorySeparator);
            return result;
        }

        public static string DirectorySeparator(string path)
        {
            return DirectorySeparator(path, Path.DirectorySeparatorChar);
        }

        public static string DirectorySeparator(string path, char directorySeparator)
        {
            if (directorySeparator != '\\') path = path.Replace('\\', directorySeparator);

            if (directorySeparator != '/') path = path.Replace('/', directorySeparator);

            return path;
        }

        public static string GenerateSessionKey(int desiredLength)
        {
            var sessionKey = new StringBuilder();
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                var random = new byte[1];
                var length = 0;
                while (length < desiredLength)
                {
                    cryptoProvider.GetBytes(random);
                    var c = (char) random[0];
                    if ((char.IsDigit(c) || char.IsLetter(c)) && random[0] < 127)
                    {
                        length++;
                        sessionKey.Append(c);
                    }
                }
            }

            return sessionKey.ToString();
        }

        public static byte[] GenerateKey(int desiredLength)
        {
            var random = new byte[desiredLength];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                cryptoProvider.GetNonZeroBytes(random);
            }

            return random;
        }

        /// <summary>
        ///     Removes entries from a collection.
        ///     The input lists are not modified, instead a new collection is returned.
        /// </summary>
        public static TList SubtractList<TList, TItem>(TList entries, params TItem[] excepts)
            where TList : ICollection<TItem>, new()
        {
            var result = new TList();
            foreach (var entry in entries) result.Add(entry);

            foreach (var except in excepts) result.Remove(except);

            return result;
        }

        public static byte[] FromHexString(string hexString)
        {
            if ((hexString.Length & 1) != 0) throw new ArgumentException("Input must have even number of characters");
            var ret = new byte[hexString.Length / 2];
            for (var i = 0; i < ret.Length; i++)
            {
                int high = hexString[i * 2];
                int low = hexString[i * 2 + 1];
                high = (high & 0xf) + ((high & 0x40) >> 6) * 9;
                low = (low & 0xf) + ((low & 0x40) >> 6) * 9;

                ret[i] = (byte) ((high << 4) | low);
            }

            return ret;
        }

        public static string ToHexString(byte[] data, char? seperator = null)
        {
            var sb = new StringBuilder();
            var len = data.Length;
            for (var i = 0; i < len; i++)
            {
                sb.Append(data[i].ToString("X2"));
                if (seperator != null && i < len - 1) sb.Append(seperator);
            }

            return sb.ToString();
        }

        public static string ToAsciiString(byte[] data, bool spaced)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                var c = '.';
                if (data[i] >= 'A' && data[i] <= 'Z') c = (char) data[i];
                if (data[i] >= 'a' && data[i] <= 'z') c = (char) data[i];
                if (data[i] >= '0' && data[i] <= '9') c = (char) data[i];
                if (spaced && i != 0) sb.Append("  ");

                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string[] ParseTextArguments(string line, char delimiter, char textQualifier)
        {
            var list = ParseTextList(line, delimiter, textQualifier);
            var count = list.Count;
            var arguments = new string[count];
            for (var i = 0; i < count; i++) arguments[i] = list[i];

            return arguments;
        }

        public static IEnumerable<string> ParseTextEnumerable(string line, char delimiter, char textQualifier)
        {
            if (string.IsNullOrWhiteSpace(line))
                yield break;

            var prevChar = '\0';
            var nextChar = '\0';
            var currentChar = '\0';
            var inString = false;

            var token = new StringBuilder();

            for (var i = 0; i < line.Length; i++)
            {
                currentChar = line[i];

                if (i > 0)
                    prevChar = line[i - 1];
                else
                    prevChar = '\0';

                if (i + 1 < line.Length)
                    nextChar = line[i + 1];
                else
                    nextChar = '\0';

                if (currentChar == textQualifier && (prevChar == '\0' || prevChar == delimiter) && !inString)
                {
                    inString = true;
                    continue;
                }

                if (currentChar == textQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
                {
                    inString = false;
                    continue;
                }

                if (currentChar == delimiter && !inString)
                {
                    yield return token.ToString();
                    token = token.Remove(0, token.Length);
                    continue;
                }

                token = token.Append(currentChar);
                yield return token.ToString();
            }
        }

        public static IList<string> ParseTextList(string line, char delimiter, char textQualifier)
        {
            IList<string> collection = new List<string>();
            if (string.IsNullOrWhiteSpace(line))
                return collection;

            var prevChar = '\0';
            var nextChar = '\0';
            var currentChar = '\0';
            var inString = false;

            var token = new StringBuilder();

            for (var i = 0; i < line.Length; i++)
            {
                currentChar = line[i];

                if (i > 0)
                    prevChar = line[i - 1];
                else
                    prevChar = '\0';

                if (i + 1 < line.Length)
                    nextChar = line[i + 1];
                else
                    nextChar = '\0';

                if (currentChar == textQualifier && (prevChar == '\0' || prevChar == delimiter) && !inString)
                {
                    inString = true;
                    continue;
                }

                if (currentChar == textQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
                {
                    inString = false;
                    continue;
                }

                if (currentChar == delimiter && !inString)
                {
                    collection.Add(token.ToString());
                    token = token.Remove(0, token.Length);
                    continue;
                }

                token = token.Append(currentChar);
            }

            collection.Add(token.ToString());
            return collection;
        }

        /// <summary>
        ///     Read a stream till the end and return the read bytes.
        /// </summary>
        public static async Task<byte[]> ReadAsync(Stream stream)
        {
            var bufferSize = 1024;
            var buffer = new byte[bufferSize];
            var result = new byte[0];
            var offset = 0;
            var read = 0;
            while ((read = await stream.ReadAsync(buffer, 0, bufferSize)) > 0)
            {
                var newSize = offset + read;
                var temp = new byte[newSize];
                Buffer.BlockCopy(result, 0, temp, 0, offset);
                Buffer.BlockCopy(buffer, 0, temp, offset, read);
                result = temp;
                offset += read;
            }

            return result;
        }
    }
}