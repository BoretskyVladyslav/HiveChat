# HiveChat 🐝

Сучасний MVP-месенджер реального часу, побудований на C# (.NET 8). Проєкт демонструє клієнт-серверну архітектуру з використанням SignalR для двостороннього зв'язку та SQLite для збереження історії повідомлень.

---

## Технологічний стек

### Backend
- **ASP.NET Core 8** — мінімальний веб-сервер
- **SignalR** — двосторонній зв'язок у реальному часі через WebSocket
- **Entity Framework Core** — ORM для роботи з базою даних
- **SQLite** — легковісна вбудована база даних

### Frontend
- **WPF (Windows Presentation Foundation)** — нативний десктоп-клієнт під Windows
- **SignalR Client** — бібліотека для підключення до хабу

---

## Основний функціонал

- 🔐 Підключення за унікальним нікнеймом
- ⚡ Миттєвий обмін повідомленнями в реальному часі (бродкаст через SignalR)
- 💾 Автоматичне збереження історії чату в локальну базу даних SQLite
- 📜 Автоматичне підтягування останніх повідомлень при вході в чат
- 🔒 Валідація та блокування інтерфейсу до моменту підключення
- 🔄 Автоматичне перепідключення при обриві зв'язку

---

## Архітектура

```
┌──────────────────┐         ┌──────────────────────────┐         ┌─────────────┐
│   WPF Клієнт 1   │◄──────►│                          │         │             │
│  (SignalR Client) │  WS    │   ASP.NET Core Сервер    │  EF     │   SQLite    │
├──────────────────┤◄──────►│      SignalR Hub          │◄──────►│   chat.db   │
│   WPF Клієнт 2   │  WS    │     /chat endpoint       │  Core   │             │
│  (SignalR Client) │         │                          │         │             │
├──────────────────┤         └──────────────────────────┘         └─────────────┘
│   WPF Клієнт N   │◄──────►          ▲
│  (SignalR Client) │  WS              │
└──────────────────┘           CORS (Allow Any)
```

**Потік повідомлення:**
1. Клієнт відправляє повідомлення через `InvokeAsync("SendMessage", username, text)`
2. Сервер отримує виклик у `ChatHub.SendMessage()`
3. Повідомлення зберігається в SQLite через `ChatDbContext`
4. Сервер розсилає бродкаст усім клієнтам через `Clients.All.SendAsync("ReceiveMessage", ...)`
5. Кожен клієнт отримує повідомлення та додає його в UI через `Dispatcher.Invoke`

---

## Структура проєкту

```
HiveChat/
├── HiveChat.Server/
│   ├── Models/
│   │   └── Message.cs
│   ├── Data/
│   │   └── ChatDbContext.cs
│   ├── Hubs/
│   │   └── ChatHub.cs
│   ├── Program.cs
│   └── HiveChat.Server.csproj
│
├── HiveChat.Client/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── HiveChat.Client.csproj
│
└── README.md
```

---

## Інструкція із запуску

### Попередні вимоги

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 або 11

### Крок 1 — Клонування

```powershell
git clone https://github.com/BoretskyVladyslav/HiveChat.git
cd HiveChat
```

### Крок 2 — Запуск сервера

Відкрийте **перший термінал**:

```powershell
cd HiveChat.Server
dotnet run
```

Дочекайтеся повідомлення:
```
Now listening on: http://localhost:5284
```

### Крок 3 — Запуск першого клієнта

Відкрийте **другий термінал**:

```powershell
cd HiveChat.Client
dotnet run
```

1. Введіть нікнейм (наприклад, `Vlad`)
2. Натисніть **Connect**
3. Побачите `Connected to server.`

### Крок 4 — Запуск другого клієнта

Відкрийте **третій термінал**:

```powershell
cd HiveChat.Client
dotnet run
```

1. Введіть інший нікнейм (наприклад, `Demian`)
2. Натисніть **Connect**
3. Відправляйте повідомлення — вони з'являться в обох вікнах одночасно

---

## Автори

- **Борецький Владислав** — Backend (ASP.NET Core, SignalR, SQLite)
- **Дем'ян** — Frontend (WPF Client)
