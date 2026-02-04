using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NotePadSummary;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "Over NotePad Summary";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(400, 320);
        BackColor = Color.White;

        // Gradient panel bovenaan
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80
        };
        headerPanel.Paint += (s, e) =>
        {
            using var brush = new LinearGradientBrush(
                headerPanel.ClientRectangle,
                Color.FromArgb(70, 130, 180),
                Color.FromArgb(100, 149, 237),
                LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
        };

        // App naam in header
        var titleLabel = new Label
        {
            Text = "NotePad Summary",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        headerPanel.Controls.Add(titleLabel);

        // Versie in header
        var versionLabel = new Label
        {
            Text = "Versie 1.0.0",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(220, 220, 220),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(22, 50)
        };
        headerPanel.Controls.Add(versionLabel);

        // Content panel
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25, 20, 25, 20)
        };

        // Hoofdtekst
        var mainText = new Label
        {
            Text = "Met liefde voor AAD gemaakt",
            Font = new Font("Segoe UI", 12, FontStyle.Italic),
            ForeColor = Color.FromArgb(60, 60, 60),
            AutoSize = false,
            Size = new Size(340, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 20)
        };

        // Bedrijfsnaam
        var companyText = new Label
        {
            Text = "WAM-Software",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 130, 180),
            AutoSize = false,
            Size = new Size(340, 35),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 55)
        };

        // Copyright
        var copyrightText = new Label
        {
            Text = "(c) since 1997",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(100, 100, 100),
            AutoSize = false,
            Size = new Size(340, 25),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 90)
        };

        // Claude CLI credit
        var claudeText = new Label
        {
            Text = "Powered by Claude CLI",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(130, 130, 130),
            AutoSize = false,
            Size = new Size(340, 25),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 120)
        };

        // OK knop
        var okButton = new Button
        {
            Text = "OK",
            Size = new Size(100, 35),
            Location = new Point(120, 160),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(70, 130, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand
        };
        okButton.FlatAppearance.BorderSize = 0;
        okButton.Click += (s, e) => Close();

        contentPanel.Controls.Add(mainText);
        contentPanel.Controls.Add(companyText);
        contentPanel.Controls.Add(copyrightText);
        contentPanel.Controls.Add(claudeText);
        contentPanel.Controls.Add(okButton);

        Controls.Add(contentPanel);
        Controls.Add(headerPanel);

        // ESC sluit venster
        KeyPreview = true;
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
                Close();
        };
    }
}
