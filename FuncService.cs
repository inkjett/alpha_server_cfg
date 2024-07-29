using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            // Если служба не остановлена
            if (service.Status != ServiceControllerStatus.Running)
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
    }
}
