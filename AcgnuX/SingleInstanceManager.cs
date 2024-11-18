using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AcgnuX
{
    public class SingleInstanceManager
    {
        private static Mutex _mutex;
        private const string MutexName = "AcgnuX_SingleInstance_Mutex";
        private App _app;

        public void Run()
        {
            _mutex = new Mutex(true, MutexName, out bool createdNew);

            if (createdNew)
            {
                // �״�����Ӧ�ó���
                _app = new App();
                _app.Run();
            }
            else
            {
                // ����ʵ��������
                ActivateExistingWindow();
                Environment.Exit(0);
            }

            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }

        private void ActivateExistingWindow()
        {
            // ���Ҳ��������д���
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }
    }

    // ���ڵ��� Win32 API
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}