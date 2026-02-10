using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotePadSummary;

public class NoteForm : Form
{
    private readonly TabControl _tabControl;
    private readonly Label _statusLabel;
    private readonly AppConfig _config;

    public NoteForm(AppConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // Form instellingen
        Text = "NotePad Summary";
        Size = new Size(900, 800);
        MinimumSize = new Size(650, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = CreateFormIcon();

        // Main layout
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            RowCount = 2,
            ColumnCount = 1
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Tab control
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Rechtermuisklik op tab sluit de tab
        _tabControl.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < _tabControl.TabCount; i++)
                {
                    if (_tabControl.GetTabRect(i).Contains(e.Location))
                    {
                        if (_tabControl.TabCount > 1)
                        {
                            _tabControl.TabPages.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        };

        // Status label
        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Height = 25,
            ForeColor = Color.DarkGreen,
            TextAlign = ContentAlignment.MiddleLeft
        };

        mainPanel.Controls.Add(_tabControl, 0, 0);
        mainPanel.Controls.Add(_statusLabel, 0, 1);

        Controls.Add(mainPanel);

        // Eerste tab aanmaken
        AddNewTab();

        // Keyboard shortcuts
        KeyPreview = true;
        KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.T)
            {
                AddNewTab();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.W && _tabControl.TabCount > 1)
            {
                CloseCurrentTab();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Hide();
                e.Handled = true;
            }
        };
    }

    public void AddNewTab()
    {
        var tabPage = new TabPage(DateTime.Now.ToString("dd-MM HH:mm"))
        {
            Padding = new Padding(5)
        };

        var tabPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        tabPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tabPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Split container voor notities en samenvatting
        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 8,
            Panel1MinSize = 100,
            Panel2MinSize = 80
        };

        // Notities panel (boven)
        var notesPanel = new Panel { Dock = DockStyle.Fill };
        var notesLabel = new Label
        {
            Text = "Notities:",
            Dock = DockStyle.Top,
            Height = 25,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };
        var notesTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10),
            AcceptsReturn = true,
            AcceptsTab = true,
            Name = "notesTextBox"
        };
        notesPanel.Controls.Add(notesTextBox);
        notesPanel.Controls.Add(notesLabel);

        // Samenvatting/SO panel (onder)
        var outputSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 8
        };

        var summaryPanel = new Panel { Dock = DockStyle.Fill };
        var summaryLabel = new Label
        {
            Text = "Samenvatting:",
            Dock = DockStyle.Top,
            Height = 25,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };
        var summaryTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10),
            ReadOnly = true,
            BackColor = Color.FromArgb(245, 245, 245),
            Name = "summaryTextBox"
        };
        summaryPanel.Controls.Add(summaryTextBox);
        summaryPanel.Controls.Add(summaryLabel);

        var soPanel = new Panel { Dock = DockStyle.Fill };
        var soLabel = new Label
        {
            Text = "SO tekst:",
            Dock = DockStyle.Top,
            Height = 25,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };
        var soTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10),
            ReadOnly = true,
            BackColor = Color.FromArgb(245, 245, 245),
            Name = "soTextBox"
        };
        soPanel.Controls.Add(soTextBox);
        soPanel.Controls.Add(soLabel);

        outputSplitContainer.Panel1.Controls.Add(summaryPanel);
        outputSplitContainer.Panel2.Controls.Add(soPanel);

        splitContainer.Panel1.Controls.Add(notesPanel);
        splitContainer.Panel2.Controls.Add(outputSplitContainer);

        // Knoppen panel
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 40,
            Padding = new Padding(0, 5, 0, 0)
        };

        var summarizeButton = new Button
        {
            Text = "Samenvatten",
            Size = new Size(120, 30),
            Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        summarizeButton.Click += async (s, e) => await OnSummarizeClick(notesTextBox, summaryTextBox, soTextBox, summarizeButton);

        var soButton = new Button
        {
            Text = "SO",
            Size = new Size(70, 30),
            Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        soButton.Click += async (s, e) => await OnSalesOpportunityClick(summaryTextBox, soTextBox, soButton);

        var clearButton = new Button
        {
            Text = "Wissen",
            Size = new Size(80, 30),
            Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        clearButton.Click += (s, e) =>
        {
            notesTextBox.Clear();
            summaryTextBox.Clear();
            soTextBox.Clear();
        };

        var copyButton = new Button
        {
            Text = "Kopieer notities",
            Size = new Size(110, 30),
            Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        copyButton.Click += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(notesTextBox.Text))
            {
                Clipboard.SetText(notesTextBox.Text);
                ShowStatus("Notities gekopieerd naar klembord!");
            }
        };

        buttonPanel.Controls.Add(summarizeButton);
        buttonPanel.Controls.Add(soButton);
        buttonPanel.Controls.Add(clearButton);
        buttonPanel.Controls.Add(copyButton);

        tabPanel.Controls.Add(splitContainer, 0, 0);
        tabPanel.Controls.Add(buttonPanel, 0, 1);

        // Zet split in het midden wanneer container resize
        splitContainer.SizeChanged += (s, e) =>
        {
            if (splitContainer.Height > splitContainer.Panel1MinSize + splitContainer.Panel2MinSize)
            {
                try
                {
                    splitContainer.SplitterDistance = splitContainer.Height / 2;
                }
                catch { }
            }
        };

        outputSplitContainer.SizeChanged += (s, e) =>
        {
            if (outputSplitContainer.Width > outputSplitContainer.Panel1MinSize + outputSplitContainer.Panel2MinSize)
            {
                try
                {
                    outputSplitContainer.SplitterDistance = outputSplitContainer.Width / 2;
                }
                catch { }
            }
        };

        tabPage.Controls.Add(tabPanel);
        _tabControl.TabPages.Add(tabPage);
        _tabControl.SelectedTab = tabPage;

        // Ctrl+Enter voor samenvatten in deze tab
        notesTextBox.KeyDown += async (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                await OnSummarizeClick(notesTextBox, summaryTextBox, soTextBox, summarizeButton);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };

        // Focus op notities
        notesTextBox.Focus();
    }

    private void CloseCurrentTab()
    {
        if (_tabControl.TabCount > 1)
        {
            // SelectedTab can be null in edge cases; guard to avoid null warnings.
            var selected = _tabControl.SelectedTab;
            if (selected != null)
                _tabControl.TabPages.Remove(selected);
        }
    }

    private Icon CreateFormIcon()
    {
        var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var bgBrush = new SolidBrush(Color.FromArgb(70, 130, 180));
            g.FillEllipse(bgBrush, 2, 2, 28, 28);
            using var pen = new Pen(Color.White, 2);
            g.DrawLine(pen, 8, 11, 24, 11);
            g.DrawLine(pen, 8, 16, 24, 16);
            g.DrawLine(pen, 8, 21, 18, 21);
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }

    private void ShowStatus(string message)
    {
        _statusLabel.Text = message;
        var timer = new System.Windows.Forms.Timer { Interval = 3000 };
        timer.Tick += (s, e) =>
        {
            _statusLabel.Text = "";
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    private async Task OnSummarizeClick(TextBox notesTextBox, TextBox summaryTextBox, TextBox soTextBox, Button summarizeButton)
    {
        var notes = notesTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(notes))
        {
            MessageBox.Show("Voer eerst wat notities in!", "Geen notities",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        summarizeButton.Enabled = false;
        summarizeButton.Text = "Bezig...";
        summaryTextBox.Text = "Even geduld, samenvatting wordt gemaakt...";
        soTextBox.Clear();

        try
        {
            var summary = await GenerateSummaryAsync(notes);
            summaryTextBox.Text = summary;

            // Kopieer naar klembord
            if (!string.IsNullOrWhiteSpace(summaryTextBox.Text))
            {
                Clipboard.SetText(summaryTextBox.Text);
                ShowStatus("Samenvatting gekopieerd naar klembord!");
            }
        }
        catch (Exception ex)
        {
            summaryTextBox.Text = $"Fout bij samenvatten: {ex.Message}";
            ShowStatus("Er ging iets mis bij het samenvatten.");
        }
        finally
        {
            summarizeButton.Enabled = true;
            summarizeButton.Text = "Samenvatten";
        }
    }

    private async Task OnSalesOpportunityClick(TextBox summaryTextBox, TextBox soTextBox, Button soButton)
    {
        var summary = summaryTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(summary))
        {
            MessageBox.Show("Maak eerst een samenvatting.", "Geen samenvatting",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        soButton.Enabled = false;
        soButton.Text = "Bezig...";
        soTextBox.Text = "Even geduld, SO-tekst wordt gemaakt...";

        try
        {
            var salesOpportunityText = await GenerateSalesOpportunityAsync(summary);
            soTextBox.Text = salesOpportunityText;

            Clipboard.SetText(salesOpportunityText);
            ShowStatus("SO-tekst gekopieerd naar klembord!");
        }
        catch (Exception ex)
        {
            soTextBox.Text = $"Fout bij SO maken: {ex.Message}";
            ShowStatus("Er ging iets mis bij het maken van SO.");
        }
        finally
        {
            soButton.Enabled = true;
            soButton.Text = "SO";
        }
    }

    private static string FindCodexPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "codex.cmd"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "codex"),
            @"C:\Program Files\nodejs\codex.cmd",
            "codex.cmd",
            "codex"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        return "codex";
    }

    private async Task<string> GenerateSummaryAsync(string notes)
    {
        var prompt = $"""
Maak een puntsgewijze samenvatting in het Nederlands.

Regels:
- Elke regel begint met een streepje (-).
- Gebruik korte, natuurlijke zinnen.
- Merge samenhangende notities in 1 regel als dat logisch is.
- Houd de tekst compact en zakelijk.
- Technische termen en namen behouden.
- Vermijd "ik" en "wij". Schrijf neutraal (bijv. "Gebruiker meldt...", "Printerdriver verwijderd...", "Printer opnieuw toegevoegd...").
- Schrijf altijd in tegenwoordige formulering (bijv. "Gebruiker meldt...", "Gebruiker gaat morgen...").

Stijlvoorbeeld:
- Contact gehad met Sharissa over problemen met de dongle van een ouder Microsoft-toetsenbord.
- Dongle verwijderd en pc herstart, maar het probleem blijft aanwezig.
- Dongle dichterbij geplaatst en de verbinding is iets stabieler.
- Gebruiker gaat morgen akkoord geven op vervanging van het toetsenbord.

Notities:
{notes}
""";

        var systemPrompt = AppConfigStore.GetEffectiveSystemPrompt(_config, AppConfigStore.PromptKind.Summary);
        return await ExecuteCodexPromptAsync(prompt, systemPrompt);
    }

    private async Task<string> GenerateSalesOpportunityAsync(string summary)
    {
        var prompt = $"""
Maak van onderstaande samenvatting een korte Sales Opportunity tekst voor intern gebruik bij de verkoopafdeling.

Regels:
- Maximaal 3 bullets.
- Elke regel begint met een streepje (-).
- Korte, amicale zinnen. Niet te formeel.
- Combineer waar mogelijk situatie + impact in 1 zin.
- Focus op praktisch advies met hardwarevervanging als logische stap bij oude of instabiele apparatuur.
- Niet verwijzen naar endpoint/monitoring services.
- Geen labels zoals "Situatie:", "Impact:", "Advies:" of "Vervolgstap:".
- Direct bruikbaar voor CRM.

Input samenvatting:
{summary}
""";

        var systemPrompt = AppConfigStore.GetEffectiveSystemPrompt(_config, AppConfigStore.PromptKind.SalesOpportunity);
        return await ExecuteCodexPromptAsync(prompt, systemPrompt);
    }

    private async Task<string> ExecuteCodexPromptAsync(string prompt, string systemPrompt)
    {
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            prompt = $"""
[SYSTEM]
{systemPrompt}

[USER]
{prompt}
""";
        }

        var escapedPrompt = prompt
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ");

        var codexPath = FindCodexPath();

        var startInfo = new ProcessStartInfo
        {
            FileName = codexPath,
            Arguments = $"exec --skip-git-repo-check \"{escapedPrompt}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Exception)
        {
            throw new Exception("Codex CLI niet gevonden. Zorg dat 'codex' in je PATH staat.");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
        {
            throw new Exception($"Codex fout: {error}");
        }

        if (string.IsNullOrWhiteSpace(output))
            return "Geen samenvatting gegenereerd.";

        // Strip ANSI escape codes
        var result = System.Text.RegularExpressions.Regex.Replace(output, @"\u001b\[[0-9;]*m", "");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\u001b\[[0-9;]*[A-Za-z]", "");

        result = result.Trim();
        var lines = result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        result = string.Join(Environment.NewLine, lines);

        return result;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnFormClosing(e);
    }
}
