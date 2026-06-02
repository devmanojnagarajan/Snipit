using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Snipit
{
    /// <summary>
    /// GH_AssemblyPriority runs before any components load.
    /// We use it to hook into the editor and inject our toolbar button.
    /// </summary>
    public class SnipitLoader : GH_AssemblyPriority
    {
        private static ToolStripButton _snipitButton;

        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += OnCanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void OnCanvasCreated(GH_Canvas canvas)
        {
            // Unsubscribe so we only do this once.
            Instances.CanvasCreated -= OnCanvasCreated;

            // The editor might not be ready yet, so defer slightly.
            var editor = Instances.DocumentEditor;
            if (editor == null) return;

            editor.Load += (s, e) => InjectToolbarButton(editor);
        }

        private static void InjectToolbarButton(GH_DocumentEditor editor)
        {
            // The GH editor has ToolStrip controls in its form.
            // We find the main toolbar and append our button.
            var toolStrip = editor.Controls.OfType<ToolStrip>()
                .FirstOrDefault(ts => ts.Items.Count > 5);

            if (toolStrip == null) return;

            // Separator to visually group our button.
            toolStrip.Items.Add(new ToolStripSeparator());

            // The button itself.
            _snipitButton = new ToolStripButton
            {
                Text = "Snipit",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Save or deploy a Snipit",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            _snipitButton.Click += OnSnipitButtonClick;
            toolStrip.Items.Add(_snipitButton);
        }

        private static void OnSnipitButtonClick(object sender, EventArgs e)
        {
            // For now, just show a test message to prove the button works.
            // We will replace this with the capture/deploy popup.
            var doc = Instances.ActiveCanvas?.Document;
            if (doc == null)
            {
                MessageBox.Show("No active document.", "Snipit");
                return;
            }

            var selected = doc.SelectedObjects();
            int count = selected?.Count ?? 0;

            MessageBox.Show(
                $"Snipit button works!\n\n" +
                $"Active document: {doc.DisplayName}\n" +
                $"Selected objects: {count}\n\n" +
                $"Next step: wire up capture/deploy here.",
                "Snipit",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}