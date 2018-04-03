using BackUp.Core;
using System;

namespace BackUp.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("creater backup's");
            Console.WriteLine("press any button to run");
            ConfigParams _params = new ConfigParams();

            DateTime now = DateTime.Now;
            DateTime d = now.AddDays(1);
            DateTime deltaTime = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            TimeSpan awaitTime = deltaTime.Subtract(now);
            ServiceLogger.Info("{thread}", $"дата запуска: {_params.Day} число месяца");
            ServiceLogger.Info("{thread}", $"время запуска: {_params.StartTime}");

            if (now.Day != _params.Day)
            {
                //Thread.Sleep(awaitTime);
            }

            int executeWait = MilisecondsToWait(_params.StartTime);
            int hoursWait = executeWait / 1000 / 60 / 60;
            int minutesWait = (executeWait - (hoursWait * 60 * 60 * 1000)) / 1000 / 60;
            int secWait = (executeWait - (hoursWait * 60 * 60 * 1000) - (minutesWait * 60 * 1000)) / 1000;
            ServiceLogger.Info("{thread}", $"импорт будет выполнен через: " +
                            $"{hoursWait} час. {minutesWait} мин. {secWait} сек.");
            //Thread.Sleep(executeWait);
            //Launcher.Start(_params);
            Console.WriteLine("backup's done");
            Console.ReadLine();
        }

        /// <summary>
        /// Время ожидание запуска интеграции
        /// </summary>
        /// <param name="runTime"></param>
        /// <returns></returns>
        private static int MilisecondsToWait(TimeSpan runTime)
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
        private static int MilisecondsToWait(string runTime)
        {
            if (TimeSpan.TryParse(runTime, out TimeSpan _runTime))
            {
                return MilisecondsToWait(_runTime);
            }
            string errorMessage = "ошибка определения времени выполнения";
            ServiceLogger.Error("{error}", errorMessage);
            throw new Exception(errorMessage);
        }
    }
}
