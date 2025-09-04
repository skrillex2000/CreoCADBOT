# 🤖 Creo CAD Assistant (AI-Powered with OpenAI + WinForms)

An AI-powered assistant for **Creo Parametric**, built using **OpenAI’s GPT models** and **WinForms**.  
This project enables engineers to interact with Creo assemblies using **natural language commands**, making CAD workflows more intuitive and productive.

---

## 🚀 Features
- **Conversational CAD Assistant** – Chat with Creo using natural language.
- **Creo Integration** – Execute CAD operations like:
  - Fetching child parts in assemblies
  - Adding and removing parts
  - Highlighting specific parts
  - Displaying structured tabular data
  - Fetching manufacturing company details
- **Function Calling with GPT** – Uses OpenAI function-calling (`ChatTool`) for safe and structured execution.
- **Custom Chat UI** – WinForms-based chat bubbles for user queries and AI responses, including table rendering.

---

## 🛠️ Tech Stack
- **Language:** C# (.NET / WinForms)
- **AI API:** [OpenAI .NET SDK](https://github.com/openai/openai-dotnet)
- **CAD API:** Creo Parametric Toolkit
- **UI:** WinForms (custom chat bubbles, DataGrid for tables)
- **Serialization:** Newtonsoft.Json + System.Text.Json

---

## 📂 Project Structure
CreoCADBOT/  
│── Form1.cs # Core application logic (AI + Creo function calling + UI handlers)  
│── Form1.Designer.cs # Auto-generated UI layout  
│── OutGoing.cs # Outgoing chat bubble  
│── Incomminglb.cs # Incoming chat bubble  
│── GlobalVariable.cs # Stores Creo child parts metadata  
│── Creo.cs # Wrapper for Creo Toolkit functions  
│── README.md # Documentation  

---

## ⚙️ Setup Instructions

### 1. Prerequisites
- Visual Studio 2022 or later
- .NET 6.0+ installed
- Creo Parametric installed with **TOOLKIT API enabled**
- OpenAI API Key (with GPT-4o or compatible model access)

### 2. Clone the Repository
```bash
git clone https://github.com/yourusername/creo-cad-bot.git
cd creo-cad-bot
