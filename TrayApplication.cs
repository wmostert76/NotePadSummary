using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NotePadSummary;

public class TrayApplication : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _startupMenuItem;
    private NoteForm? _noteForm;
    private bool _disposed;

    private const string AppName = "NotePadSummary";
    private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    // Windows API voor globale hotkey
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 1;
    private const uint MOD_CONTROL = 0x0002;
    private const uint VK_ADD = 0x6B; // NumPad Plus

    private readonly HotkeyWindow _hotkeyWindow;

    public TrayApplication()
    {
        // Maak context menu
        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("Notities openen", null, OnOpenNotes);
        _contextMenu.Items.Add("-");

        // Starten met Windows checkbox
        _startupMenuItem = new ToolStripMenuItem("Starten met Windows")
        {
            CheckOnClick = true,
            Checked = IsStartupEnabled()
        };
        _startupMenuItem.CheckedChanged += OnStartupChanged;
        _contextMenu.Items.Add(_startupMenuItem);

        _contextMenu.Items.Add("-");
        _contextMenu.Items.Add("Over...", null, OnAbout);
        _contextMenu.Items.Add("-");
        _contextMenu.Items.Add("Afsluiten", null, OnExit);

        // Maak systray icon
        _trayIcon = new NotifyIcon
        {
            Icon = CreateNoteIcon(),
            Visible = true,
            Text = "NotePad Summary (Ctrl+NumPad+)",
            ContextMenuStrip = _contextMenu
        };

        _trayIcon.DoubleClick += OnOpenNotes;

        // Registreer globale hotkey via hidden window
        _hotkeyWindow = new HotkeyWindow(OnHotkeyPressed);
        var registered = RegisterHotKey(_hotkeyWindow.Handle, HOTKEY_ID, MOD_CONTROL, VK_ADD);

        if (registered)
        {
            ShowBalloon("NotePad Summary", "Actief! Druk Ctrl+NumPad+ om notities te openen.");
        }
        else
        {
            ShowBalloon("NotePad Summary", "Hotkey kon niet worden geregistreerd. Dubbelklik op het icoon om te openen.");
        }
    }

    private bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    private void SetStartupEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
            if (key == null) return;

            if (enabled)
            {
                var exePath = Application.ExecutablePath;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kon startup instelling niet wijzigen: {ex.Message}", "Fout",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnStartupChanged(object? sender, EventArgs e)
    {
        SetStartupEnabled(_startupMenuItem.Checked);
    }

    private Icon CreateNoteIcon()
    {
        // Maak een simpel notitie-icoontje programmatisch
        var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Achtergrond (geel post-it)
            using var bgBrush = new SolidBrush(Color.FromArgb(255, 242, 100));
            g.FillRectangle(bgBrush, 2, 2, 28, 28);

            // Schaduw
            using var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
            g.FillRectangle(shadowBrush, 4, 28, 28, 2);
            g.FillRectangle(shadowBrush, 28, 4, 2, 26);

            // Lijntjes op de notitie
            using var pen = new Pen(Color.FromArgb(180, 150, 50), 2);
            g.DrawLine(pen, 6, 10, 26, 10);
            g.DrawLine(pen, 6, 16, 26, 16);
            g.DrawLine(pen, 6, 22, 20, 22);

            // Rand
            using var borderPen = new Pen(Color.FromArgb(200, 180, 60), 1);
            g.DrawRectangle(borderPen, 2, 2, 27, 27);
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }

    private void ShowBalloon(string title, string text)
    {
        _trayIcon.BalloonTipTitle = title;
        _trayIcon.BalloonTipText = text;
        _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(2000);
    }

    private void OnHotkeyPressed()
    {
        OpenOrAddTab();
    }

    private void OnOpenNotes(object? sender, EventArgs e)
    {
        OpenOrAddTab();
    }

    private void OpenOrAddTab()
    {
        if (_noteForm == null || _noteForm.IsDisposed)
        {
            _noteForm = new NoteForm();
            _noteForm.Show();
        }
        else if (_noteForm.Visible)
        {
            // Venster is al open, maak nieuw tabblad
            _noteForm.AddNewTab();
        }
        else
        {
            _noteForm.Show();
        }

        _noteForm.BringToFront();
        _noteForm.Activate();
    }

    private void OnAbout(object? sender, EventArgs e)
    {
        using var aboutForm = new AboutForm();
        aboutForm.ShowDialog();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        UnregisterHotKey(_hotkeyWindow.Handle, HOTKEY_ID);
        _trayIcon.Visible = false;
        Application.Exit();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        UnregisterHotKey(_hotkeyWindow.Handle, HOTKEY_ID);
        _hotkeyWindow.Dispose();
        _noteForm?.Dispose();
        _trayIcon.Dispose();
        _contextMenu.Dispose();
    }
}

// Hidden window om WM_HOTKEY berichten te ontvangen
internal class HotkeyWindow : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private readonly Action _onHotkey;

    public HotkeyWindow(Action onHotkey)
    {
        _onHotkey = onHotkey;
        CreateHandle(new CreateParams());
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            _onHotkey();
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        DestroyHandle();
    }
}
