# 📝 NotePad Summary

> AI-Powered Note Summarization Tool | System Tray Application | Powered by Codex CLI

Een handige Windows systray applicatie waarmee je snel notities kunt maken en automatisch laten samenvatten door AI.

## ✨ Features

- **Systray Applicatie** - Draait op de achtergrond met een handig geel notitie-icoontje
- **Globale Hotkey** - `Ctrl + NumPad+` opent direct het notitievenster
- **Tabbladen** - Meerdere notities tegelijk open, elk met datum/tijd
- **AI Samenvatting** - Zet rommelige notities om in bondige bullet points via Codex CLI
- **Auto Clipboard** - Samenvatting wordt automatisch naar klembord gekopieerd
- **Windows Startup** - Optioneel automatisch starten met Windows

## 🚀 Installatie

### Vereisten
- Windows 10/11
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Codex CLI](https://github.com/openai/codex) geïnstalleerd en geconfigureerd

### Bouwen
```bash
git clone https://github.com/wmostert76/NotePadSummary.git
cd NotePadSummary
dotnet build --configuration Release
```

### Uitvoeren
```bash
./bin/Release/net8.0-windows/NotePadSummary.exe
```

## 📖 Gebruik

| Actie | Sneltoets |
|-------|-----------|
| Notities openen | `Ctrl + NumPad+` |
| Nieuw tabblad | `Ctrl + T` of hotkey als venster open is |
| Tabblad sluiten | Rechtermuisklik op tab |
| Samenvatten | Klik "Samenvatten" of `Ctrl + Enter` |
| Venster verbergen | `Escape` |

## 🎨 Screenshots

De applicatie toont een gesplitst venster:
- **Boven**: Notities invoeren
- **Onder**: AI-gegenereerde samenvatting

## 📋 Samenvatting Stijl

De AI genereert bondige samenvattingen in bullet-point formaat:
```
- Telefoongesprek gevoerd met Jantje (Inkoop).
- Printprobleem gemeld, foutmelding driver not found.
- Driver opnieuw geïnstalleerd via fabrikant.
- Testpagina succesvol geprint.
- Ticket gesloten.
```

## 🛠️ Technologie

- C# / .NET 8.0
- Windows Forms
- Codex CLI voor AI samenvatting

---

**Met liefde voor AAD gemaakt door WAM-Software © since 1997 with Claude CLI**
