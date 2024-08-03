using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace alphaserver_cfg
{
    internal class FuncService
    {
        public static void StopService(string serviceName)
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
                FuncData.generateMsg("Служба " + serviceName + " уже остановлена");
            }
        }
        public static void StartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);

            if (GetSvrStatus(serviceName).Contains("Dis")) // проверка на статус запуска службы, отключена или инет 
            {
                ChangeStartMode(serviceName,"Automatic");
            }

            if (service.Status != ServiceControllerStatus.Running) // Если служба не остановлена
            {
                service.Start();                
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(3)); //Используйте , WaitForStatus чтобы приостановить обработку
                                                                                                 //приложения до тех пор, пока служба не достигнет требуемого состояния.
                FuncData.generateMsg("Служба " + serviceName + " запущена");
            }
            else
            {
                FuncData.generateMsg("Служба " + serviceName + " уже запущена");
            }
        }

        public static void killAlphaPorcess(string serviceName)
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
                                FuncData.generateMsg("Служба " + serviceName + " остановлена принудительно");
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
                FuncData.generateMsg("Служба " + serviceName + " остановлена");
            }

        }
        public static void ServiceManagement(string _serviceName , string _command) // общая функция управдения службой
        {
            if (CheckSvrExist(_serviceName))
            {
                if (!string.IsNullOrEmpty(_command) && (!string.IsNullOrEmpty(_serviceName)))
                {
                    if (_command == "stop")
                    {
                        StopService(_serviceName);
                    }
                    else if (_command == "start")
                    {
                        StartService(_serviceName);
                    }
                    else if (_command == "restart")
                    {
                        StopService(_serviceName);
                        StartService(_serviceName);
                    }
                    else if (_command == "disabled")
                    {
                        ChangeStartMode(_serviceName, "Disabled");
                        StopService(_serviceName);
                    }
                }
                else
                {
                    FuncData.generateMsg("Один из параметров при для работы со службой оказался пуст, имя службы: " + _serviceName + ", команда " + _command + ".");
                }
            }
            else
            {
                FuncData.generateMsg("Служба: " + _serviceName + " не найдена в системе");
            }
        }

        public static void ChangeStartMode(string _serviceName, string _command) // функция управления статусом запуска службой, отключена включена итд
        {
            //Automatic
            //Disabled
            var m = new ManagementObject($"Win32_Service.Name=\"{_serviceName}\"");
            m.InvokeMethod("ChangeStartMode", new object[] { _command });
            FuncData.generateMsg("Изменен статус слжубы: " + _serviceName + ", на: " + _command);
        }

        public static string GetSvrStatus(string _serviceName) // функция проверки статуса запуска службы
        {
            ManagementPath p = new ManagementPath($"Win32_Service.Name=\"{_serviceName}\"");
            ManagementObject ManagementObj = new ManagementObject(p);
            return ManagementObj["StartMode"].ToString();     
        }

        public static bool CheckSvrExist(string _serviceName) // проверка наличия службы
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Contains(_serviceName));
        }
    }
}
