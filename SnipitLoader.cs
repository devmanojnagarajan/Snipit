using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Snipit
{
    /// <summary>
    /// Adds the Snipit button to Grasshopper's canvas toolbar (the strip named
    /// "CanvasToolbar" that holds the zoom controls). Event-driven, no polling:
    ///   PriorityLoad -> CanvasCreated -> editor.Shown -> add button.
    /// </summary>
    public class SnipitLoader : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += OnCanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private static void OnCanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= OnCanvasCreated;

            var editor = Instances.DocumentEditor;
            if (editor == null) return;

            editor.Shown += OnEditorShown;
        }

        private static void OnEditorShown(object sender, EventArgs e)
        {
            var editor = sender as GH_DocumentEditor;
            if (editor == null) return;
            editor.Shown -= OnEditorShown;

            var toolbar = editor.Controls.Find("CanvasToolbar", true)
                .OfType<ToolStrip>()
                .FirstOrDefault();
            if (toolbar == null) return;

            var loadedBitmap = System.Reflection.Assembly
                                .GetExecutingAssembly()
                                .GetManifestResourceStream("Snipit.Resource.icon.png");
            Image loadedImage = loadedBitmap != null ? new Bitmap(loadedBitmap) : null;

            if (toolbar.Items.OfType<ToolStripButton>().Any(b => b.Text == "Snipit"))
                return;

            toolbar.Items.Add(new ToolStripSeparator());
            var button =  new ToolStripButton 
            {
                Text = "Snipit",
                DisplayStyle = ToolStripItemDisplayStyle.Image,  
                Image = loadedImage,
                ToolTipText = "Snipit"                
            };
            button.Click += OnSnipitClick;
            toolbar.Items.Add(button);
            

        }

        private static void OnSnipitClick(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();

            var capture = new ToolStripMenuItem("Capture Selection");
            var deploy = new ToolStripMenuItem("Deploy Snipit");

            menu.Items.Add(capture);
            menu.Items.Add(deploy);

            if (sender is ToolStripButton btn && btn.Owner != null)
                menu.Show(btn.Owner.PointToScreen(
                    new System.Drawing.Point(btn.Bounds.Left, btn.Bounds.Bottom)));
        }
    }
}