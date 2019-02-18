using System;
using System.Collections.Generic;
using System.Text;

namespace ForensicsCat.Models
{
    class OSBaseInfo
    {
        public string ProductName;
        public string ProductEdition;

        public string Version;
        public string CSDVersion;
        public string BuildNumber;

        public string KernelExecVersion;

        public string Architecture;
        public string Timezone;

        public DateTime InstallDate;
    }
}
