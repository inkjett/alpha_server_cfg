// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;
using System.Text;
//using System.Runtime.InteropServices;
using System.ServiceProcess; // нужно добавить ссылку в проект
//using static System.Net.Mime.MediaTypeNames;
//using static System.ServiceProcess.ServiceController;
using Microsoft.Win32;
//using Windows;
using System.Security.Principal;
//using System.Diagnostics;
//using System.Security.Principal;
//using System.IO; // работа с файлами 
using System.Reflection;





class Program
{
    //переменные для параметров запуска 
    static bool launchIsAdmin = false; // выставляется в true когда запускаем приложение с правами администратора
    //static bool needLogs = false; // включить логирование 
    static bool needToInsrall = false; // установка
    static bool needToUnInstall = false; // удаление
    static bool help = false; // помощь
    static bool remote = false;
    //static string CurrentDir = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\")); //текущая папка с проектом
    static string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
    static bool epmtyParametr = true;
    static string currentPathToServer = "C:\\Program Files\\Automiq\\Alpha.Server\\Server";
    static string currnetCfgName = "APServer.cfg";

  

    
    //информация по лог файлу
    static string logfileName = "logFile.txt";
    static string CombinePath = Path.Combine(CurrentDir, logfileName); //общий путь до файла

    //процесс Alpha.Server, если его нужно завершить

    static void generateMsg(string msg)
    {
        var appLog = new EventLog("Application");
        appLog.Source = "cfg_to_Alpha.Server";
        appLog.WriteEntry(msg);
    }


    // Остановка службы
    static void StopService(string serviceName) 
    {
        ServiceController service = new ServiceController(serviceName);
        if (service.Status != ServiceControllerStatus.Stopped)
        {
            if (!service.Status.Equals(ServiceControllerStatus.StopPending))
            {
                service.Stop();
                System.Threading.Thread.Sleep(1000);
                killAlphaPorcess(serviceName);//т.к. иногда бывает нужно останавливать службу со статусом "Останока", выполняем функцию функцию чтобы "убить" процесс принудительно
            }
        }
        else
        {
            generateMsg("Служба " + serviceName + " уже остановлена");
        }
    }

    //функция принудительной остановки службы
    static void killAlphaPorcess(string serviceName)
    {
        ServiceController service = new ServiceController(serviceName);//обявляем новую службу
        if (service.Status.Equals(ServiceControllerStatus.StopPending))
        {
            var startDT = DateTime.Now; // переменная с началом выполнения цикла
            var endDT = DateTime.Now; // переменная с окончанием выполнения цикла
            while (service.Status.Equals(ServiceControllerStatus.StopPending)) // если вдруг служба висит в стостоянии "Остановка" ждем 5 сек и "убиваем" процесс
            {
                if ((endDT - startDT).Seconds >= 5)
                {
                    var process = Process.GetProcessesByName("Alpha.Server"); // ищем нужный процесс
                    foreach (Process i in process) // для каждого процессе Alpha.Server проверяем нужный путь 
                    {
                        if (i.MainModule.FileName.Contains("Automiq\\Alpha.Server\\Server"))
                        {
                            i.Kill(); // закрываем текущее приложение
                            generateMsg("Служба " + serviceName + " остановлена принудительно");
                            break;
                        }
                    }
                    break;
                }
                endDT = DateTime.Now;
            }
        }
        else
        {
            generateMsg("Служба " + serviceName + " остановлена");
        }
        
    }

