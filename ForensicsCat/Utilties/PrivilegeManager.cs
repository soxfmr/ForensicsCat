using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ForensicsCat.Utilties
{
    class PrivilegeManager
    {
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";

        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const int TOKEN_QUERY = 0x00000008;

        private const int SE_PRIVILEGE_DISABLED = 0x00000000;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokenPrivileges
        {
            public int PrivilegeCount;
            public long Luid;
            public int Attributes;
        }

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentProcess();

        [DllImport("Advapi32.dll")]
        private static extern bool OpenProcessToken(int ProcessHandle, int DesiredAccess, ref int TokenHandle);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(int TokenHandle, bool DisableAllPrivileges, ref TokenPrivileges NewState, int BufferLength, 
            int PreviousState, int ReturnLength);

        public static bool Disable(string privilege)
        {
            return AdjustPrivilege(privilege, false);
        }

        public static bool Enable(string privilege)
        {
            return AdjustPrivilege(privilege, true);
        }

        private static bool AdjustPrivilege(string privilege, bool enable)
        {
            int token = 0;
            long luid = 0;

            TokenPrivileges tokenPrivileges;

            if (! OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY | TOKEN_ADJUST_PRIVILEGES, ref token))
            {
                return false;
            }

            if (! LookupPrivilegeValue(null, privilege, ref luid))
            {
                return false;
            }

            tokenPrivileges.PrivilegeCount = 1;
            tokenPrivileges.Luid = luid;

            tokenPrivileges.Attributes = enable ? SE_PRIVILEGE_ENABLED : SE_PRIVILEGE_DISABLED;

            if (! AdjustTokenPrivileges(token, false, ref tokenPrivileges, 0, 0, 0))
            {
                return false;
            }

            return true;
        }
    }
}
