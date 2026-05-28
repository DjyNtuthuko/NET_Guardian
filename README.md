# NET Guardian

<p align="center">
  <img src="logo.png" alt="NET Guardian Logo" width="160"/>
</p>

<p align="center">
  A cybersecurity awareness chatbot built with C# and WPF
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows-0078D4?style=flat-square"/>
  <img src="https://img.shields.io/badge/language-C%23-239120?style=flat-square"/>
  <img src="https://img.shields.io/badge/framework-.NET%20WPF-512BD4?style=flat-square"/>
  <img src="https://img.shields.io/badge/purpose-Educational-orange?style=flat-square"/>
</p>

---

## Overview

NET Guardian is a desktop application that educates users on common cybersecurity threats through interactive conversation. Built with C# and WPF, it simulates a knowledgeable security advisor — responsive, adaptive, and easy to talk to.

Topics covered include password safety, phishing, malware, online scams, privacy protection, safe browsing, two-factor authentication, and social engineering.

---

## Features

**Conversation**
- Personalized greetings using the user's name
- Keyboard-friendly input with Enter key support
- Chat history tracking and clear chat functionality

**Cybersecurity Knowledge**
- Multi-topic educational content with randomized responses
- Keyword-based topic detection with typo tolerance
- Context-aware follow-up handling

**Smart Behavior**
- Sentiment detection — recognizes worried, confused, curious, frustrated, happy, and angry states and adjusts tone accordingly
- Persistent user memory saved to local storage — remembers your name and favourite topic across sessions
- Fuzzy keyword matching to handle common spelling mistakes

**Interface**
- Modern WPF UI with styled message bubbles
- Embedded audio greeting on launch

---

## How It Works

**1. Startup**

On launch, NET Guardian plays an audio greeting, welcomes the user, and asks for their name before beginning the conversation.

**2. Conversation Flow**

The chatbot detects cybersecurity topics through keyword matching, handles common spelling mistakes automatically, and generates responses using a delegate-based system. It tracks the last discussed topic to provide contextual follow-ups.

**3. User Memory**

The application stores the user's name and favourite topic in a local text file, allowing it to personalize responses across sessions.

**4. Sentiment Detection**

The chatbot reads emotional cues in user input and adjusts its responses accordingly — offering reassurance to worried users, clarity to confused ones, and patience to frustrated ones.

---

## Topic Highlights

| Topic | What You'll Learn |
|---|---|
| Password Safety | Strong password creation, password managers, avoiding reuse |
| Phishing | Spotting fake emails, suspicious links, and common scam patterns |
| Malware | Viruses, ransomware, and safe downloading habits |
| Two-Factor Auth | How 2FA works and why it matters |
| Social Engineering | Manipulation tactics and how to recognize them |
| Privacy | Controlling your digital footprint online |

---

## Project Structure

```
NET_Guardian/
│
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── responses.cs
├── audio.cs
├── logo.png
├── NetGuardianAudio.wav
├── users.txt
│
├── Properties/
├── bin/
└── obj/
```

---

## Getting Started

**Requirements**

- Visual Studio 2022 or newer
- .NET Desktop Development workload installed

**Setup**

```bash
git clone https://github.com/DjyNtuthuko/NET_Guardian.git
```

1. Open the solution in Visual Studio
2. Build the project (`Ctrl + Shift + B`)
3. Run the application (`F5`)

---

## Technologies

| Technology | Role |
|---|---|
| C# | Core application logic |
| .NET Framework | Runtime environment |
| WPF / XAML | Desktop UI and layout |
| Visual Studio | Development environment |

---

## Educational Purpose

This project was built to demonstrate practical software development concepts including object-oriented programming, delegate usage, file I/O, event-driven programming, WPF UI development, sentiment analysis logic, and user interaction design.

---

## Roadmap

Possible future improvements:

- Database integration for user profiles
- Voice recognition input
- AI-generated responses
- User authentication
- Dark mode toggle
- Expanded topic library
- Exportable chat history

---

## Author

Developed by **Thando Mike Ndaba**

GitHub: [DjyNtuthuko/NET_Guardian](https://github.com/DjyNtuthuko/NET_Guardian)
