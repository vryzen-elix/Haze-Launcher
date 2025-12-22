using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WpfApp6.Services.Launch
{
	public static class PSBasics
	{
		public static Process FortniteProcess;

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr OpenProcess(int access, bool inherit, int pid);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint size, uint allocType, uint protect);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer, uint size, out IntPtr written);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr attr, uint stack, IntPtr start, IntPtr param, uint flags, IntPtr id);

		[DllImport("kernel32.dll")]
		static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll")]
		static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		public static bool InjectDLL(int pid, string dllPath)
		{
			try
			{
				if (!File.Exists(dllPath))
				{
					Debug.WriteLine("DLL not found: " + dllPath);
					return false;
				}

				IntPtr proc = OpenProcess(0x1F0FFF, false, pid);
				if (proc == IntPtr.Zero)
				{
					Debug.WriteLine("OpenProcess failed.");
					return false;
				}

				byte[] dllBytes = System.Text.Encoding.Unicode.GetBytes(dllPath + "\0");

				IntPtr addr = VirtualAllocEx(proc, IntPtr.Zero, (uint)dllBytes.Length, 0x3000, 0x40);
				if (addr == IntPtr.Zero)
				{
					Debug.WriteLine("VirtualAllocEx failed.");
					return false;
				}

				bool wrote = WriteProcessMemory(proc, addr, dllBytes, (uint)dllBytes.Length, out _);
				if (!wrote)
				{
					Debug.WriteLine("WriteProcessMemory failed.");
					return false;
				}

				IntPtr loadLib = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
				if (loadLib == IntPtr.Zero)
				{
					Debug.WriteLine("LoadLibraryW not found.");
					return false;
				}

				IntPtr thread = CreateRemoteThread(proc, IntPtr.Zero, 0, loadLib, addr, 0, IntPtr.Zero);
				if (thread == IntPtr.Zero)
				{
					Debug.WriteLine("CreateRemoteThread failed.");
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Injection error: " + ex.Message);
				return false;
			}
		}
		public static async void Start(string path, string args, string email, string password)
		{
			string exe = Path.Combine(path, "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe");

			FortniteProcess = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = exe,
					Arguments = $"-AUTH_LOGIN={email} -AUTH_PASSWORD={password} -AUTH_TYPE=epic {args}",
					WorkingDirectory = Path.GetDirectoryName(exe),
					UseShellExecute = false,
					CreateNoWindow = false,
					WindowStyle = ProcessWindowStyle.Normal
				}
			};

			FortniteProcess.Start();
 
			while (FortniteProcess.MainWindowHandle == IntPtr.Zero && !FortniteProcess.HasExited)
				await Task.Delay(250);

 
 
			await Task.Delay(20000);  

 
            string dll = Path.Combine(path, "FortniteGame\\Binaries\\Win64\\OGFNClient.dll");
            string ROR = Path.Combine(path, "FortniteGame\\Binaries\\Win64\\EOR.dll");


            if (File.Exists(dll))
            {
                InjectDLL(FortniteProcess.Id, dll);
                await Task.Delay(1500); // small delay = stability
            }

            if (File.Exists(ROR))
            {
                InjectDLL(FortniteProcess.Id, ROR);
            }
        }


	}
}
