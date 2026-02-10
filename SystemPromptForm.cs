using System;
using System.Drawing;
using System.Windows.Forms;

namespace NotePadSummary;

internal sealed class SystemPromptForm : Form
{
    private readonly TextBox _textBox;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;
    private readonly Button _resetButton;

    public string SystemPromptText => _textBox.Text;

    public SystemPromptForm(string effectivePrompt, string defaultPrompt)
    {
        Text = "System prompt";
        Size = new Size(820, 560);
        MinimumSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterParent;

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 3,
            ColumnCount = 1
        };
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var info = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Pas hier de system prompt aan. Deze wordt opgeslagen in je Documents folder (OneDrive sync) en geldt voor samenvatten en SO."
        };

        _textBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10),
            AcceptsReturn = true,
            AcceptsTab = true,
            Text = effectivePrompt
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 40,
            Padding = new Padding(0, 6, 0, 0)
        };

        _saveButton = new Button
        {
            Text = "Opslaan",
            Size = new Size(110, 30),
            Margin = new Padding(0, 0, 10, 0),
            DialogResult = DialogResult.OK
        };

        _cancelButton = new Button
        {
            Text = "Annuleren",
            Size = new Size(110, 30),
            Margin = new Padding(0, 0, 10, 0),
            DialogResult = DialogResult.Cancel
        };

        _resetButton = new Button
        {
            Text = "Reset (default)",
            Size = new Size(140, 30),
            Margin = new Padding(0, 0, 10, 0)
        };
        _resetButton.Click += (_, _) => _textBox.Text = defaultPrompt;

        buttons.Controls.Add(_saveButton);
        buttons.Controls.Add(_cancelButton);
        buttons.Controls.Add(_resetButton);

        main.Controls.Add(info, 0, 0);
        main.Controls.Add(_textBox, 0, 1);
        main.Controls.Add(buttons, 0, 2);

        Controls.Add(main);

        AcceptButton = _saveButton;
        CancelButton = _cancelButton;
    }
}

