using BackUp.Core;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace BackUp.Svc
{
    public partial class Service1 : ServiceBase
    {
        /// <summary>
        /// Флаг разрешённости сервиса
        /// </summary>
        private bool enableService = true;

        /// <summary>
        /// Поток
        /// </summary>
        private Thread serviceWorker = null;

        /// <summary>
        /// Параметры из конфига
        /// </summary>
        ConfigParams _params = new ConfigParams();

        /// <summary>
        /// Конструктор
        /// </summary>
        public Service1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// На старт
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                serviceWorker = new Thread(DoBackups);
                serviceWorker.Start();
            }
            catch (Exception e)
            {
                ServiceLogger.Info("{error}", "произошла ошибка");
                ServiceLogger.Error("{error}", e.ToString());
            }
        }

        /// <summary>
        /// На остановку
        /// </summary>
        protected override void OnStop()
        {
            enableService = false;
            serviceWorker.Abort();
        }

        /// <summary>
        /// Создание backup-ов
        /// </summary>
        private void DoBackups()
        {
            while (enableService)
            {
                // текущее время
                DateTime now = DateTime.Now;
                // разница во времени
                DateTime deltaTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                string[] timeArray = _params.StartTime.Split(':');

                if (now.Day != _params.Day || !_params.Months.Contains(now.Month))
                {
                    // засыпаем на сутки, если день не тот
                    Thread.Sleep(deltaTime.AddDays(1) - deltaTime);
                }
                else
                {
                    int hour = Int32.Parse(timeArray[0]);
                    int minutes = Int32.Parse(timeArray[1]);

                    // дата следующего backup-а
                    DateTime nextSending = new DateTime(now.Year, now.Month, _params.Day, hour, minutes, 0);

                    // время ожидания
                    var awaitDate = nextSending.Subtract(now);

                    if (awaitDate.TotalMilliseconds < 0)
                    {
                        Thread.Sleep(3600000);
                        continue;
                    }

                    ServiceLogger.Info("{thread}", $"дата запуска: {_params.Day} число месяца");
                    ServiceLogger.Info("{thread}", $"время запуска: {_params.StartTime}");
                    ServiceLogger.Info("{thread}", $"до следующей запуска: " +
                        $"{awaitDate.Days} day(s) {awaitDate.Hours} hour(s) {awaitDate.Minutes} minute(s) {awaitDate.Seconds} second(s)");
                    // подождём ещё
                    Thread.Sleep(awaitDate);

                    try
                    {
                        Launcher.Start(_params);
                    }
                    catch (Exception e)
                    {
                        ServiceLogger.Info("{error}", "произошла ошибка");
                        ServiceLogger.Error("{error}", e.ToString());
                    }
                }
            }
        }
    }
}
