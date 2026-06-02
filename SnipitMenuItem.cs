using System;
using System.Drawing;
using System.Windows.Forms;

namespace Snipit
{
    /// <summary>
    /// A menu item that paints an "x" on its right edge and fires DeleteClicked
    /// when that x is clicked (instead of the normal item Click). Used in the
    /// Deploy dropdown so each saved snipit has an inline delete button.
    /// </summary>
    public class SnipitMenuItem : ToolStripMenuItem
    {
        public event EventHandler DeleteClicked;

        // Size of the clickable x hot-zone on the right edge.
        private const int XBoxSize = 16;
        private const int RightPadding = 6;

        public SnipitMenuItem(string text) : base(text)
        {
            // Reserve room on the right so the label never overlaps the x.
            Padding = new Padding(0, 0, XBoxSize + RightPadding, 0);
        }

        // The rectangle (in item-local coords) where the x is drawn / clicked.
        private Rectangle XBounds =>
            new Rectangle(
                Width - XBoxSize - RightPadding,
                (Height - XBoxSize) / 2,
                XBoxSize, XBoxSize);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var r = XBounds;

            // Draw the x as two diagonal strokes.
            using (var pen = new Pen(Color.Firebrick, 1.6f))
            {
                int pad = 4;
                g.DrawLine(pen, r.Left + pad, r.Top + pad,
                                r.Right - pad, r.Bottom - pad);
                g.DrawLine(pen, r.Right - pad, r.Top + pad,
                                r.Left + pad, r.Bottom - pad);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // If the click landed on the x, fire DeleteClicked and swallow it
            // so the normal Click (deploy) does NOT also fire.
            if (XBounds.Contains(e.Location))
            {
                DeleteClicked?.Invoke(this, EventArgs.Empty);
                return;
            }
            base.OnMouseDown(e);
        }
    }
}