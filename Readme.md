Thanks for your feedback! Here's the updated **README.md** with the corrected **usage instructions**, including the step to copy the binaries to a directory in your PlatformIO project, the proper **command line execution**, and an explanation of how to navigate to decoded lines in **VSCode**.

---

# ESP32 Exception Decoder

## Overview

This tool is a **custom exception decoder** for **ESP32** devices, designed to decode **exception backtraces** and **register dumps**. It automatically maps memory addresses from the exception to the corresponding **source code lines** using the **`addr2line`** utility included in the **ESP32 toolchain**. This tool is especially useful for those who cannot get the **internal exception decoder** of **PlatformIO** to work properly.

### **Key Features:**
- **Automatic build detection**: Detects the latest build of the firmware from PlatformIO’s `.pio/build` directory.
- **Serial terminal support**: Works as a **serial terminal** and listens to the serial port to decode exception backtraces in real-time.
- **File support**: Reads exception backtraces from an input file.
- **Boot loop prevention**: Detects if the system is in a **boot loop** and halts further decoding.
- **Exception decoding**: Uses the **`addr2line`** utility from the **ESP32 toolchain** to decode exception data and map memory addresses to the corresponding source code.

## Installation

### Clone the repository:

1. Clone this repository to your local machine:
   ```bash
   git clone https://github.com/umartechboy/ESP32_ExceptionDecoder.git
   ```

2. Navigate to the directory where the repository is cloned:
   ```bash
   cd ESP32-ExceptionDecoder
   ```

### Build the project:

Make sure that **.NET SDK** is installed. You can then **build the project** using **.NET Core**:

```bash
dotnet build
```

This will build the project and prepare it for execution.

---

## Usage

### **Step 1: Copy Binaries to Your PlatformIO Project Directory**

After building the project, copy the generated **`ESP32_ExceptionDecoder`** binary to your PlatformIO project directory.

For example, you can create a directory named **`eDecoder`** in your PlatformIO project folder and copy the binary there. For Windows, copy all files from your build directory to **`eDecoder`**.

### **Step 2: Execute the Tool**

#### **On Windows**:
In **Command Prompt** or **PowerShell**, navigate to your PlatformIO project directory and execute the tool with the following command:

```bash
.\eDecoder\ESP32_ExceptionDecoder
```

#### **On Linux/Mac**:
On **Linux** or **Mac**, you can execute the tool with **`dotnet`** by running:

```bash
dotnet .\eDecoder\ESP32_ExceptionDecoder.dll
```

This will:
- Automatically detect the latest build from **`.pio/build`**.
- Automatically detect the serial port and baud rate for real-time decoding (default is `COM3` and `250000` baud).
- Begin decoding exception data from the serial port or an input file.

---

### **Command-Line Arguments**

You can also pass custom arguments to the tool if needed:

- **`--build` or `-b`**: Specify the build directory or target build.
- **`--elf` or `-e`**: Specify the **ELF file**.
- **`--tools` or `-t`**: Specify the path to the **ESP32 toolchain** (default is detected automatically).
- **`--addr2line` or `-a`**: Specify the full path & name of your **addr2line utility** 
- **`--com` or `-c`**: Specify the **COM port** for serial communication.
- **`--speed` or `-s`**: Specify the **baud rate** for serial communication (default is `250000`).
- **`--file` or `-f`**: Use a **file** as input for backtrace logs instead of serial.

#### Example Command with Custom Arguments:

```bash
.\eDecoder\ESP32_ExceptionDecoder.exe --build "RAMPS" --elf "C:/path/to/firmware.elf" --com "COM3" --speed 115200
```

This will:
- Use the firmware from the **`D8500`** build folder.
- Read exceptions from **COM3** with a baud rate of `115200`.

### **File Mode**

If you have an **exception file** (e.g., `exception.txt`), you can provide it as input to decode the backtrace:

```bash
.\eDecoder\ESP32_ExceptionDecoder.exe --file "exception.txt"
```

This will:
- Decode the contents of `exception.txt` and display the decoded backtrace information.

---

## Example Output

![Screenshot](https://raw.githubusercontent.com/umartechboy/ESP32_ExceptionDecoder/refs/heads/main/Screenshot.png)

![Screenshot](https://raw.githubusercontent.com/umartechboy/ESP32_ExceptionDecoder/refs/heads/main/Screenshot%202.png)

When decoding, the tool outputs information such as the **exception type**, **register dumps**, and **backtrace** with **decoded file names and line numbers**.

The tool also uses **command-line styling**, like colored output, to make it easier to read. You can click on the decoded lines in **VSCode**, and it will navigate directly to the corresponding line in the source code.

**Example output in the terminal:**

```

You can click on the decoded line, and **VSCode** will open the corresponding file and line number.

---

## Boot Loop Prevention

This tool automatically detects if your ESP32 device is stuck in a **boot loop**. If multiple restarts are detected in succession, the tool will stop printing further lines and display a **boot loop detected** message, preventing unnecessary logs.

---

## Contribution

Feel free to **fork** this repository and make contributions via **pull requests**. Contributions, bug fixes, and feature requests are always welcome.

---

Let me know if this version is more aligned with your expectations or if any additional changes are needed!