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
            Launcher.Start(_params);
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