    // Запуск слжубы
    static void StartService(string serviceName)
    {
        ServiceController service = new ServiceController(serviceName);
        // Если служба не остановлена
        if (service.Status != ServiceControllerStatus.Running)
        {
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(3)); //Используйте , WaitForStatus чтобы приостановить обработку
                                                                                             //приложения до тех пор, пока служба не достигнет требуемого состояния.
            generateMsg("Служба "+ serviceName + " запущена");
        }
        else
        {
            generateMsg("Служба " + serviceName + " уже запущена");
        }
    }


    //функция записи в лог - атавизм сохранение лога в файл 
    static void SaveLogToFile(string logMsg)    
    {
        DateTime now = DateTime.Now;
        string logText = "\n" + now.ToString("hh.mm.ss.fff") + "  "+ logMsg;
        if (!File.Exists(CombinePath))
        {
            var myFile = File.Create(CombinePath); 
            myFile.Close(); // создали и сразу закрыли
            File.AppendAllText(CombinePath, now.ToString("hh.mm.ss.fff") + "  " + "Файл лога создан");
        }
        File.AppendAllText(CombinePath, logText);
    }

    //определение есть ли парва администратора у запущеного прложения
    static bool IsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    static bool BackUpCFG(string pathFrom, string pathTo, string fileName)
    {
        DateTime now = DateTime.Now;
        try
        {
            FileInfo fileInfo = new FileInfo(pathFrom + "\\" + fileName);
            if (!Directory.Exists(pathTo))
            { 
                Directory.CreateDirectory(pathTo);
                generateMsg("Создаа паппка для бакапа конфигураций:"+pathTo);
            }
            fileInfo.CopyTo(pathTo + "\\" + "APServer_" + now.ToString("MM_dd_yyyy_HH_mm") + ".cfg", true);
            generateMsg("Файл бакапа сохранен:"+ pathTo + "\\" + "APServer_" + now.ToString("MM_dd_yyyy_HH_mm") + ".cfg");
        }
        catch (Exception e)
        {
            generateMsg(e.ToString());
        }
        //generateMsg("Конфигурация " + Proc_arg + " скопирована в папку c Alpha.Server");
        return true;
    }
    //получам размер папки с бакапами 
    static double getSizeOfFolder(string folderPath)
    {
        return (from file in Directory.EnumerateFiles(folderPath) let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();//бежим по всем файлам и смотрим их размер
    }

    static void getConnectToRemotePC()
    {
        ConnectionOptions connection = new ConnectionOptions();
        //connection.Username = "DESKTOP-U4JAP8P\\AutomiqUsr";
        connection.Username = "AutomiqUsr";
        connection.Password = "qwerty@123";
        connection.Authority = "ntlmdomain:DOMAIN";
        connection.EnablePrivileges = true;
        connection.Authentication = AuthenticationLevel.Unchanged;
        connection.Impersonation = ImpersonationLevel.Impersonate;
        ManagementScope scope = new ManagementScope("\\\\172.16.149.118\\root\\CIMV2", connection);
        //@"\\" + strIPAddress + @"\root\cimv2"
        scope.Connect();
        Console.WriteLine("IsConnected="+scope.IsConnected);
        Console.ReadLine();
    }

    static void Main(string[] args)
    {
        //var temp = Assembly.GetExecutingAssembly();
        //generateMsg("Location="+Assembly.GetExecutingAssembly());
        //проходим по аргументам при запуске файла проекта
        ProcessStartInfo proc = new ProcessStartInfo();
        proc.UseShellExecute = true; // непонятный параметр, который не позволяет запускать приложение из cmd если true
        
        proc.WorkingDirectory = CurrentDir;
        proc.FileName = CurrentDir + "\\alphaserver_cfg.exe";
        string Proc_arg = ""; 
        foreach (string arg in args) // Аргументы при запуске процессе
        {
            if (arg.Contains(".cfg"))
            {
                Proc_arg = arg;
                epmtyParametr = false;
            }
            else if (arg.Contains("help") || arg.Contains("?"))
            {
                help = true;
                epmtyParametr = false;
            }
            else if (arg == "launchisAdminTrue" && !help)
            {
                launchIsAdmin = true;
                epmtyParametr = false;
            }
            else if (arg == "install" && !help && !needToUnInstall)
            {
                needToInsrall = true;
                Proc_arg += "install";
                epmtyParametr = false;
            }
            else if (arg == "uninstall" && !help && !needToInsrall)
            {
                needToUnInstall = true;
                Proc_arg += " uninstall";
                epmtyParametr = false;
            }
            else if (arg == "remote")
            {
                remote = true;
                Proc_arg = " remote";
                epmtyParametr = false;
            }
        }    

        //пока не придумал ничего лучше по получению пути до запускаемого файла, ниже это примеры что пробывал 
        //Console.WriteLine("CodeBase="+System.Reflection.Assembly.GetEntryAssembly().CodeBase); //Location=C:\Users\AutomiqUsr\Desktop\projects\alphaserver_cfg\alphaserver_cfg\bin\Debug\net6.0\alphaserver_cfg.dll
        //Console.WriteLine("Location=" + Assembly.GetExecutingAssembly().Location); //Location=C:\Users\AutomiqUsr\Desktop\projects\alphaserver_cfg\alphaserver_cfg\bin\Debug\net6.0\alphaserver_cfg.dll  
        //Console.WriteLine("GetCurrentDirectory="+Directory.GetCurrentDirectory());// GetCurrentDirectory=C:\Users\AutomiqUsr        
        //Console.WriteLine("FileName=" + proc.FileName); //FileName=C:\Users\AutomiqUsr\alphaserver_cfg.exe
        //Console.WriteLine("FileName2="+ Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"))); //FileName2=C:\Users\AutomiqUsr
        proc.Arguments = Proc_arg;
        //proc.Arguments = "launchisAdminTrue" + (needLogs ? " needLogs" : "") + (needToInsrall ? " needToInsrall" : "") + (needToInsrall ? " needToInsrall" : "");
        proc.Verb = "runas";

        static void regData(string direction)
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
                command.SetValue("", CurrentDir + "\\alphaserver_cfg.exe \"%1\"");
                copyAS.SetValue("MUIVerb", "Copy to Alpha.Server");
                cfgKey.Close();
                generateMsg("Данные для запуска приложения в реестр записаны");
            }
            else if (direction == "uninstall")
            {
                RegistryKey currentUserKey = Registry.ClassesRoot;
                RegistryKey cfgKey = currentUserKey.OpenSubKey(".cfg", true);
                cfgKey.DeleteSubKeyTree("shell");
                cfgKey.Close();
                generateMsg("Данные для запуска приложения удалены из реестра");
            }
        }

        try
        {
            if (!IsAdministrator()&& !launchIsAdmin && !help && !epmtyParametr)
            {

               Process.Start(proc); //запукаем процесс поторно с парвами администаратора
               Environment.Exit(0); // завершаем текущий
            }
            else if (help)
            {
                /*.WriteLine("Приложение для автоматического копирования конфигурации Alpha.Server в папку со службой Alpha.Server и автоматическим перезапуском службы");
                Console.WriteLine("Для установки/удаления приложения используйте атрибуты install или uninstall, пример alphaserver_cfg.exe install или alphaserver_cfg.exe uninstall");
                Console.WriteLine("Копирование конфигурации осуществляется вызовом контестного меню кликом правой кнопки мыши по фалу .cfg");
                Console.WriteLine("Made by Oleg Galyamov for all users using AlphaPlatform (galyamov.oleg@automiq.ru)");*/
                generateMsg("Приложение для автоматического копирования конфигурации Alpha.Server в папку со службой Alpha.Server и автоматическим перезапуском службы.\n" +
                            "Для установки/удаления приложения используйте атрибуты install или uninstall, пример alphaserver_cfg.exe install или alphaserver_cfg.exe uninstall.\n"+
                            "Копирование конфигурации осуществляется вызовом контестного меню кликом правой кнопки мыши по фалу .cfg\n"+
                            "Made by Oleg Galyamov for all users using AlphaPlatform (galyamov.oleg@automiq.ru)");
            }
            else if (IsAdministrator() && !epmtyParametr && !help)
            {
                if (needToInsrall)
                {
                    regData("install");
                }
                if (needToUnInstall)
                {
                    regData("uninstall");
                }
                else if (!needToInsrall && !needToUnInstall &&!remote)
                {
                    StopService("Alpha.Server");
                    if (Proc_arg.Contains(".cfg"))
                    {
                        BackUpCFG(currentPathToServer, Path.Combine(currentPathToServer.Remove(currentPathToServer.LastIndexOf("Server") - 1), "CfgBackUP"), currnetCfgName);
                        FileInfo fileInfo = new FileInfo(Proc_arg);
                        fileInfo.CopyTo(Path.Combine(currentPathToServer, currnetCfgName), true);
                        generateMsg("Конфигурация " + Proc_arg + " скопирована в папку c Alpha.Server");
                        generateMsg("Текущий размер папки с баками:" + (getSizeOfFolder(Path.Combine(currentPathToServer.Remove(currentPathToServer.LastIndexOf("Server") - 1), "CfgBackUP")) / 1048576) + "мб");
                    }
                    else
                    {
                        generateMsg("Аргумент не содержит ссылок на конфигурацию. Аргумент - " + Proc_arg);
                    }
                    System.Threading.Thread.Sleep(1000);
                    StartService("Alpha.Server");
                }
                else if (remote)
                {
                    getConnectToRemotePC();
                    Console.ReadLine();
                }

            }
        }
         catch (Exception e)
        {
            generateMsg(e.ToString());
            Console.WriteLine(e.ToString());
            Console.ReadLine();
        }
    }
}