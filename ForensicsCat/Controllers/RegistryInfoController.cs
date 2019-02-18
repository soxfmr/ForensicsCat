using ForensicsCat.Functions;
using ForensicsCat.Models;
using ForensicsCat.Utilties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static ForensicsCat.Models.ServiceInfo;

namespace ForensicsCat.Controllers
{
    class RegistryInfoController
    {
        private const string KEY_SERVICES = "SYSTEM\\ControlSet001\\Services";
        private const string KEY_OS_INFO = "Microsoft\\Windows NT\\CurrentVersion";

        private RegistryHive systemHive = null;
        private RegistryHive softwareHive = null;

        public RegistryInfoController()
        {
        }

        public List<ServiceInfo> GetServices()
        {
            ServiceInfo service = null;

            string currentSrvKey = null;

            Regex svchost = null;
            ServiceType serviceType;

            List<ServiceInfo> result = null;
            Dictionary<string, object> values = null;

            if (systemHive == null)
            {
                return null;
            }

            result = new List<ServiceInfo>();
            svchost = new Regex("svchost.+?-k", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var serviceNames = systemHive.GetSubKeys(KEY_SERVICES);
            if (serviceNames == null)
            {
                return null;
            }

            foreach (var name in serviceNames)
            {
                currentSrvKey = ConcatKey(KEY_SERVICES, name);

                values = systemHive.GetValuePair(currentSrvKey);
                if (values == null)
                {
                    continue;
                }

                if (! values.ContainsKey("ImagePath") || ! values.ContainsKey("Type"))
                {
                    continue;
                }

                // Ignore the driver services
                serviceType = (ServiceType) values["Type"];
                if (serviceType < ServiceType.Win32OwnProcess)
                {
                    continue;
                }

                service = new ServiceInfo();
                service.Name = name;
                service.Type = serviceType;
                service.ImagePath = values["ImagePath"].ToString();

                service.DisplayName = systemHive.LoadMUIString(currentSrvKey, "DisplayName", values["DisplayName"]?.ToString());
                service.Description = systemHive.LoadMUIString(currentSrvKey, "Description", values["Description"]?.ToString());

                service.ServiceAccount = values["ObjectName"]?.ToString();
                service.FailureCommand = values["FailureCommand"]?.ToString();
                service.LastModified = systemHive.GetLastModified(currentSrvKey);
                
                if (values.ContainsKey("DependOnService"))
                {
                    service.Dependencies = (string[]) values["DependOnService"];
                }
                
                // Reading the real DLL which is get loaded by the service manager
                if (svchost.IsMatch(service.ImagePath))
                {
                    service.ServiceDLL = systemHive.GetValue(ConcatKey(currentSrvKey, "Parameters"), "ServiceDll")?.ToString();
                }

                result.Add(service);
            }

            return result;
        }

        public List<UserInfo> GetUsers()
        {
            return null;
        }

        public List<HotfixInfo> GetHotfixes()
        {
            return null;
        }

        public OSBaseInfo GetOSBaseInfo()
        {
            if (softwareHive == null)
            {
                return null;
            }

            var info = softwareHive.GetValuePair(KEY_OS_INFO);
            if (info == null)
            {
                return null;
            }

            var osInfo = new OSBaseInfo();
            osInfo.ProductName = info["ProductName"]?.ToString();
            osInfo.ProductEdition = info["EditionID"]?.ToString();
            osInfo.Version = info["CurrentVersion"]?.ToString();
            osInfo.CSDVersion = info["CSDVersion"]?.ToString();
            osInfo.BuildNumber = info["CurrentBuildNumber"]?.ToString();
            osInfo.Architecture = null;
            osInfo.Timezone = null;
            osInfo.KernelExecVersion = null;
            osInfo.InstallDate = DateTime.MinValue;

            return osInfo;
        }

        public void GetAutoruns()
        {

        }

        private string ConcatKey(params string[] args)
        {
            return string.Join("\\", args);
        }
    }
}
