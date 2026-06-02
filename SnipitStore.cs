using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Snipit
{
    /// <summary>
    /// Per-file storage. Every snipit is an independent file, so one bad write
    /// can never take down the rest of the library.
    ///
    ///   {root}/{Tab}/{Name}.snipit   <- binary GH_Archive bytes
    ///   {root}/{Tab}/{Name}.png      <- thumbnail sidecar
    ///
    /// Root defaults to %AppData%\Snipit and is configurable for cloud sync.
    /// </summary>
    public class SnipitStore
    {
        public string Root { get; }

        public SnipitStore(string root = null)
        {
            Root = root ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Snipit");
            Directory.CreateDirectory(Root);
        }

        // ---- WRITE ---------------------------------------------------------

        public void Save(string tab, string name, byte[] snipitBytes, Bitmap thumbnail = null)
        {
            tab = Sanitize(tab);
            name = Sanitize(name);

            var tabDir = Path.Combine(Root, tab);
            Directory.CreateDirectory(tabDir);

            var path = Path.Combine(tabDir, name + ".snipit");
            AtomicWrite(path, snipitBytes);

            if (thumbnail != null)
            {
                var thumbPath = Path.Combine(tabDir, name + ".png");
                var tmp = thumbPath + ".tmp";
                thumbnail.Save(tmp, ImageFormat.Png);
                if (File.Exists(thumbPath)) File.Delete(thumbPath);
                File.Move(tmp, thumbPath);
            }
        }

        private static void AtomicWrite(string path, byte[] bytes)
        {
            var tmp = path + ".tmp";
            File.WriteAllBytes(tmp, bytes);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        // ---- READ ----------------------------------------------------------

        public List<string> ListTabs()
        {
            if (!Directory.Exists(Root)) return new List<string>();
            return Directory.GetDirectories(Root)
                .Select(Path.GetFileName)
                .OrderBy(x => x)
                .ToList();
        }

        public List<SnipitEntry> ListSnipits(string tab)
        {
            var result = new List<SnipitEntry>();
            var tabDir = Path.Combine(Root, Sanitize(tab));
            if (!Directory.Exists(tabDir)) return result;

            foreach (var file in Directory.GetFiles(tabDir, "*.snipit"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var thumb = Path.Combine(tabDir, name + ".png");
                result.Add(new SnipitEntry
                {
                    Tab = tab,
                    Name = name,
                    Path = file,
                    ThumbnailPath = File.Exists(thumb) ? thumb : null
                });
            }
            return result.OrderBy(s => s.Name).ToList();
        }

        public byte[] Load(SnipitEntry info) => File.ReadAllBytes(info.Path);

        public List<SnipitEntry> Search(string query)
        {
            query = (query ?? "").Trim();
            var all = ListTabs().SelectMany(ListSnipits);
            if (query.Length == 0) return all.ToList();
            return all.Where(s =>
                s.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // ---- RENAME / DELETE ----------------------------------------------

        public void Rename(SnipitEntry info, string newName)
        {
            newName = Sanitize(newName);
            var dir = Path.GetDirectoryName(info.Path);
            File.Move(info.Path, Path.Combine(dir, newName + ".snipit"));
            if (info.ThumbnailPath != null && File.Exists(info.ThumbnailPath))
                File.Move(info.ThumbnailPath, Path.Combine(dir, newName + ".png"));
        }

        public void Delete(SnipitEntry info)
        {
            if (File.Exists(info.Path)) File.Delete(info.Path);
            if (info.ThumbnailPath != null && File.Exists(info.ThumbnailPath))
                File.Delete(info.ThumbnailPath);
        }

        public void RenameTab(string oldTab, string newTab)
        {
            var src = Path.Combine(Root, Sanitize(oldTab));
            var dst = Path.Combine(Root, Sanitize(newTab));
            if (Directory.Exists(src)) Directory.Move(src, dst);
        }

        public void DeleteTab(string tab)
        {
            var dir = Path.Combine(Root, Sanitize(tab));
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        // ---- helpers -------------------------------------------------------

        private static string Sanitize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "Untitled";
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s.Trim();
        }
    }

    public class SnipitEntry
    {
        public string Tab;
        public string Name;
        public string Path;
        public string ThumbnailPath;
    }
}