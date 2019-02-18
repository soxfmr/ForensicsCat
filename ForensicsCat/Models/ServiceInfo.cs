using System;
using System.Collections.Generic;
using System.Text;

namespace ForensicsCat.Models
{
    class ServiceInfo
    {
        public string Name;
        public string DisplayName;
        public string Description;

        public string ImagePath;
        public string FailureCommand;

        public string ServiceAccount;
        public List<string> Dependencies;

        public DateTime LastModified;
    }
}
