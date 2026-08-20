# HoneyKhaalaProject

A Windows desktop application built with **C# and WPF** for managing investor contributions, business investments, and monthly profit distribution.

The application allows multiple investors to contribute different amounts to multiple businesses, calculates each investor's investment percentage and profit share, and stores monthly records locally in JSON files.

## 🚀 Features

* 👥 Add and remove investors
* 💰 Track each investor's contribution to individual businesses
* 🏢 Manage multiple businesses
* 📊 Calculate investment percentages
* 💵 Calculate and distribute business profits among investors
* 📅 Manage data by month
* 💾 Save monthly records as JSON files
* 📂 Load previously saved monthly records
* 🔄 Automatically refresh the month selector
* 🖥️ Windows desktop interface built with WPF
* 🧩 Uses MVVM concepts for separating application data and logic

## 🛠️ Technologies Used

* **C#**
* **.NET 9**
* **WPF (Windows Presentation Foundation)**
* **XAML**
* **CommunityToolkit.Mvvm**
* **JSON / System.Text.Json**
* **Git & GitHub**

## 🏗️ Project Structure

```text
HoneyKhaalaProject/
│
├── HoneyKhaalaProject/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── CalculationWindow.xaml
│   ├── CalculationWindow.xaml.cs
│   └── ...
│
├── HoneyKhaalaProject.VM/
│   ├── MainVM.cs
│   ├── Investor.cs
│   ├── Contribution.cs
│   ├── BusinessEntry.cs
│   └── MonthData.cs
│
└── HoneyKhaalaProject.sln
```

### Architecture

The project separates the WPF interface from data and ViewModel classes:

```text
WPF Interface
      ↓
Calculation Window
      ↓
ViewModels / Models
      ↓
Investment & Profit Calculations
      ↓
JSON Local Storage
```

## 📈 How Profit Distribution Works

Each business has its own total investment and profit.

For each investor:

```text
Investor's Business Contribution
              ÷
Total Business Contributions
              ×
Business Profit
              =
Investor's Profit Share
```

The application then combines the investor's profit shares from all businesses to calculate their overall monthly profit.

## 💾 Data Storage

Monthly data is stored locally as JSON files in the user's application data directory:

```text
%AppData%\HoneyKhaalaProject\Months
```

Each month's data is stored separately using a format such as:

```text
2026-01.json
2026-02.json
2026-03.json
```

This allows previous months to be opened and edited without affecting other records.

## ▶️ How to Run

### Requirements

* Windows
* .NET 9 SDK
* Visual Studio 2022 or later with WPF/.NET development support

### Steps

1. Clone the repository:

```bash
git clone https://github.com/Syed-Abdullaah/HoneyKhaalaProject.git
```

2. Open:

```text
HoneyKhaalaProject.sln
```

3. Restore the NuGet packages.

4. Build the solution.

5. Run the application.

## 📸 Screenshots

Screenshots of the application's interface can be added here.

### Month Selection

*Add screenshot here.*

### Investment & Profit Calculation

*Add screenshot here.*

## 🧠 What I Learned

This project was built as a practical C# application and helped me develop experience with:

* C# application development
* WPF and XAML
* MVVM concepts
* Data binding
* Observable collections
* Property change notifications
* JSON serialization and deserialization
* File handling
* Investment and profit calculations
* Organizing a multi-project solution
* Git and GitHub version control

## 🔮 Future Improvements

Possible improvements include:

* [ ] Move more business logic from code-behind into View Models
* [ ] Add charts and visual analytics
* [ ] Add stronger input validation
* [ ] Add database support
* [ ] Improve error handling
* [ ] Add export to Excel/PDF
* [ ] Add authentication and user accounts
* [ ] Improve UI responsiveness and accessibility

## 📌 Project Status

**Completed learning project**

This project represents an earlier stage of my C# development journey and is part of my growing software development portfolio.

---

**Author:** Syed-Abdullah

**GitHub:** https://github.com/Syed-Abdullaah
