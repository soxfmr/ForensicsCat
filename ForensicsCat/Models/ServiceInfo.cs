using System;
using System.Collections.Generic;
using System.Text;

namespace ForensicsCat.Models
{
    class ServiceInfo
    {
        public enum ServiceType
        {
            KernelDriver = 1,
            FileSystemDriver = 2,
            Adapter = 4,
            RecognizerDriver = 8,
            Win32OwnProcess = 16,
            Win32ShareProcess = 32,
            InteractiveProcess = 256
        };

        public string Name;
        public string DisplayName;
        public string Description;

        public string ImagePath;
        public string FailureCommand;

        public string ServiceDLL;

        public string ServiceAccount;
        public string[] Dependencies;

        public ServiceType Type;

        public DateTime LastModified;
    }
}
