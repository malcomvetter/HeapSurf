using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace HeapSurf
{
    public class HeapSurfer
    {
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int MEM_COMMIT = 0x00001000;
        const int PAGE_READWRITE = 0x04;
        const int PROCESS_WM_READ = 0x0010;

        private string _configFile;

        public HeapSurfer()
        { }

        public HeapSurfer(string file)
        {
            _configFile = file;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        struct MEMORY_BASIC_INFORMATION
        {
            public int BaseAddress;
            public int AllocationBase;
            public int AllocationProtect;
            public int RegionSize;
            public int State;
            public int Protect;
            public int lType;
        }

        struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        public void Surf()
        {
            var config = Config.Load(_configFile);
            if (config == null)
            {
                config = Config.Default;
            }
            Console.WriteLine("...Config...\n{0}\n", config.ToString());
            foreach (var process in config.Processes)
            {
                Surf(process);
            }
        }

        public void Surf(string processName)
        {
            //var processes = Process.GetProcesses();
            foreach (var process in Process.GetProcessesByName(processName))
            {
                SearchProcessMemory(process);
            }
        }

        void SearchProcessMemory(Process process)
        {
            // getting minimum & maximum address
            var sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);
            var proc_min_address = sys_info.minimumApplicationAddress;
            var proc_max_address = sys_info.maximumApplicationAddress;
            var proc_min_address_l = (long)proc_min_address;
            var proc_max_address_l = (long)proc_max_address;

            //Opening the process with desired access level
            var processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, process.Id);
            var mem_basic_info = new MEMORY_BASIC_INFORMATION();
            var bytesRead = 0;  // number of bytes read with ReadProcessMemory

            while (proc_min_address_l < proc_max_address_l)
            {
                VirtualQueryEx(processHandle, proc_min_address, out mem_basic_info, 28); //28 = sizeof(MEMORY_BASIC_INFORMATION)

                //If this memory chunk is accessible
                if (mem_basic_info.Protect == PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
                {
                    //Read everything into a buffer
                    byte[] buffer = new byte[mem_basic_info.RegionSize];
                    ReadProcessMemory((int)processHandle, mem_basic_info.BaseAddress, buffer, mem_basic_info.RegionSize, ref bytesRead);

                    //Search the buffer for CC#s
                    if (string.IsNullOrEmpty(_configFile))
                    {
                        _configFile = "config.xml";
                    }
                    var CCSurfer = new CreditCardSurfer(_configFile);
                    CCSurfer.FindCC(buffer, process.ProcessName, proc_max_address_l);
                }

                // move to the next memory chunk
                proc_min_address_l += mem_basic_info.RegionSize;
                proc_min_address = new IntPtr(proc_min_address_l);

                if (mem_basic_info.RegionSize == 0)
                {
                    break;
                    mem_basic_info.RegionSize = 4096; //in case of a null read, which shouldn't happen
                }
            }
            if (mem_basic_info.RegionSize == 0 && Config.Default.FallBackToProcdump)
            {
                try
                {
                    //In case the above DLL pull in fails to access a process's memory, fail back to SysInternals procdump
                    Process.Start("procdump.exe", "-accepteula -ma " + process.Id + " " + process.Id + ".dmp");

                    using (var fsSource = new FileStream(process.Id + ".dmp", FileMode.Open, FileAccess.Read))
                    {
                        // Read the source file into a byte array. 
                        int numBytesRead = 0;
                        while (numBytesRead < fsSource.Length)
                        {
                            var bytes = new byte[1024];
                            var bytesToRead = (int) fsSource.Length - numBytesRead;
                            if (bytesToRead > 1024)
                            {
                                bytesToRead = 1024;
                            }

                            int n = fsSource.Read(bytes, numBytesRead, bytesToRead);
                            var CCSurfer = new CreditCardSurfer();
                            CCSurfer.FindCC(bytes, process.ProcessName, numBytesRead);

                            if (n == 0)
                            {
                                break;
                            }
                            numBytesRead += n;
                        }
                    }
                    var currentDirectory = Directory.GetCurrentDirectory(); 
                    foreach (var f in new DirectoryInfo(currentDirectory).GetFiles("*.dmp"))
                    {
                        f.Delete();
                    }
                }
                catch { }
            }
        }
    }
}