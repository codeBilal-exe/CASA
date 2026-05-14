using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MISS.Browser.Services
{
    public class ServerControlService
    {
        private Dictionary<string, Process> _processes = new Dictionary<string, Process>();
        private string _basePath;

        public ServerControlService(string basePath)
        {
            _basePath = basePath;
        }

        public void StartServer(string name, string relativeExePath)
        {
            if (IsRunning(name)) return;

            string fullPath = Path.Combine(_basePath, relativeExePath);
            string? workingDir = Path.GetDirectoryName(fullPath);
            if (workingDir == null) return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fullPath,
                WorkingDirectory = workingDir,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process? process = Process.Start(startInfo);
            if (process != null)
            {
                _processes[name] = process;
            }
        }

        public void StopServer(string name)
        {
            if (_processes.ContainsKey(name))
            {
                try
                {
                    if (!_processes[name].HasExited)
                    {
                        _processes[name].Kill();
                    }
                }
                catch { }
                _processes.Remove(name);
            }
        }

        public bool IsRunning(string name)
        {
            if (_processes.ContainsKey(name))
            {
                if (_processes[name].HasExited)
                {
                    _processes.Remove(name);
                    return false;
                }
                return true;
            }
            return false;
        }

        public void StopAll()
        {
            foreach (var name in new List<string>(_processes.Keys))
            {
                StopServer(name);
            }
        }
    }
}
