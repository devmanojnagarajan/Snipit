using System;
using System.Drawing;
using System.Windows.Forms;

namespace Snipit
{
    /// <summary>
    /// Minimal name-entry dialog for capturing a snipit. Deliberately small —
    /// this same form will grow later to add a tab picker and a thumbnail
    /// preview, becoming the full capture panel.
    /// </summary>
    public class SnipitNameDialog : Form
    {
        private readonly TextBox _nameBox;

        /// <summary>The name the user entered (valid only when DialogResult == OK).</summary>
        public string SnipitName => _nameBox.Text.Trim();

        public SnipitNameDialog(string defaultName = "")
        {
            // --- form chrome ---
            Text = "Save Snipit";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(320, 110);

            // --- label ---
            var label = new Label
            {
                Text = "Name for this snipit:",
                Location = new Point(12, 15),
                AutoSize = true
            };

            // --- text field ---
            _nameBox = new TextBox
            {
                Text = defaultName,
                Location = new Point(15, 38),
                Width = 290
            };
            _nameBox.SelectAll();

            // --- OK button ---
            var okButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Location = new Point(149, 72),
                Width = 75
            };

            // --- Cancel button ---
            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(230, 72),
                Width = 75
            };

            Controls.Add(label);
            Controls.Add(_nameBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            // Enter = Save, Esc = Cancel.
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
    }
}