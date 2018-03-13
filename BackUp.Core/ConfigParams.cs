using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUp.Core
{
    /// <summary>
    /// Параметры из конфига
    /// </summary>
    public class ConfigParams
    {
        /// <summary>
        /// Время запуска
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// День запуска
        /// </summary>
        public int Day { get; set; }
        
        /// <summary>
        /// Месяцы запуска
        /// </summary>
        public int[] Months { get; set; }

        /// <summary>
        /// Путь для скачивания
        /// </summary>
        public string PathFrom { get; set; }

        /// <summary>
        /// Путь для сохранения
        /// </summary>
        public string PathTo { get; set; }

        /// <summary>
        /// Фтп сервер
        /// </summary>
        public string FtpServer { get; set; }

        /// <summary>
        /// Логин для входа по фтп
        /// </summary>
        public string FtpUserName { get; set; }

        /// <summary>
        /// Пароль для входа по фтп
        /// </summary>
        public string FtpUserPassword { get; set; }

        /// <summary>
        /// Список директорий
        /// </summary>
        public string StrFolders { get; set; }

        /// <summary>
        /// Директории с backup-ами
        /// </summary>
        public string[] Folders { get; set; }

        /// <summary>
        /// email отправителя
        /// </summary>
        public string EmailFromAddress { get; set; }

        /// <summary>
        /// email получателя
        /// </summary>
        public string[] EmailToAddresses { get; set; }

        /// <summary>
        /// Имя отправителя email
        /// </summary>
        public string EmailFromName { get; set; }

        /// <summary>
        /// Пароль от почты 
        /// </summary>
        public string EmailPassword { get; set; }

        /// <summary>
        /// Хост почты
        /// </summary>
        public string EmailHost { get; set; }
        
        /// <summary>
        /// Порт для почты
        /// </summary>
        public int EmailPort { get; set; }

        /// <summary>
        /// Разрешить SSL
        /// </summary>
        public bool EmailEnableSsl { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public ConfigParams()
        {
            StartTime = System.Configuration.ConfigurationManager.AppSettings["Backup.StartTime"];
            Day = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["Backup.Day"]);
            Months = System.Configuration.ConfigurationManager.AppSettings["Backup.Months"].Split(';').Where(w => !String.IsNullOrEmpty(w)).Select(s => Int32.Parse(s)).ToArray();
            PathFrom = System.Configuration.ConfigurationManager.AppSettings["Backup.PathFrom"];
            PathTo = System.Configuration.ConfigurationManager.AppSettings["Backup.PathTo"];
            FtpServer = System.Configuration.ConfigurationManager.AppSettings["Backup.FtpServer"];
            FtpUserName = System.Configuration.ConfigurationManager.AppSettings["Backup.Username"];
            FtpUserPassword = System.Configuration.ConfigurationManager.AppSettings["Backup.UserPassword"];
            StrFolders = System.Configuration.ConfigurationManager.AppSettings["Backup.Folders"].Replace("\n", "").Replace("\r", "").Replace(" ", "");
            Folders = StrFolders.Split(';').Where(w => !String.IsNullOrEmpty(w)).Select(s => s).ToArray();
            EmailFromAddress = System.Configuration.ConfigurationManager.AppSettings["Email.From.Address"];
            EmailToAddresses = System.Configuration.ConfigurationManager.AppSettings["Email.To.Address"].Split(';');
            EmailFromName = System.Configuration.ConfigurationManager.AppSettings["Email.From.Name"];
            EmailPassword = System.Configuration.ConfigurationManager.AppSettings["Email.Password"];
            EmailHost = System.Configuration.ConfigurationManager.AppSettings["Email.Host"];
            EmailPort = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["Email.Port"]);
            EmailEnableSsl = Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["Email.EnableSSL"]);
        }
    }
}
