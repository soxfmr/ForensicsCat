using ForensicsCat.Utilties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ForensicsCat.Functions
{
    class RegistryHive
    {
        internal static readonly UInt32 HKEY_USER = 0x80000003;

        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern Int32 RegLoadKey(UInt32 hKey, String lpSubKey, String lpFile);

        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern Int32 RegUnLoadKey(UInt32 hKey, String lpSubKey);

        private bool Mounted = false;
        private string MountedName = null;

        public RegistryHive(string name, string path)
        {
            Mount(name, path);
        }

        public bool Mount(string name, string path)
        {
            int status = 0;
            bool ret = false;

            RegistryKey subKey;

            this.MountedName = name;

            subKey = Registry.CurrentUser.OpenSubKey(name);
            if (subKey != null)
            {
                this.MountedName = Common.GetMd5Hash(name + DateTime.Now.Millisecond);
                subKey.Close();
            }

            status = RegLoadKey(HKEY_USER, this.MountedName, path);
            if (status == 0)
            {
                this.Mounted = true;
                ret = true;
            }

            return ret;
        }

        public void UnMount()
        {
            if (Mounted)
            {
                RegUnLoadKey(HKEY_USER, this.MountedName);
            }
        }
        
        public string[] GetSubKeys(string key)
        {
            string[] ret = null;
            string keyName = string.Format("{0}\\{1}", this.MountedName, key);

            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyName))
            {
                ret = registryKey.GetSubKeyNames();
            }

            return ret;
        }

        public Dictionary<string, object> GetValuePair(string key)
        {
            string[] valueNames = null;

            Dictionary<string, object> ret = null;
            string keyName = string.Format("{0}\\{1}", this.MountedName, key);

            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyName))
            {
                ret = new Dictionary<string, object>();

                valueNames = registryKey.GetValueNames();
                foreach (var name in valueNames)
                {
                    ret.Add(name, registryKey.GetValue(name));
                }
            }

            return ret;
        }

        public string GetMountedName()
        {
            return MountedName;
        }
    }
}
