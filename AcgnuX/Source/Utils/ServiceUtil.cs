using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Utils
{
    public class ServiceUtil
    {
        /// <summary>
        /// 检查并安装服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceFilePath"></param>
        public static void CheckAndInstall(string serviceName, string serviceFilePath)
        {
            if (!IsServiceExisted(serviceName))
            {
                InstallService(serviceFilePath);
            }
        }

        /// <summary>
        /// 检查并启动服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceFilePath"></param>
        public static void CheckAndStart(string serviceName, string serviceFilePath, string[] startArgs)
        {
            CheckAndInstall(serviceName, serviceFilePath);
            ServiceStart(serviceName, startArgs);
        }

        //判断服务是否存在
        public static bool IsServiceExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController sc in services)
            {
                if (sc.ServiceName.ToLower() == serviceName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        //安装服务
        public static void InstallService(string serviceFilePath)
        {
            using (AssemblyInstaller installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceFilePath;
                IDictionary savedState = new Hashtable();
                installer.Install(savedState);
                installer.Commit(savedState);
            }
        }

        //卸载服务
        public static void UninstallService(string serviceFilePath)
        {
            using (AssemblyInstaller installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceFilePath;
                installer.Uninstall(null);
            }
        }
        //启动服务
        public static void ServiceStart(string serviceName, string[] startArgs)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Stopped)
                {
                    if(null != startArgs && startArgs.Length > 0)
                    {
                        control.Start(startArgs);
                    }
                    else
                    {
                        control.Start();
                    }
                }
            }
        }

        //停止服务
        public static void ServiceStop(string serviceName)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Running)
                {
                    control.Stop();
                }
            }
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="startArgs"></param>
        public static void Restart(string serviceName, string[] startArgs)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Running)
                {
                    control.Stop();
                }
                control.WaitForStatus(ServiceControllerStatus.Stopped);
                control.Start(startArgs);
            }
        }
    }
}
