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
using alphaserver_cfg;


partial class Program
{
    //переменные для параметров запуска 
    static bool launchIsAdmin = false; // выставляется в true когда запускаем приложение с правами администратора
    //static bool needLogs = false; // включить логирование 
    static bool needToInsrall = false; // установка
    static bool needToUnInstall = false; // удаление
    static bool help = false; // помощь
    static bool remote = false;
    static bool tmp = false;
    //static string CurrentDir = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\")); //текущая папка с проектом
    static string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
    static string currentPathToServer = "C:\\Program Files\\Automiq\\Alpha.Server\\Server";
    static string currnetCfgName = "APServer.cfg";
    static FuncExcept MsgExept = new FuncExcept();
    static FuncData FuncData = new FuncData();
    static FuncService FuncService = new FuncService();


    //информация по лог файлу
    public static string logfileName = "logFile.txt";
    public static string CombinePath = Path.Combine(CurrentDir, logfileName); //общий путь до файла




    static void Main(string[] args)
    {
        //var temp = Assembly.GetExecutingAssembly();
        //generateMsg("Location="+Assembly.GetExecutingAssembly());
        //проходим по аргументам при запуске файла проекта


        ProcessStartInfo proc = new ProcessStartInfo();
        proc.UseShellExecute = true; // непонятный параметр, который не позволяет запускать приложение из cmd если true
        
        proc.WorkingDirectory = CurrentDir;
        proc.FileName = CurrentDir + "\\alphaserver_cfg.exe";
        if (!FuncData.IsAdministrator())
        {
            string Proc_arg = "";
            if (args.Length > 0)
            {
                foreach (string arg in args) // Аргументы при запуске процессе
                {
                    if (arg.Contains(".cfg"))
                    {
                        Proc_arg = arg.Replace(" ", "<"); ;
                    }
                    else if (arg.Contains("help") || arg.Contains("?"))
                    {
                        FuncData.generateMsg("Приложение для автоматического копирования конфигурации Alpha.Server в папку со службой Alpha.Server и автоматическим перезапуском службы.\n" +
                                    "Для установки/удаления приложения используйте атрибуты install или uninstall, пример alphaserver_cfg.exe install или alphaserver_cfg.exe uninstall.\n" +
                                    "Копирование конфигурации осуществляется вызовом контестного меню кликом правой кнопки мыши по фалу .cfg\n" +
                                    "Made by Oleg Galyamov for all users using AlphaPlatform (galyamov.oleg@automiq.ru)");
                    }
                    else { Proc_arg = arg; }
                }
            }
            else
            {
                Proc_arg = "empty";
            }
            proc.Arguments = Proc_arg;
            proc.Verb = "runas";
            Process.Start(proc); //запукаем процесс поторно с парвами администаратора
            Environment.Exit(0); // завершаем текущий
        }

        //пока не придумал ничего лучше по получению пути до запускаемого файла, ниже это примеры что пробывал 
        //Console.WriteLine("CodeBase="+System.Reflection.Assembly.GetEntryAssembly().CodeBase); //Location=C:\Users\AutomiqUsr\Desktop\projects\alphaserver_cfg\alphaserver_cfg\bin\Debug\net6.0\alphaserver_cfg.dll
        //Console.WriteLine("Location=" + Assembly.GetExecutingAssembly().Location); //Location=C:\Users\AutomiqUsr\Desktop\projects\alphaserver_cfg\alphaserver_cfg\bin\Debug\net6.0\alphaserver_cfg.dll  
        //Console.WriteLine("GetCurrentDirectory="+Directory.GetCurrentDirectory());// GetCurrentDirectory=C:\Users\AutomiqUsr        
        //Console.WriteLine("FileName=" + proc.FileName); //FileName=C:\Users\AutomiqUsr\alphaserver_cfg.exe
        //Console.WriteLine("FileName2="+ Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"))); //FileName2=C:\Users\AutomiqUsr
      
        if (FuncData.IsAdministrator())
        {
            if (args.Length == 0) // проверка на пустой параметр при запуске, чтобы не было исколючения 
            {
                    FuncData.generateMsg("При запуске приложения с правами адинистратора входной атрибут оказался пуст");
                    Environment.Exit(0);
            }
            string Proc_arg = args[0];
            if (Proc_arg == "empty")//удаление установка в реестре
            {
                if (FuncData.CheckRegData())
                {
                    FuncData.regData("uninstall", CurrentDir);
                }
                else
                {
                    FuncData.regData("install", CurrentDir);
                }
            }
            else if (Proc_arg == "install")
            {
                FuncData.regData("install", CurrentDir);
            }
            else if (Proc_arg == "uninstall")
            {
                FuncData.regData("uninstall", CurrentDir);
            }
            else if (Proc_arg.Contains(".cfg"))
            {
                FuncService.StopService("Alpha.Server");
                if (Proc_arg.Contains(".cfg"))
                {
                    FuncData.BackUpCFG(currentPathToServer, Path.Combine(currentPathToServer.Remove(currentPathToServer.LastIndexOf("Server") - 1), "CfgBackUP"), currnetCfgName);
                    Proc_arg = Proc_arg.Replace("<", " ");
                    FileInfo fileInfo = new FileInfo(Proc_arg);
                    fileInfo.CopyTo(Path.Combine(currentPathToServer, currnetCfgName), true);
                    FuncData.generateMsg("Конфигурация " + Proc_arg + " скопирована в папку c Alpha.Server");
                    FuncData.generateMsg("Текущий размер папки с баками:" + (FuncData.getSizeOfFolder(Path.Combine(currentPathToServer.Remove(currentPathToServer.LastIndexOf("Server") - 1), "CfgBackUP")) / 1048576) + "мб");
                }
                else
                {
                    FuncData.generateMsg("Аргумент не содержит ссылок на конфигурацию. Аргумент - " + Proc_arg);
                }
                System.Threading.Thread.Sleep(1000);
                FuncService.StartService("Alpha.Server");
            }
            else if (remote)
            {
                string _IP = "192.168.1.39";
                string _user = "administrator";
                string _password = "qwerty@123";
                try
                {
                    Console.WriteLine("Remote");
                    //FuncRemote.getConnectToRemotePC(_IP, _user, _password);
                    //Console.ReadLine();
                }
                catch (Exception e)
                {
                    FuncData.generateMsg("При подключени к " + _IP + "\\" + _user + " произошла ошибка: " + FuncExcept.ExceptionMsg(e.ToString()));
                }

            }
            else if (Proc_arg.Contains("local"))
            {
                string[] tmp = Proc_arg.Substring(Proc_arg.IndexOf(":")+1).Split(";");
                //Console.WriteLine(tmp[0]);
                //Console.ReadLine();

            }
        }
    }
}