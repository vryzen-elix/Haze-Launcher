using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WpfApp6.Services.Launch; // FakeAC
using PlooshLauncher;          // Proc & PSBasics

class Config
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Path2420 { get; set; } = "";
}

static class NativeConsole
{
    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();
}

class Program
{
    static readonly string configFile = "config.json";
    static Config config = new();

    static async Task Main()
    {
        NativeConsole.AllocConsole();
        Console.Title = "Haze Launcher";

        LoadConfig();

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== Haze Launcher ===");
            Console.ResetColor();

            Console.WriteLine($"Email: {(string.IsNullOrEmpty(config.Email) ? "<not set>" : config.Email)}");
            Console.WriteLine($"19.10 Path: {(string.IsNullOrEmpty(config.Path2420) ? "<not set>" : config.Path2420)}");
            Console.WriteLine();

            Console.WriteLine("1. Set Email & Password");
            Console.WriteLine("2. Set 19.10 Path");
            Console.WriteLine("3. Launch Fortnite");
            Console.WriteLine("0. Exit");
            Console.Write("Select option: ");

            switch (Console.ReadLine())
            {
                case "1": SetEmailPassword(); break;
                case "2": SetPath2420(); break;
                case "3": await LaunchFortniteWithInjection(); break;
                case "0": return;
                default:
                    Console.WriteLine("Invalid choice!");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static void SetEmailPassword()
    {
        Console.Write("Enter Email: ");
        config.Email = Console.ReadLine() ?? "";
        Console.Write("Enter Password: ");
        config.Password = Console.ReadLine() ?? "";
        SaveConfig();
    }

    static void SetPath2420()
    {
        Console.Write("Enter 19.10 Path: ");
        config.Path2420 = Console.ReadLine() ?? "";
        SaveConfig();
    }

    static void SaveConfig()
    {
        File.WriteAllText(configFile,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
    }

    static void LoadConfig()
    {
        if (!File.Exists(configFile)) return;
        try
        {
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile)) ?? new();
        }
        catch
        {
            config = new();
        }
    }

    // ---------------- LAUNCH FLOW ----------------

    static async Task LaunchFortniteWithInjection()
    {
        if (string.IsNullOrEmpty(config.Path2420) || !Directory.Exists(config.Path2420))
        {
            Console.WriteLine("Invalid 19.10 path!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Downloading required DLLs...");
        if (!await DownloadAllDlls())
        {
            Console.WriteLine("DLL download failed. Aborting launch.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Launching Fortnite...");

        try
        {
            PSBasics.Start(
                config.Path2420,
                "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be " +
                "-fltoken=h1cdhchd10150221h130eB56 -skippatchcheck " +
                "caldera=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50X2lkIjoiMTM5ZDAzOGFmOTM2NDcyODgxMTdlYWU3MWYxZGQ5ZTQiLCJnZW5lcmF0ZWQiOjE3MDQ0MTE5MDQsImNhbGRlcmFHdWlkIjoiODhjZmQ5NzYtM2U2OS00MWYzLWI2ODEtYzQyOTcxM2ZkMWFlIiwiYWNQcm92aWRlciI6IkVhc3lBbnRpQ2hlYXQiLCJub3RlcyI6IiIsImZhbGxiYWNrIjpmYWxzZX0.Q8hdxvrW2sH-3on6JEBLANB0rkPAGUwbZYPrCOMTtvA",
                config.Email,
                config.Password
            );

            Proc.Start(
                config.Path2420,
                "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping_EAC.exe",
                "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -nobe -fromfl=eac -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck",
                suspend: true
            );

            FakeAC.Start(
                config.Path2420,
                "FortniteLauncher.exe",
                "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck",
                "dsf"
            );

            Console.WriteLine("Fortnite launched successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Launch failed: {ex.Message}");
        }

        Console.ReadKey();
    }

 

    static async Task<bool> DownloadAllDlls()
    {
        try
        {
            await DownloadFile(
                "https://github.com/kazzydev123/dll-for-haze/raw/refs/heads/main/Starfall.dll",
                Path.Combine(config.Path2420,
                    "Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64\\GFSDK_Aftermath_Lib.x64.dll"));

            await DownloadFile(
                "https://github.com/kazzydev123/dll-for-haze/raw/refs/heads/main/Haze%20Arena.dll",
                Path.Combine(config.Path2420,
                    "FortniteGame\\Binaries\\Win64\\OGFNClient.dll"));

            await DownloadFile(
                "https://github.com/vryzen-elix/starfallabc/raw/refs/heads/main/Edit%20on%20Release%20&%20Reset%20on%20Release.dll",
                Path.Combine(config.Path2420,
                    "FortniteGame\\Binaries\\Win64\\EOR.dll"));

            return true;
        }
        catch
        {
            return false;
        }
    }

    static async Task DownloadFile(string url, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using WebClient wc = new();
        await wc.DownloadFileTaskAsync(url, destination);
    }
}
