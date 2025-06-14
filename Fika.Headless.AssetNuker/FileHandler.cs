using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System.Reflection;

namespace Fika.Headless.AssetNuker
{
    internal static class FileHandler
    {
        private static readonly IImageEncoder _pngEncoder = new PngEncoder();

        public static async void HandleBundleFile(AssetsManager manager, FileInfo fileInfo)
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
                    await Program._replacementImage.SaveAsync(ms, _pngEncoder);
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

        public static async void HandleAssetsFile(AssetsManager manager, FileInfo fileInfo)
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
                    await Program._replacementImage.SaveAsync(ms, _pngEncoder);
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
    }
}
