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
    //static string CurrentDir = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\")); //текущая папка с проектом
    static string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
    static string currentPathToServer = "C:\\Program Files\\Automiq\\Alpha.Server\\Server";
    static string currnetCfgName = "APServer.cfg";


    static void Main(string[] args)
    {

        ProcessStartInfo proc = new ProcessStartInfo();
        proc.UseShellExecute = true; // непонятный параметр, который не позволяет запускать приложение из cmd если true        
        proc.WorkingDirectory = CurrentDir;
        proc.FileName = CurrentDir + "\\alphaserver_cfg.exe";
        
        if (!FuncData.IsAdministrator()) // первый запуск без прав адмнистратора 
        {
            if (args.Length > 0)
            {
                foreach (string arg in args) // Аргументы при запуске процессе
                {
                    if (arg.Contains(".cfg"))
                    {
                        proc.Arguments = arg.Replace(" ", "<");
                    }
                    else { proc.Arguments = arg; }
                }
            }
            else
            {
                proc.Arguments = "empty";
            }
            proc.Verb = "runas";
            Process.Start(proc); //запукаем процесс поторно с парвами администаратора
            Environment.Exit(0); // завершаем текущий процесс 
        }

        //пока не придумал ничего лучше по получению пути до запускаемого файла, ниже это примеры что пробывал 
        //Console.WriteLine("CodeBase="+System.Reflection.Assembly.GetEntryAssembly().CodeBase); //Location=C:\Users\AutomiqUsr\Desktop\projects\alphaserver_cfg\alphaserver_cfg\bin\Debug\net6.0\alphaserver_cfg.dll
        //Console.WriteLine("Location=" + Assembly.GetExecutingAssembly().Location); //Location=C:\Users\AutomiqUsr\Desktop\projects\alphaserver_cfg\alphaserver_cfg\bin\Debug\net6.0\alphaserver_cfg.dll  
        //Console.WriteLine("GetCurrentDirectory="+Directory.GetCurrentDirectory());// GetCurrentDirectory=C:\Users\AutomiqUsr        
        //Console.WriteLine("FileName=" + proc.FileName); //FileName=C:\Users\AutomiqUsr\alphaserver_cfg.exe
        //Console.WriteLine("FileName2="+ Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"))); //FileName2=C:\Users\AutomiqUsr
      
        if (FuncData.IsAdministrator()) // второй запуск, уже с правами адинистратора
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
                    FuncData.generateMsg("Приложение для автоматического копирования файла конфигурации Alpha.Server (*.cfg) в папку со службой Alpha.Server и последующим автоматическим перезапуском службы.\n" +
                                         "Копирование конфигурации осуществляется вызовом контестного меню кликом правой кнопки мыши по фалу с раширением .cfg\n"+
                                         "Так же по нажатию правой кнопки мыши и вызова контекстного меню доступен перезапуск службы Alpha.Server");
                }
            }
            else if (Proc_arg.Contains(".cfg"))
            {
                FuncService.ServiceManagement("Alpha.Server", "stop");
                if (Proc_arg.Contains(".cfg"))
                {
                    FuncData.BackUpCFG(currentPathToServer, Path.Combine(currentPathToServer.Remove(currentPathToServer.LastIndexOf("Server") - 1), "CfgBackUP"), currnetCfgName);
                    Proc_arg = Proc_arg.Replace("<", " ");
                    FileInfo fileInfo = new FileInfo(Proc_arg);
                    fileInfo.CopyTo(Path.Combine(currentPathToServer, currnetCfgName), true);
                    FuncData.generateMsg("Конфигурация " + Proc_arg + " скопирована в папку Alpha.Server");
                    FuncData.generateMsg("Текущий размер папки с баками:" + (FuncData.getSizeOfFolder(Path.Combine(currentPathToServer.Remove(currentPathToServer.LastIndexOf("Server") - 1), "CfgBackUP")) / 1048576) + "мб");
                }
                else
                {
                    FuncData.generateMsg("Аргумент не содержит ссылок на конфигурацию. Аргумент - " + Proc_arg);
                }
                System.Threading.Thread.Sleep(1000);
                FuncService.ServiceManagement("Alpha.Server", "start");
            }
            else if (Proc_arg.Contains("local"))
            {
                string[] tmp = Proc_arg.Substring(Proc_arg.IndexOf(":") + 1).Split(";");
                FuncService.ServiceManagement(tmp[0], tmp[1]);
            }
            else if (Proc_arg.Contains("remote"))
            {
                string _IP = "192.168.1.39";
                string _user = "administrator";
                string _password = "qwerty@123";
                try
                {
                    Console.WriteLine("Remote");
                }
                catch (Exception e)
                {
                    FuncData.generateMsg("При подключени к " + _IP + "\\" + _user + " произошла ошибка: " + FuncExcept.ExceptionMsg(e.ToString()));
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
        }
    }
}