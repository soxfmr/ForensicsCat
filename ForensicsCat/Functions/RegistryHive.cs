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
        private const uint HKEY_USER = 0x80000003;
        private const uint KEY_READ = 0x20019;

        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern uint RegLoadKey(UInt32 hKey, string lpSubKey, string lpFile);

        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern uint RegUnLoadKey(uint hKey, string lpSubKey);
        
        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern uint RegOpenKeyEx(uint hKey, string lpSubKey, uint ulOptions, uint samDesired, ref uint phkResult);

        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern uint RegCloseKey(uint hKey);

        [DllImport("Advapi32.dll", SetLastError = true)]
        static extern uint RegLoadMUIString(uint hKey, string pszValue, StringBuilder pszOutBuf, int cbOutBuf, ref uint pcbData, int Flags, string pszDirectory);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegQueryInfoKey(
                uint hKey,
                StringBuilder lpClass,
                uint lpcbClass,
                uint lpReserved,
                uint lpcSubKeys,
                uint lpcbMaxSubKeyLen,
                uint lpcbMaxClassLen,
                uint lpcValues,
                uint lpcbMaxValueNameLen,
                uint lpcbMaxValueLen,
                uint lpcbSecurityDescriptor,
                ref long lpftLastWriteTime
            );

        private bool Mounted = false;
        private string MountedName = null;

        public bool ForceMuiLoading
        {
            get;
            set;
        }

        public string MuiDirectory
        {
            get;
            set;
        }

        public RegistryHive(string name, string path)
        {
            MuiDirectory = null;
            ForceMuiLoading = false;

            Mount(name, path);
        }

        public bool Mount(string name, string path)
        {
            uint status = 0;
            bool ret = false;

            RegistryKey subKey;

            MountedName = name;

            subKey = Registry.CurrentUser.OpenSubKey(name);
            if (subKey != null)
            {
                MountedName = Common.GetMd5Hash(name + DateTime.Now.Millisecond);
                subKey.Close();
            }

            status = RegLoadKey(HKEY_USER, MountedName, path);
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
            string keyName = GetMountedKeyName(key);

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
            string keyName = GetMountedKeyName(key);

            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyName))
            {
                ret = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                valueNames = registryKey.GetValueNames();
                foreach (var name in valueNames)
                {
                    ret.Add(name, registryKey.GetValue(name));
                }
            }

            return ret;
        }

        public object GetValue(string key, string valueName)
        {
            object result = null;

            string keyName = GetMountedKeyName(key);
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyName))
            {
                result = registryKey.GetValue(valueName);
            }

            return result;
        }

        public string LoadMUIString(string key, string valueName, string defaultValue = null)
        {
            uint hKey = 0;
            uint status = 0;
            uint requiredSize = 0;

            string result = defaultValue;
            string keyName = GetMountedKeyName(key);

            StringBuilder buffer = null;

            if (string.IsNullOrEmpty(result) || ! result.StartsWith("@"))
            {
                return result;
            }

            // The resource string will be loaded from MuiDirectory if the property is specified
            // or force to read the resource string on the local machine which may has an incompatible version of library

            if (string.IsNullOrEmpty(MuiDirectory) && ! ForceMuiLoading)
            {
                return result;
            }

            status = RegOpenKeyEx(HKEY_USER, keyName, 0, KEY_READ, ref hKey);
            if (status != 0)
            {
                return result;
            }

            buffer = new StringBuilder(4096);

            status = RegLoadMUIString(hKey, valueName, buffer, buffer.Capacity, ref requiredSize, 0, MuiDirectory);
            if (status == 0)
            {
                result = buffer.ToString();
            }

            RegCloseKey(hKey);

            return result;
        }

        public DateTime GetLastModified(string key)
        {
            uint hKey = 0;
            uint status = 0;

            long lastWrittenAt = 0;
            DateTime result = DateTime.MinValue;
            
            string keyName = GetMountedKeyName(key);

            status = RegOpenKeyEx(HKEY_USER, keyName, 0, KEY_READ, ref hKey);
            if (status != 0)
            {
                return result;
            }

            status = RegQueryInfoKey(hKey, null, 0, 0, 0, 0, 0, 0, 0, 0, 0, ref lastWrittenAt);
            if (status == 0)
            {
                result = DateTime.FromFileTimeUtc(lastWrittenAt);
            }

            RegCloseKey(hKey);

            return result;
        }

        public string GetMountedName()
        {
            return MountedName;
        }

        private string GetMountedKeyName(string key)
        {
            return string.Format("{0}\\{1}", MountedName, key);
        }
    }
}
