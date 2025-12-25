# GoKart RFID Lap Counter System 🏎️🏁

Based on CF-815 UHF RFID Reader, this tool provides an automated lap counting solution for Go-Kart racing.

## 🌟 Features

*   **Automated Lap Counting**: High-precision lap counting with **5-second anti-duplicate** protection.
*   **Visual Dashboard**: 
    *   Full-screen (1080P) optimized UI.
    *   **Color-coded Status**:
        *   🟢 **Green**: Running
        *   🟡 **Yellow**: Last Lap (Lap 19/20)
        *   🔵 **Cyan**: Finished (Lap 20/20)
        *   🔴 **Red**: Inactive
*   **Audio Feedback**:
    *   🔊 Distinct beep for each valid lap.
    *   🎵 Special victory sound sequence upon completion.
*   **Live Logging**: Real-time color-coded event log (Raw Data / Laps / Errors).
*   **Session Control**: Manual "Start" and "Reset" capability for individual karts.

## 🛠️ Tech Stack

*   **Language**: C# (WinForms)
*   **Framework**: .NET Framework 3.5 (x86)
*   **Database**: SQLite (Local storage for lap times)
*   **Hardware SDK**: CF-815 UHF Reader 288 SDK

## 🚀 Getting Started

### Prerequisites
1.  **CF-815 RFID Reader** connected via USB-Serial (e.g., COM7).
2.  **Drivers**: Silicon Labs CP210x USB to UART Bridge.
3.  **.NET 3.5 Runtime** installed on Windows.

### Installation
1.  Clone this repository:
    ```bash
    git clone https://github.com/yuji4091/GoKartRFID.git
    ```
2.  Open `GoKartLapCounter.sln` in Visual Studio (2019/2022).
3.  Build solution (Target: **x86**).
4.  Run `GoKartLapCounter.exe`.

### Usage
1.  **Auto-Connect**: The system automatically searches for the reader on startup.
2.  **RFID Tags**: Tags starting with `AA01` to `AA20` (Hex) are automatically recognized.
3.  **Reset**: Select a kart and click "Reset Selection" to restart its session.

## 📝 Configuration

*   **COM Port**: Auto-detect (COM3 - COM10).
*   **Baud Rate**: 115200 / 57600 (Auto-negotiate).
*   **Anti-Duplicate**: Default 5 seconds (Editable in code `LapCounterEngine.cs`).

---

**Note**: Ensure `UHFReader288.dll` and `sqlite3.dll` are present in the output directory.

_Developed by Kawada & Antigravity AI_
