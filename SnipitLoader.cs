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
            capture.Click += OnCapture;

            var deploy = new ToolStripMenuItem("Deploy Snipit");
            deploy.Click += OnDeploy;



            menu.Items.Add(capture);
            menu.Items.Add(deploy);

            if (sender is ToolStripButton btn && btn.Owner != null)
                menu.Show(btn.Owner.PointToScreen(
                    new System.Drawing.Point(btn.Bounds.Left, btn.Bounds.Bottom)));
        }

        private static void OnCapture(object sender, EventArgs e)
        {
            var doc = Instances.ActiveCanvas?.Document;
            var bytes = SnipitEngine.CaptureSelection(doc, out int count);
            if (bytes == null)
            {
                MessageBox.Show("Select some components on the canvas first.", "Snipit");
                return;
            }

            using (var dialog = new SnipitNameDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                if (string.IsNullOrWhiteSpace(dialog.SnipitName)) return;

                new SnipitStore().Save("General", dialog.SnipitName, bytes);
                MessageBox.Show($"Saved '{dialog.SnipitName}' ({count} objects).", "Snipit");
            }
        }

        private static void OnDeploy(object sender, EventArgs e)
        {
            var canvas = Instances.ActiveCanvas;
            var doc = canvas?.Document;
            if (doc == null) return;

            var store = new SnipitStore();
            var snipits = store.ListSnipits("General");
            if (snipits.Count == 0)
            {
                MessageBox.Show("No saved snipits yet.", "Snipit");
                return;
            }

            var menu = new ContextMenuStrip();
            menu.RenderMode = ToolStripRenderMode.System;

            foreach (var entry in snipits)
            {
                var captured = entry;
                var item = new ToolStripMenuItem(entry.Name);

                // Left-click = deploy at cursor.
                item.Click += (s, ev) =>
                {
                    // Only deploy on a left-click; right-click is handled separately.
                    if (ev is MouseEventArgs me && me.Button == MouseButtons.Right) return;

                    var screenPt = Cursor.Position;
                    var clientPt = canvas.PointToClient(screenPt);
                    var canvasPt = canvas.Viewport.UnprojectPoint(clientPt);
                    var bytes = store.Load(captured);
                    SnipitEngine.Deploy(bytes, doc, canvasPt, out _);
                };

                // Right-click = show a small Delete menu.
                item.MouseDown += (s, me) =>
                {
                    if (me.Button != MouseButtons.Right) return;

                    // Close the deploy menu first so it doesn't fight for focus.
                    menu.Close();

                    var confirm = MessageBox.Show(
                        $"Delete snipit '{captured.Name}'?", "Snipit",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes)
                        store.Delete(captured);
                };

                menu.Items.Add(item);
            }
            menu.Show(Cursor.Position);
        }

    }
}