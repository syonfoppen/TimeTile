# ⏱️ Syon Time Dashboard

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MAUI Blazor](https://img.shields.io/badge/MAUI-Blazor%20Hybrid-0078D4?logo=blazor)](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Vibe Coded](https://img.shields.io/badge/Vibe%20Coded-%F0%9F%A4%99-blueviolet)](#-vibe-coded)

A Windows desktop dashboard for tracking time on [Azure DevOps](https://dev.azure.com/) work items with [7pace Timetracker](https://www.7pace.com/). Pin your tasks, start timers, and stay on top of your sprint — all from a single dark-themed window.

---

## 🤙 Vibe Coded

> **This entire app was vibe coded.** Every line — from the architecture to the CSS gradients — was built through conversations with [GitHub Copilot](https://github.com/features/copilot). No boilerplate was hand-written, no Stack Overflow was consulted. Just vibes.
>
> [Vibe coding](https://en.wikipedia.org/wiki/Vibe_coding) means describing what you want in plain language and letting AI generate the code. You guide it, review it, nudge it, and ship it. This repo is the real, unpolished result of that process — a working desktop app with auth, API integrations, state management, and a custom UI.
>
> Is every line perfect? No. Does it work? Yes. That's the vibe.

---

## ✨ Features

| Feature | Description |
|---|---|
| 📌 **Pinned Dashboard** | Pin Azure DevOps work items to a personal tile grid |
| ▶️ **Time Tracking** | Start/stop timers on work items via the 7pace API |
| 🟢 **Live Timer Bar** | Persistent bar showing the active session with real-time elapsed time |
| 🚀 **Pin My Sprint** | One click to bulk-pin all your Tasks from the current sprint |
| 🔍 **Work Item Search** | Search by ID or keyword with project/team/iteration filters |
| 🎛️ **Filter Panel** | Filter tiles by state, type, or assigned user |
| 🔄 **Auto Refresh** | Tile data refreshes from Azure DevOps every 60 seconds |
| 🌙 **Dark Theme** | Custom dark UI with purple/pink gradient accents |
| 💾 **Offline Storage** | Pinned tiles and settings persisted locally with LiteDB |

---

## 🏗️ Architecture

```
src/
├── Syon.TimeDashboard/                 # MAUI host — app shell, config, platform code
├── Syon.TimeDashboard.Core/            # Domain models, interfaces, enums
├── Syon.TimeDashboard.Infrastructure/  # API clients, auth, persistence, services
└── Syon.TimeDashboard.UI/              # Blazor components + Fluxor state stores
tests/
└── Syon.TimeDashboard.Tests/           # xUnit tests
```

### Tech Stack

| | Technology | Purpose |
|---|---|---|
| 🖼️ | **.NET MAUI Blazor Hybrid** | Desktop shell + Blazor UI |
| 🔄 | **[Fluxor](https://github.com/mrpmorris/Fluxor)** | Redux-style state management |
| 🔐 | **MSAL + WAM** | Azure AD / Entra ID authentication |
| ☁️ | **Azure DevOps REST API v7.1** | Work items via WIQL + batch |
| ⏱️ | **7pace REST API v3.2** | Time tracking (start, stop, log) |
| 🗄️ | **LiteDB** | Local embedded database |
| 🛡️ | **Polly** | HTTP retry/resilience |
| 🔑 | **DPAPI** | Secure token storage (Windows) |

---

## 📋 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MAUI workload — `dotnet workload install maui`
- Windows 10 version 1809+ (build 17763)
- An [Azure DevOps](https://dev.azure.com/) organization with [7pace Timetracker](https://www.7pace.com/)

---

## 🚀 Getting Started

### 1. Clone

```bash
git clone https://github.com/YOUR_USERNAME/Syon.TimeDashboard.git
cd Syon.TimeDashboard
```

### 2. Configure

Edit `src/Syon.TimeDashboard/appsettings.json`:

```json
{
  "AppSettings": {
    "AzureDevOpsOrgUrl": "https://dev.azure.com/YOUR_ORG",
    "SevenPaceApiBaseUrl": "https://YOUR_ORG.timehub.7pace.com",
    "EntraClientId": "YOUR_ENTRA_CLIENT_ID",
    "EntraTenantId": "YOUR_ENTRA_TENANT_ID"
  }
}
```

<details>
<summary><strong>Entra ID App Registration Setup</strong></summary>

You need a [Microsoft Entra ID app registration](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app):

1. Go to **Azure Portal** → **Entra ID** → **App registrations** → **New registration**
2. Set **Redirect URI** to `https://login.microsoftonline.com/common/oauth2/nativeclient` (Mobile and desktop)
3. Under **API permissions**, add **Azure DevOps** → `user_impersonation`
4. Copy the **Application (client) ID** and **Directory (tenant) ID** into `appsettings.json`

</details>

### 3. Build & Run

```bash
dotnet build
dotnet run --project src/Syon.TimeDashboard
```

### 4. Authenticate

1. Open **Settings** in the app
2. **Sign In with Microsoft** for Azure DevOps access
3. **Start Pairing** to connect 7pace Timetracker via PIN

---

## 🛠️ Development

```bash
# Build the full solution
dotnet build Syon.TimeDashboard.slnx

# Run tests
dotnet test

# Publish a self-contained executable
dotnet publish src/Syon.TimeDashboard -c Release -r win-x64 --self-contained -o ./publish
```

The `publish/` folder is a standalone app — zip it and share, no SDK needed on the target machine.

---

## 🤝 Contributing

Contributions, issues, and ideas are welcome!

1. Fork the repo
2. Create a branch (`git checkout -b feature/cool-thing`)
3. Commit your changes
4. Push and open a PR

---

## 📝 License

[MIT](LICENSE)
