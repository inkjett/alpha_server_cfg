// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;
using System.Text;
//using System.Runtime.InteropServices;
//using System.ServiceProcess; // нужно добавить ссылку в проект
//using static System.Net.Mime.MediaTypeNames;
//using static System.ServiceProcess.ServiceController;
//using Microsoft.Win32;
//using Windows;
//using System.Security.Principal;
//using System.Diagnostics;
//using System.Security.Principal;
using System.IO; // работа с файлами 
using System.Reflection;
using alphaserver_cfg;
using System.Windows.Threading;
using System.Windows;
using WpfApp1;
using System.Threading.Channels;

partial class Program
{
    //переменные для параметров запуска 
    static bool launchIsAdmin = false; // выставляется в true когда запускаем приложение с правами администратора
    //static bool needLogs = false; // включить логирование 
    static bool needToInsrall = false; // установка
    static bool needToUnInstall = false; // удаление
    static bool help = false; // помощь
    static bool remote = false;
    //static string CurrentDir = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\")); //текущая папка с проектом
    public static string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
    static bool epmtyParametr = false; 
    static string currentPathToServer = "C:\\Program Files\\Automiq\\Alpha.Server\\Server";
    static string currnetCfgName = "APServer.cfg";
    static FuncExcept MsgExept = new FuncExcept();
    static FuncData FuncData = new FuncData();
    static FuncService FuncService = new FuncService();
    public static ProcessStartInfo proc = new ProcessStartInfo();

    //информация по лог файлу
    public static string logfileName = "logFile.txt";
    public static string CombinePath = Path.Combine(CurrentDir, logfileName); //общий путь до файла



    [STAThread]
    static void Main(string[] args)
    {
        //проходим по аргументам при запуске файла проекта

        proc.UseShellExecute = true; // непонятный параметр, который не позволяет запускать приложение из cmd если true
                                     
        proc.WorkingDirectory = CurrentDir;
        proc.FileName = CurrentDir + "\\alphaserver_cfg.exe";
        string Proc_arg = "";
        proc.Arguments = Proc_arg;
        //proc.Arguments = "launchisAdminTrue" + (needLogs ? " needLogs" : "") + (needToInsrall ? " needToInsrall" : "") + (needToInsrall ? " needToInsrall" : "");
        proc.Verb = "runas";
        if (args.Length != 0)
        {
            foreach (string arg in args) // Аргументы при запуске процессе
            {
                if (arg.Contains(".cfg"))
                {                              
                    Proc_arg = arg;
                    Proc_arg = Proc_arg.Replace(" ", "<");
                    epmtyParametr = false;
                    break;
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
            //FuncData.generateMsg("Proc_arg1=" + Proc_arg);
            proc.Arguments = Proc_arg;
            //proc.Arguments = "launchisAdminTrue" + (needLogs ? " needLogs" : "") + (needToInsrall ? " needToInsrall" : "") + (needToInsrall ? " needToInsrall" : "");
            proc.Verb = "runas";
            if (!FuncData.IsAdministrator() && !help && !epmtyParametr)
            {
                Process.Start(proc); //запукаем процесс поторно с правами администаратора
                Environment.Exit(0); // завершаем текущий
            }
            else if (help)
            {
                FuncData.generateMsg("Приложение для автоматического копирования конфигурации Alpha.Server в папку со службой Alpha.Server и автоматическим перезапуском службы.\n" +
                            "Для установки/удаления приложения используйте атрибуты install или uninstall, пример alphaserver_cfg.exe install или alphaserver_cfg.exe uninstall.\n" +
                            "Копирование конфигурации осуществляется вызовом контестного меню кликом правой кнопки мыши по фалу .cfg\n" +
                            "Made by Oleg Galyamov for all users using AlphaPlatform (galyamov.oleg@automiq.ru)");
            }
            else if (FuncData.IsAdministrator())
            {
                if (!needToInsrall && !needToUnInstall && !remote)
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
                    string _IP = "192.168.1.40";
                    string _user = "administrator";
                    string _password = "qwerty@123";
                    try
                    {
                        Console.WriteLine("Remote");
                        FuncRemote.getConnectToRemotePC(_IP, _user, _password);
                    }
                    catch (Exception e)
                    {
                        FuncData.generateMsg("При подключени к " + _IP + "\\" + _user + " произошла ошибка: " + FuncExcept.ExceptionMsg(e.ToString()));
                    }

                }

            }

        }
        else
        {
            if (!FuncData.IsAdministrator())
            {
                Process.Start(proc); //запукаем процесс поторно с правами администаратора
                Environment.Exit(0); // завершаем текущий
            }
            var app = new Application();
            var window = new CurrentWindow();
            app.Run(window);
         }
    }
}