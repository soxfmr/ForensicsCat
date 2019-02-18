using System;
using System.Collections.Generic;
using System.Text;

namespace ForensicsCat.Models
{
    class UserInfo
    {
        public string SID;

        public string Username;
        public List<string> Groups;

        public DateTime LastLogon;
        public DateTime LastPassChanged;
        public DateTime CreatedAt;
    }
}
