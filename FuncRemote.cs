using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace alphaserver_cfg
{
    internal class FuncRemote
    {       
        public static void getConnectToRemotePC(string _serverIP, string _user, string _password)
        {
            ConnectionOptions connection = new ConnectionOptions();
            //connection.Username = "DESKTOP-U4JAP8P\\AutomiqUsr";
            connection.Username = _user;
            connection.Password = _password;
            connection.Authority = "ntlmdomain:DOMAIN";
            connection.EnablePrivileges = true;
            connection.Authentication = AuthenticationLevel.Unchanged;
            connection.Impersonation = ImpersonationLevel.Impersonate;
            ManagementScope scope = new ManagementScope("\\\\192.168.1.39\\root\\CIMV2", connection);
            //ManagementScope scope = new ManagementScope("\\\\172.16.149.118\\root\\CIMV2", connection);
            scope.Connect();
            Console.WriteLine("IsConnected=" + scope.IsConnected);
            Console.ReadLine();
        }
    }
}
