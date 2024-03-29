﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
                    generateMsg("Создаа паппка для бакапа конфигураций:" + pathTo);
                }
                fileInfo.CopyTo(pathTo + "\\" + "APServer_" + now.ToString("MM_dd_yyyy_HH_mm") + ".cfg", true);
                generateMsg("Файл бакапа сохранен:" + pathTo + "\\" + "APServer_" + now.ToString("MM_dd_yyyy_HH_mm") + ".cfg");
            }
            catch (Exception e)
            {
                generateMsg(e.ToString());
            }
            //generateMsg("Конфигурация " + Proc_arg + " скопирована в папку c Alpha.Server");
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
                @= "C:\\Users\\AutomiqUsr\\Desktop\\projects\\alphaserver_cfg\\alphaserver_cfg\\bin\\Debug\\net6.0\\alphaserver_cfg.exe \"%1\""*/ // путь указан для примера

                RegistryKey currentUserKey = Registry.ClassesRoot;
                RegistryKey cfgKey = currentUserKey.CreateSubKey(".cfg");
                RegistryKey shell = cfgKey.CreateSubKey("shell");
                RegistryKey copyAS = shell.CreateSubKey("copyAS");
                RegistryKey command = copyAS.CreateSubKey("command");
                command.SetValue("", _CurrentDir + "\\alphaserver_cfg.exe \"%1\"");
                copyAS.SetValue("MUIVerb", "Copy to Alpha.Server");
                cfgKey.Close();
                FuncData.generateMsg("Данные для запуска приложения в реестр записаны");
            }
            else if (direction == "uninstall")
            {
                RegistryKey currentUserKey = Registry.ClassesRoot;
                RegistryKey cfgKey = currentUserKey.OpenSubKey(".cfg", true);
                cfgKey.DeleteSubKeyTree("shell");
                cfgKey.Close();
                FuncData.generateMsg("Данные для запуска приложения удалены из реестра");
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
