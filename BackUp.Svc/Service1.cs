using BackUp.Core;
using System;
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
        /// Время ожидание запуска интеграции
        /// </summary>
        /// <param name="runTime"></param>
        /// <returns></returns>
        private int MilisecondsToWait(TimeSpan runTime)
        {
            int result = 10000;
            var _currentTime = DateTime.Now.TimeOfDay;

            switch (TimeSpan.Compare(_currentTime, runTime))
            {
                case 1:
                    result = (int)(86400000 - _currentTime.TotalMilliseconds + runTime.TotalMilliseconds);
                    break;
                case 0:
                    result = 0;
                    break;
                case -1:
                    result = (int)(runTime.TotalMilliseconds - _currentTime.TotalMilliseconds);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Время ожидания запуска интеграции
        /// </summary>
        /// <param name="runTime"></param>
        /// <returns></returns>
        private int MilisecondsToWait(string runTime)
        {
            if (TimeSpan.TryParse(runTime, out TimeSpan _runTime))
            {
                return MilisecondsToWait(_runTime);
            }
            string errorMessage = "ошибка определения времени выполнения";
            ServiceLogger.Error("{error}", errorMessage);
            throw new Exception(errorMessage);
        }

        /// <summary>
        /// Создание backup-ов
        /// </summary>
        private void DoBackups()
        {
            ServiceLogger.Info("{thread}", "поехали");
            while (enableService)
            {
                DateTime now = DateTime.Now;
                
                if (!CheckFirstSaturdayOfMonth(now))
                {
                    DateTime deltaTime = new DateTime(now.AddDays(1).Year, 
                        now.AddDays(1).Month, now.AddDays(1).Day, 0, 0, 0);
                    Thread.Sleep(deltaTime.Subtract(now));
                    continue;
                }

                int executeWait = MilisecondsToWait(_params.StartTime);
                int hoursWait = executeWait / 1000 / 60 / 60;
                int minutesWait = (executeWait - (hoursWait * 60 * 60 * 1000)) / 1000 / 60;
                int secWait = (executeWait - (hoursWait * 60 * 60 * 1000) - (minutesWait * 60 * 1000)) / 1000;
                ServiceLogger.Info("{thread}", $"дата запуска: первая суббота месяца");
                ServiceLogger.Info("{thread}", $"время запуска: {_params.StartTime}");
                ServiceLogger.Info("{thread}", $"импорт будет выполнен через: " +
                                $"{hoursWait} час. {minutesWait} мин. {secWait} сек.");

                Thread.Sleep(executeWait);

                try
                {
                    Launcher.Start(_params);
                }
                catch (Exception e)
                {
                    ServiceLogger.Error("{error}", e.ToString());
                }
                // спать день
                Thread.Sleep(86400000);
            }
        }

        /// <summary>
        /// Проверяет является ли сегодняшний день первой субботой месяца
        /// </summary>
        /// <returns></returns>
        private bool CheckFirstSaturdayOfMonth(DateTime now)
        {
            if (now.DayOfWeek == DayOfWeek.Saturday
                && now.AddDays(-7).Month != now.Month)
            {
                return true;
            }
            return false;
        }
    }
}
