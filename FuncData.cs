﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace alphaserver_cfg
{
    internal class FuncData
    {
        public Program Program = new Program();
        public static bool BackUpCFG(string pathFrom, string pathTo, string fileName)
        {
            DateTime now = DateTime.Now;
            try
            {
                FileInfo fileInfo = new FileInfo(pathFrom + "\\" + fileName);
                if (!Directory.Exists(pathTo))
                {
                    Directory.CreateDirectory(pathTo);
                    generateMsg("Создана папка для бакапа файлов конфигураций:" + pathTo);
                }
                fileInfo.CopyTo(pathTo + "\\" + "APServer_" + now.ToString("MM_dd_yyyy_HH_mm") + ".cfg", true);
                generateMsg("Файл бакапа сохранен:" + pathTo + "\\" + "APServer_" + now.ToString("MM_dd_yyyy_HH_mm") + ".cfg");
            }
            catch (Exception e)
            {
                generateMsg(e.ToString());
            }
            return true;
        }

        public static void generateMsg(string msg)
        {
            var appLog = new EventLog("Application");
            appLog.Source = "cfg_to_Alpha.Server";
            appLog.WriteEntry(msg);
        }

        //получам размер папки с бакапами 
        public static double getSizeOfFolder(string folderPath)
        {
            return (from file in Directory.EnumerateFiles(folderPath) let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();//бежим по всем файлам и смотрим их размер
        }


        //определение есть ли парва администратора у запущеного прложения
        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static void regData(string direction,string _CurrentDir)
        {
            if (direction == "install")
            {
                //reg файл
                /*Windows Registry Editor Version 5.00

                [HKEY_CLASSES_ROOT\.cfg]
                @= "AS .cfg"

                [HKEY_CLASSES_ROOT\.cfg\shell]

                [HKEY_CLASSES_ROOT\.cfg\shell\copyAS]
                "MUIVerb" = "Copy to Alpha.Server"

                [HKEY_CLASSES_ROOT\.cfg\shell\copyAS\command]
                @= "C:\\Users\\AutomiqUsr\\Desktop\\projects\\alphaserver_cfg\\alphaserver_cfg\\bin\\Debug\\net6.0\\alphaserver_cfg.exe \"%1\"" // путь указан для примера

                [HKEY_CLASSES_ROOT\Directory\Background\shell\AlphaServer]
                "MUIVerb" = "Alpha.Server"
                "Position" = "Top"
                "SubCommands" = "stop;start"
                
                [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\start]
                [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\start\command]
                @="\"C:\\Users\\AutomiqUsr\\Documents\\alpha_server_cfg\\bin\\Debug\\net6.0-windows\\\\alphaserver_cfg.exe\" \"local:Alpha.Server;start\""

                [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\stop]
                [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\stop\command]
                @="\"C:\\Users\\AutomiqUsr\\Documents\\alpha_server_cfg\\bin\\Debug\\net6.0-windows\\\\alphaserver_cfg.exe\" \"local:Alpha.Server;stop\""*/

                
                // меню правая кнопка по файлу cfg
                RegistryKey currentUserKey = Registry.ClassesRoot;
                RegistryKey cfgKey = currentUserKey.CreateSubKey(".cfg"); // Добовляем пункт меню по клику по файлу .cfg
                RegistryKey shell = cfgKey.CreateSubKey("shell");
                RegistryKey copyAS = shell.CreateSubKey("copyAS");
                RegistryKey command = copyAS.CreateSubKey("command");
                command.SetValue("", "\""+_CurrentDir + "\\alphaserver_cfg.exe\" \"%1\"");
                copyAS.SetValue("MUIVerb", "Copy to Alpha.Server");
                cfgKey.Close();

                //меню перезарузки службы Alpha.Server
                RegistryKey _ControlMenu = currentUserKey.OpenSubKey(@"\Directory\Background\shell",true).CreateSubKey("Alpha.Server");
                _ControlMenu.SetValue("MUIVerb", "Alpha.Server");
                _ControlMenu.SetValue("Position", "Top");
                _ControlMenu.SetValue("SubCommands", "РеСтарт;Стоп;Отключить");
                _ControlMenu.Close();

                RegistryKey _secondary_menu = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell", true);
                RegistryKey _start = _secondary_menu.CreateSubKey("РеСтарт");
                RegistryKey _start_command = _start.CreateSubKey("command");
                _start_command.SetValue("", "\"" + _CurrentDir + "\\alphaserver_cfg.exe\" \"local:Alpha.Server;restart\"");
                _start_command.Close();

                RegistryKey _stop = _secondary_menu.CreateSubKey("Стоп");
                RegistryKey _stop_command = _stop.CreateSubKey("command");
                _stop_command.SetValue("", "\"" + _CurrentDir + "\\alphaserver_cfg.exe\" \"local:Alpha.Server;stop\"");
                _stop_command.Close();

                RegistryKey _disable = _secondary_menu.CreateSubKey("Отключить");
                RegistryKey _disable_command = _disable.CreateSubKey("command");
                _disable_command.SetValue("", "\"" + _CurrentDir + "\\alphaserver_cfg.exe\" \"local:Alpha.Server;disabled\"");
                _disable_command.Close();

                FuncData.generateMsg("Данные для запуска приложения в реестр записаны");
            }
            else if (direction == "uninstall")
            {
                RegistryKey currentUserKey = Registry.ClassesRoot;
                if (currentUserKey.GetSubKeyNames().Contains(".cfg"))
                {
                    currentUserKey.DeleteSubKeyTree(".cfg");
                    currentUserKey.OpenSubKey(@"\Directory\Background\shell", true).DeleteSubKey("Alpha.Server");
                    if (currentUserKey.GetSubKeyNames().Contains("РеСтарт")) { Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell", true).DeleteSubKeyTree("Start"); }
                    if (currentUserKey.GetSubKeyNames().Contains("Стоп")) { Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell", true).DeleteSubKeyTree("Stop"); }
                    if (currentUserKey.GetSubKeyNames().Contains("Отключить")) { Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell", true).DeleteSubKeyTree("Disable"); }
                    FuncData.generateMsg("Данные для запуска приложения удалены из реестра");
                }
                else
                {
                    FuncData.generateMsg("Записей об установленном приложении не найдено");
                }
            }
        }

        public static bool CheckRegData()
        {   
            RegistryKey currentUserKey = Registry.ClassesRoot;
            if (currentUserKey.GetSubKeyNames().Contains(".cfg"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //функция записи в лог - атавизм сохранение лога в файл 
        static void SaveLogToFile(string logMsg, string _CombinePath)
        {
            DateTime now = DateTime.Now;
            string logText = "\n" + now.ToString("hh.mm.ss.fff") + "  " + logMsg;
            if (!File.Exists(_CombinePath))
            {
                var myFile = File.Create(_CombinePath);
                myFile.Close(); // создали и сразу закрыли
                File.AppendAllText(_CombinePath, now.ToString("hh.mm.ss.fff") + "  " + "Файл лога создан");
            }
            File.AppendAllText(_CombinePath, logText);
        }

    }
}
