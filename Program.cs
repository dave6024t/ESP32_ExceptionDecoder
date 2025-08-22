using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Security.Cryptography;

namespace ESP32_ExceptionDecoder
{
    internal class Program
    {
        static string sha = "";
        static string build = "";
        static string elf = ".pio/build/{0}/firmware.elf";
        static string tools = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".platformio/packages/toolchain-xtensa-esp32");
        static string addr2linePath = "";
        static string addr2Line = "";
        static string traceDecode(string trace)
        {
            var p = new Process() { StartInfo = new ProcessStartInfo(addr2Line, "-e " + string.Format(elf, build) + " -f -p ESP32  " + trace ) { UseShellExecute = false, RedirectStandardOutput = true, /* RedirectStandardInput = false*/ } };
            p.Start();
            return p.StandardOutput.ReadToEnd();
        }
        // This functions returns the sha of the elf file
        static string shaOfElf(string elf)
        {
            try  
            {
                // compute the actual sha of a file
                using (SHA256 sha = SHA256.Create())
                {
                    using (var stream = File.OpenRead(elf))
                    {
                        var hash = sha.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        static void Write(char chr)
        {
            Write(chr.ToString());
        }
        static void Write(string message, params object[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message, args);


            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void WriteErrorLine(string message, params object[] args)
        {
            WriteError(message, args);
            Console.WriteLine();
        }
        static void WriteTraceLine(string message)
        {
            if (message == "")
                return;
            var fName = message.Substring(0, message.LastIndexOf(":"));
            var dir = Path.GetDirectoryName(fName);
            dir = dir?.TrimStart('?', '\r', '\n', ':', '0');

            var file = Path.GetFileName(fName);
            var line = message.Substring(message.LastIndexOf(":") + 1);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(dir);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("/");

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Write(file);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(":");

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(line.Trim());

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();


            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void WriteError(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write(message, args);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            if (args == null)
                args = new string[0];
            bool usePort = true;
            var comPort = SerialPort.GetPortNames().Last();
            int baud = 250000;
            var allTrace = "";
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i].StartsWith("-b") || args[i].StartsWith("--build"))
                {
                    build = args[i + 1];
                }
                else if (args[i].StartsWith("-e") || args[i].StartsWith("--elf"))
                {
                    elf = args[i + 1];
                }
                else if (args[i].StartsWith("-t") || args[i].StartsWith("--tools"))
                {
                    tools = args[i + 1];
                }
                else if (args[i].StartsWith("-a") || args[i].StartsWith("--addr2line"))
                {
                    addr2linePath = args[i + 1];
                }
                else if (args[i].StartsWith("-c") || args[i].StartsWith("--com"))
                {
                    try
                    {
                        if (bool.Parse(args[i + 1]))
                            usePort = false;
                        else
                            usePort = true;
                    }
                    catch
                    {
                        comPort = args[i + 1];
                    }
                }
                else if (args[i].StartsWith("-s") || args[i].StartsWith("--speed"))
                {
                    try
                    {
                        baud = int.Parse(args[i + 1]);
                    }
                    catch
                    {
                        baud = 115200;
                    }
                }
                else if (args[i].StartsWith("-f") || args[i].StartsWith("--file"))
                {
                    usePort = false;
                    allTrace = args[i + 1];
                }
            }

            if (build == "" && Directory.Exists(".pio/build"))
                build = Path.GetFileName(Directory.GetDirectories(".pio/build").ToList().Find(d => Directory.GetLastWriteTime(d) == Directory.GetDirectories(".pio/build").ToList().Max(d => Directory.GetLastWriteTime(d))));

            if (addr2linePath == "")
                addr2Line = Path.Combine(tools, "bin", "xtensa-esp32-elf-addr2line");
            else
                addr2Line = addr2linePath;

            // Summarize on console what args are we going to use:
            sha = shaOfElf(string.Format(elf, build));
            if (sha == "")
            {
                WriteErrorLine("Could not find the ELF file. Please check the path.");
                return;
            }
            Console.WriteLine("Using Elf: {0}", string.Format(elf, build));
            Console.WriteLine("SHA of elf: {0}", sha);
            Console.WriteLine("xtensa tools: {0}", tools);
            Console.WriteLine("add2line util: {0}", addr2Line);
            if (usePort)
                Console.WriteLine("COM Port: {0} @{1}", comPort, baud);
            else
                WriteErrorLine("Decoding Error: {0}", allTrace);

            if (args.Length % 2 == 1)
            {
                usePort = false;
                if (File.Exists(args.Last()))
                    allTrace = File.ReadAllText(allTrace);
                else
                    allTrace = args.Last();
            }
            Console.WriteLine("ESP32 Exception Decoder");
            var sp = new SerialPort(comPort, baud);
            List<char> line = new();
            int restartCount = 0;
            bool stopPrinting = false;
            void newLine(string line)
            {
                if (line.StartsWith("ELF file SHA256"))
                {
                    if (!stopPrinting)
                    {
                        var trueSha = line.Split(":")[1].Trim();
                        if (!sha.StartsWith(trueSha))
                        {
                            WriteErrorLine("SHA of the source ELF file does not match with the current one. Are you sure you are running the latest firmware?");
                        }
                    }
                    restartCount++;
                }
                else if (stopPrinting)
                    return;
                if (line.StartsWith("PC"))
                {
                    var pc = line.Split(new char[] { '\t', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    Console.WriteLine();
                    WriteErrorLine("Exception occured at: ");
                    WriteTraceLine(traceDecode(pc).Trim());
                }
                else if (line.StartsWith("Guru Meditation"))
                {
                    Console.WriteLine();
                    line = line.Substring(line.IndexOf("(") + 1);
                    line = line.Substring(0, line.IndexOf(")")).Trim();
                    WriteErrorLine(line);
                }
                else if (line.StartsWith("Backtrace: "))
                {
                    Console.WriteLine();
                    WriteErrorLine("Decoded Backtrace: ");
                    var traces = line.Split(' ').ToList().Skip(1);
                    foreach (var trace in traces)
                    {
                        var decode = traceDecode(trace).Trim();
                        WriteTraceLine(decode);
                    }

                }
                if (restartCount >= 1)
                {
                    if (restartCount == 1)
                        Console.WriteLine();
                    stopPrinting = true;
                    WriteError("\rBoot loop detected: {0}", restartCount);
                    return;
                }
            }
            sp.DataReceived += (sender, e) =>
            {
                while (sp.BytesToRead > 0)
                {
                    var bytes = new byte[sp.BytesToRead];
                    sp.Read(bytes, 0, bytes.Length);
                    foreach (var bRead in bytes)
                    {
                        if (bRead == '\n')
                        {
                            var l = string.Join("", line);
                            line.Clear();
                            newLine(l);
                        }
                        else
                            line.Add((char)bRead);
                        if (!stopPrinting)
                            Write((char)bRead);
                    }
                }
            };
            if (usePort)
            {
                try
                {
                    Console.WriteLine("Opening {0} @ {1}", comPort, baud);
                    sp.Open();
                    Console.WriteLine("Opened successfully");
                }
                catch
                {
                    Console.WriteLine("Could not open the com port");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Using backtrace inf: {0}", allTrace);
                foreach (var l in allTrace.Split('\r', 'n'))
                {
                    newLine(l);
                }
            }
            if (usePort)
                while (true)
                {
                    var inStream = Console.OpenStandardInput();
                    StreamReader sr = new(inStream);
                    var rLine = sr.ReadLine();
                    sp.WriteLine(rLine);
                    rLine = sr.ReadLine();
                }
            else
                Console.WriteLine("Docoding ended.");
        }

    }
}
