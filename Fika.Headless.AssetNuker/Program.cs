using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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
        private static readonly IImageEncoder _pngEncoder = new PngEncoder();
        private static readonly Lock _fileLock = new();
        private static readonly DirectoryInfo _runningDirectory = new(Directory.GetCurrentDirectory());

        private static Image<Bgra32> _replacementImage;

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

            if (!File.Exists(@$"{_runningDirectory.FullName}\BepInEx\plugins\Fika.Headless.dll"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the 'Fika.Headless.dll' file! Make sure the application is in your Headless installation folder and that the headless plugin is installed.");
                return Task.FromResult(false);
            }

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
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(files, parallelOptions, RenameAndDeleteFiles);
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
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(files, parallelOptions, ProcessFile);
        }

        private static ValueTask ProcessFile(FileInfo fileInfo, CancellationToken ct)
        {
            AssetsManager manager = new();
            if (fileInfo.FullName.EndsWith(".assets"))
            {
                HandleAssetsFile(manager, fileInfo);
            }
            else
            {
                HandleBundleFile(manager, fileInfo);
            }

            return ValueTask.CompletedTask;
        }

        private static async void HandleBundleFile(AssetsManager manager, FileInfo fileInfo)
        {
            Console.WriteLine($"Opening bundle {fileInfo.FullName}");

            BundleFileInstance bundle = manager.LoadBundleFile(fileInfo.FullName);
            if (bundle.file.BlockAndDirInfo.DirectoryInfos.Count < 1)
            {
                return;
            }

            AssetsFileInstance assets = manager.LoadAssetsFileFromBundle(bundle, 0);
            if (!assets.file.Metadata.TypeTreeEnabled)
            {
                manager.LoadClassDatabaseFromPackage(assets.file.Metadata.UnityVersion);
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly
                .GetManifestResourceNames()
                .Where(x => x.Contains("emptyTexture.png"))
                .First();

            if (bundle.file.BlockAndDirInfo.DirectoryInfos.Count > 1)
            {
                List<AssetBundleDirectoryInfo> dirInfos = bundle.file.BlockAndDirInfo.DirectoryInfos
                    .Where(x => x.Name.EndsWith(".resS") || x.Name.EndsWith(".resource"))
                    .ToList();
                foreach (AssetBundleDirectoryInfo dirInfo in dirInfos)
                {
                    dirInfo.Replacer = new ContentRemover();
                }
            }

            foreach (AssetFileInfo asset in assets.file.GetAssetsOfType(AssetClassID.Texture2D))
            {
                AssetTypeValueField textureBase = manager.GetBaseField(assets, asset.PathId);
                TextureFile texture = TextureFile.ReadTextureFile(textureBase);
                Console.WriteLine($"Replacing Texture2D: {texture.m_Name}");

                await using (MemoryStream ms = new())
                {
                    await _replacementImage.SaveAsync(ms, _pngEncoder);
                    texture.SetTextureDataRaw(ms.ToArray(), 4, 4);
                    texture.WriteTo(textureBase);
                }

                asset.SetNewData(textureBase);
            }

            foreach (AssetFileInfo asset in assets.file.GetAssetsOfType(AssetClassID.AudioClip))
            {
                AssetTypeValueField audioBase = manager.GetBaseField(assets, asset);
                Console.WriteLine($"Replacing Audio: {audioBase["m_Name"].AsString}");

                audioBase["m_Resource"]["m_Source"].Value.AsString = "resources.resource";
                audioBase["m_Resource"]["m_Offset"].AsULong = 95203392;
                audioBase["m_Resource"]["m_Size"].AsUInt = 128;

                asset.SetNewData(audioBase);
            }

            bundle.file.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assets.file);

            await using (AssetsFileWriter writer = new(fileInfo.FullName + ".mod"))
            {
                bundle.file.Write(writer);
            }

            manager.UnloadAll();
        }

        private static async void HandleAssetsFile(AssetsManager manager, FileInfo fileInfo)
        {
            Console.WriteLine($"Opening assets {fileInfo.FullName}");

            manager.LoadClassPackage("uncompressed.tpk");
            AssetsFileInstance assets = manager.LoadAssetsFile(fileInfo.FullName, true);
            if (!assets.file.Metadata.TypeTreeEnabled)
            {
                manager.LoadClassDatabaseFromPackage(new UnityVersion(assets.file.Metadata.UnityVersion));
            }

            foreach (AssetFileInfo asset in assets.file.GetAssetsOfType(AssetClassID.Texture2D))
            {
                AssetTypeValueField textureBase = manager.GetBaseField(assets, asset.PathId);
                TextureFile texture = TextureFile.ReadTextureFile(textureBase);
                Console.WriteLine($"Replacing Texture2D: {texture.m_Name}");

                await using (MemoryStream ms = new())
                {
                    await _replacementImage.SaveAsync(ms, _pngEncoder);
                    texture.SetTextureDataRaw(ms.ToArray(), 4, 4);
                    texture.WriteTo(textureBase);
                }

                asset.SetNewData(textureBase);
            }

            foreach (AssetFileInfo asset in assets.file.GetAssetsOfType(AssetClassID.AudioClip))
            {
                AssetTypeValueField audioBase = manager.GetBaseField(assets, asset);
                Console.WriteLine($"Replacing Audio: {audioBase["m_Name"].AsString}");

                audioBase["m_Resource"]["m_Source"].Value.AsString = "resources.resource";
                audioBase["m_Resource"]["m_Offset"].AsULong = 95203392;
                audioBase["m_Resource"]["m_Size"].AsUInt = 128;

                asset.SetNewData(audioBase);
            }

            await using (AssetsFileWriter writer = new(fileInfo.FullName + ".mod"))
            {
                assets.file.Write(writer);
            }

            manager.UnloadAll();
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

        private static Task<List<FileInfo>> GetAllFiles()
        {
            Console.WriteLine("Getting all files from the Data directory, this can take some time...");
            return Task.Run(() =>
            {
                List<FileInfo> fileInfos = [];
                string[] files = Directory.GetFiles(@$"{_runningDirectory.FullName}\EscapeFromTarkov_Data", "*.*", SearchOption.AllDirectories);
                foreach (string item in files)
                {
                    FileInfo fileInfo = new(item);
                    if (!IsValidFile(fileInfo))
                    {
                        continue;
                    }

                    fileInfos.Add(new(item));
                }

                return fileInfos;
            });
        }
    }
}
