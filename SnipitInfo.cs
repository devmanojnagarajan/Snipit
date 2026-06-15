using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Snipit
{
    /// <summary>
    /// Grasshopper plugin metadata. This is what shows up in
    /// Grasshopper's "Solution > Display > Loaded Assemblies" panel
    /// and identifies the plugin to GH at load time.
    /// </summary>
    public class SnipitInfo : GH_AssemblyInfo
    {
        public override string Name => "Snipit";

        // Plugin icon (24x24), scaled down from the embedded 512x512 resource.
        private static Bitmap _icon;
        public override Bitmap Icon
        {
            get
            {
                if (_icon == null)
                {
                    var stream = typeof(SnipitInfo).Assembly
                        .GetManifestResourceStream("Snipit.Resource.icon.png");
                    if (stream != null)
                        using (stream)
                        using (var full = new Bitmap(stream))
                            _icon = new Bitmap(full, 24, 24);
                }
                return _icon;
            }
        }

        public override string Description =>
            "Save and deploy reusable Grasshopper component snippets across script files.";

        public override Guid Id => new Guid("61EBA52F-B3C9-43C3-9536-9A6E4841AA6E");

        public override string AuthorName => "Manoj Nagarajan";

        public override string AuthorContact => "manojnagarajan27@gmail.com";
    }
}
