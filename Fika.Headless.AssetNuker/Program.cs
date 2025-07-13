using AssetsTools.NET.Extra;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
using System.Reflection;

namespace Fika.Headless.AssetNuker
{

    /// <summary>
    /// <see href="https://github.com/GrooveypenguinX/WTT-BundleMaster/blob/main/WTT_BundleMaster/Services/ReplacerService.cs"/> <br/>
    /// <see href="https://github.com/nesrak1/AssetsTools.NET/wiki/Getting-Started:-Bundle-file-writing"/>
    /// </summary>
    internal class Program
    {
        private static readonly int _signatureLength = 7;
        private static readonly Lock _fileLock = new();
        private static readonly DirectoryInfo _runningDirectory = new(Directory.GetCurrentDirectory());
        private static readonly ParallelOptions _parallelOptions = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        public static Image<Bgra32> _replacementImage { get; private set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Running prechecks");
            if (!await RunPreChecks())
            {
                Console.ResetColor();
                Console.WriteLine("Failed to run prechecks. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Caching assets to inject");

            await CacheReplacements();

            List<FileInfo> files = await GetAllFiles();
            Console.WriteLine($"Loaded {files.Count} files");

            files.RemoveAll(f => f.Name == "globalgamemanagers.assets");

            await ProcessFiles(files);
            await RenameModFiles(files);

            Console.WriteLine($"{files.Count} files were nuked.");
            Console.ReadKey();
        }

        private static Task<bool> RunPreChecks()
        {
            if (!Directory.Exists(@$"{_runningDirectory.FullName}\EscapeFromTarkov_Data"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the 'EscapeFromTarkov_Data' folder! Make sure the application is in your Headless installation folder.");
                return Task.FromResult(false);
            }

#if RELEASE
            if (!File.Exists(@$"{_runningDirectory.FullName}\BepInEx\plugins\Fika.Headless.dll"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the 'Fika.Headless.dll' file! Make sure the application is in your Headless installation folder and that the headless plugin is installed.");
                return Task.FromResult(false);
            } 
#endif

            if (!File.Exists(@$"{_runningDirectory.FullName}\uncompressed.tpk"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the 'uncompressed.tpk' file! Make sure that you extracted it properly.");
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private static async Task CacheReplacements()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resources = assembly.GetManifestResourceNames();
            string textureName = resources
                .Where(x => x.Contains("emptyTexture.png"))
                .First();

            Stream emptyStream = assembly.GetManifestResourceStream(textureName)
                ?? throw new NullReferenceException("Could not find emptyTexture stream");

            await using MemoryStream memoryStream = new();
            await emptyStream.CopyToAsync(memoryStream);
            _replacementImage = Image.Load<Bgra32>(memoryStream.ToArray());

            if (_replacementImage == null)
            {
                throw new NullReferenceException("Image was not loaded");
            }
        }

        private static async Task RenameModFiles(List<FileInfo> files)
        {
            await Parallel.ForEachAsync(files, _parallelOptions, RenameAndDeleteFiles);
        }

        private static ValueTask RenameAndDeleteFiles(FileInfo fileInfo, CancellationToken token)
        {
            Console.WriteLine($"Replacing {fileInfo.Name} with modified file");
            File.Delete(fileInfo.FullName);
            File.Move(fileInfo.FullName + ".mod", fileInfo.FullName);

            return ValueTask.CompletedTask;
        }

        private static async Task ProcessFiles(List<FileInfo> files)
        {
            await Parallel.ForEachAsync(files, _parallelOptions, ProcessFile);
        }

        private static ValueTask ProcessFile(FileInfo fileInfo, CancellationToken ct)
        {
            AssetsManager manager = new();
            if (fileInfo.FullName.EndsWith(".assets"))
            {
                FileHandler.HandleAssetsFile(manager, fileInfo);
            }
            else
            {
                FileHandler.HandleBundleFile(manager, fileInfo);
            }

            return ValueTask.CompletedTask;
        }

        private static bool IsValidFile(FileInfo fileInfo)
        {
            if (fileInfo.Name.EndsWith(".assets"))
            {
                return true;
            }

            if (fileInfo.Name.EndsWith(".mod"))
            {
                return true;
            }

            if (fileInfo.Length < _signatureLength)
            {
                return false;
            }

            Span<byte> buffer = stackalloc byte[_signatureLength];
            using (FileStream fileStream = new(fileInfo.FullName, FileMode.Open))
            {
                fileStream.ReadExactly(buffer);

                string signature = System.Text.Encoding.ASCII.GetString(buffer);
                StringComparison comparison = StringComparison.Ordinal;
                return signature.StartsWith("UnityFS", comparison)
                    || signature.StartsWith("UnityWeb", comparison)
                    || signature.StartsWith("UnityRaw", comparison);
            }
        }

        private async static Task<List<FileInfo>> GetAllFiles()
        {
            Console.WriteLine("Getting all files from the Data directory, this can take some time...");

            ConcurrentBag<FileInfo> fileInfos = [];

            string rootPath = Path.Combine(_runningDirectory.FullName, "EscapeFromTarkov_Data");
            IEnumerable<string> filePaths = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories);

            await Parallel.ForEachAsync(filePaths, _parallelOptions, (item, cancellationToken) =>
            {
                FileInfo fileInfo = new(item);

                if (IsValidFile(fileInfo))
                {
                    fileInfos.Add(fileInfo);
                }

                return ValueTask.CompletedTask;
            });

            return fileInfos.ToList();
        }
    }
}
