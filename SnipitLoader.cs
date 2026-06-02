using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel;

namespace Snipit
{
    public class SnipitLoader : GH_AssemblyPriority
    {
        private static Timer _timer;

        public override GH_LoadingInstruction PriorityLoad()
        {
            _timer = new Timer { Interval = 300 };
            _timer.Tick += TryInject;
            _timer.Start();
            return GH_LoadingInstruction.Proceed;
        }

        private static void TryInject(object sender, EventArgs e)
        {
            var editor = Instances.DocumentEditor;
            if (editor == null) return;

            var toolbar = FindCanvasToolbar(editor);
            if (toolbar == null) return;

            _timer.Stop();
            AddSnipitButton(toolbar);
        }

        private static ToolStrip FindCanvasToolbar(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is ToolStrip ts && !(c is MenuStrip)
                    && ts.Items.OfType<ToolStripComboBox>().Any())
                    return ts;

                var nested = FindCanvasToolbar(c);
                if (nested != null) return nested;
            }
            return null;
        }

        private static void AddSnipitButton(ToolStrip toolbar)
        {
            if (toolbar.Items.OfType<ToolStripButton>().Any(b => b.Text == "Snipit"))
                return;

            toolbar.Items.Add(new ToolStripSeparator());

            var button = new ToolStripButton
            {
                Text = "Snipit",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Snipit",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
            button.Click += (s, e) => { /* functionality added later */ };
            toolbar.Items.Add(button);
        }
    }
}