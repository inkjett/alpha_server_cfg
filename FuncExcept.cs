using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace alphaserver_cfg
{
    internal class FuncExcept
    {
        public static string ExceptionMsg(string _exptIn)
        {
            if (_exptIn.Contains("(0x800706BA): Сервер RPC недоступен.")){
                return "Сервер RPC недоступен(0x800706BA).Проверьте доступность IP в сети.";}
            if (_exptIn.Contains("Отказано в доступе")){
                return "Отказано в доступе. Для применения конфигураций на удаленный компьютер необходимо настоить DCOM (в крайнем случае необходима настроить WMI). " +
                    "\nПример ссылок: https://learn.microsoft.com/ru-ru/windows/win32/wmisdk/connecting-to-wmi-remotely-with-c- ,\n" +
                    "https://community.lansweeper.com/t5/troubleshooting-scanning-issues/wmi-access-is-denied-0x80070005/ta-p/64246 ,\n" +
                    "https://social.msdn.microsoft.com/Forums/en-US/cbd93b81-2c23-4363-a00e-f702d6fa1349/managementscopeconnect-giving-an-error-quotaccess-denied-exception-from-hresult-0x80070005?forum=vbgeneral .\n" +
                    "так же в папке с программой создан файл lansweeper.vbs для записи данных в реестр (файл необходимо запустить с правами администратора на удалнном компьютере).\n" +
                    "Проверку подкючения можно проивести выполнив команду: wmic /user:\"administrator\" /node:\"192.168.1.39\" computersystem get totalphysicalmemory";
                    string filedata = "' Lansweeper settings script\r\n\r\n' Enable dcom\r\nSet Myshell = WScript.CreateObject(\"WScript.Shell\")\r\n\r\nOn Error Resume Next\r\nErr.Clear\r\n\r\nMyshell.RegWrite \"HKLM\\SOFTWARE\\Microsoft\\Ole\\EnableDCOM\",\"Y\",\"REG_SZ\"\r\n\r\nif  Err.Number <> 0  then\r\n  msgbox  \"Error: \" & Err.Number & vbCrLf & Err.Description & vbCrLf & vbCrLf & \"--> Make sure you are running this script elevated with administrative credentials!!\",16,\"Script error\"\r\nend if\r\n\r\nMyshell.RegWrite \"HKLM\\SOFTWARE\\Microsoft\\Ole\\LegacyAuthenticationLevel\",2,\"REG_DWORD\"\r\nMyshell.RegWrite \"HKLM\\SOFTWARE\\Microsoft\\Ole\\LegacyImpersonationLevel\",3,\"REG_DWORD\"\r\n\r\n' Set dcom default permissions\r\nMyshell.regdelete \"HKLM\\SOFTWARE\\Microsoft\\Ole\\DefaultLaunchPermission\"\r\nMyshell.regdelete \"HKLM\\SOFTWARE\\Microsoft\\Ole\\MachineAccessRestriction\"\r\nMyshell.regdelete \"HKLM\\SOFTWARE\\Microsoft\\Ole\\MachineLaunchRestriction\"\r\n\r\n' Set windows firewall\r\nMyshell.run \"netsh firewall set service RemoteAdmin enable\"\r\nMyshell.run \"netsh firewall add portopening protocol=tcp port=135 name=DCOM_TCP135\"\r\n\r\n' Disable simple file sharing\r\nMyshell.RegWrite \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa\\ForceGuest\",\"0\",\"REG_DWORD\"\r\n\r\n' Set LocalAccountTokenFilterPolicy\r\nMyshell.RegWrite \"HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\\LocalAccountTokenFilterPolicy\",\"1\",\"REG_DWORD\"\r\n\r\n' Enable WMI Service and start it\r\nMyshell.run \"sc config winmgmt start= auto\"\r\nMyshell.run \"net start winmgmt\"";
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\lansweeper.vbs", filedata);
            }
            return _exptIn;
        }

    }
}
