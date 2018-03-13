using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BackUp.Core
{
    /// <summary>
    /// Запускатель 
    /// </summary>
    public static class Launcher
    {
        /// <summary>
        /// строка подключения
        /// </summary>
        private static string connection = "defaultConnection";

        /// <summary>
        /// Текст письма
        /// </summary>
        private static string messageBody;

        /// <summary>
        /// Запускает основную логику
        /// </summary>
        public static void Start(ConfigParams _params)
        {
            messageBody = null;
            if (CreateBackup(_params))
            {
                messageBody = "<p><strong>Создание BackUp-ов chuvashia.com завершилось успешно</strong></p>";
                CopyBackups(_params);
            }
            else
            {
                messageBody = "<p><strong>Создание BackUp-ов chuvashia.com завершилось с ошибкой</strong></p>";
            }

            SendEmail(_params, messageBody);
        }

        /// <summary>
        /// Выполняет скрипт по созданию backUp
        /// </summary>
        private static bool CreateBackup(ConfigParams _params)
        {
            bool result = false;

            foreach (string fold in _params.Folders)
            {
                bool isExist = Directory.Exists(_params.PathFrom + fold);
                if (!isExist)
                {
                    Directory.CreateDirectory(_params.PathFrom + fold);
                }
            }

            try
            {
                ServiceLogger.Info("{work}", "запуск скрипта по созданию backup");
                using (var db = new DbModel(connection))
                {
                    if (db.Command != null)
                    {
                        db.Command.CommandTimeout = 1800000;
                        db.backup_service(_params.StrFolders);
                        result = true;
                    }
                }
                ServiceLogger.Info("{work}", "создание backup завершено");
            }
            catch (Exception e)
            {
                ServiceLogger.Info("{error}", "скрипт по созданию backup завершился ошибкой");
                ServiceLogger.Error("{error}", e.ToString());
            }

            return result;
        }

        /// <summary>
        /// Рассылает сообщения на email
        /// </summary>
        /// <param name="_params"></param>
        /// <returns></returns>
        private static void SendEmail(ConfigParams _params, string body)
        {
            try
            {
                ServiceLogger.Info("{work}", "рассылка оповещения");
                foreach (var emailTo in _params.EmailToAddresses)
                {
                    if (!String.IsNullOrEmpty(emailTo))
                    {
                        var from = new MailAddress(_params.EmailFromAddress, _params.EmailFromName);
                        var to = new MailAddress(emailTo);

                        var message = new MailMessage(from, to);
                        message.Subject = "BackUp chuvashia.com";
                        message.Body = body;
                        message.IsBodyHtml = true;

                        var smtp = new SmtpClient(_params.EmailHost, _params.EmailPort);
                        smtp.Credentials = new NetworkCredential(_params.EmailFromAddress, _params.EmailPassword);
                        smtp.EnableSsl = _params.EmailEnableSsl;

                        smtp.Send(message);
                    }
                }
                ServiceLogger.Info("{work}", "рассылка оповещения проведена");
            }
            catch (Exception e)
            {
                ServiceLogger.Info("{error}", "рассылка оповещений завершилась ошибкой");
                ServiceLogger.Error("{error}", e.ToString());
            }
        }

        /// <summary>
        /// Копирует backup-ы с chuvashia.com на машину
        /// </summary>
        private static void CopyBackups(ConfigParams _params)
        {
            messageBody += "<p>Список скопированных backup-ов:</p><p>";
            string dateDir = DateTime.Now.ToString("yyyyMMdd");

            foreach(string fold in _params.Folders)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(_params.PathFrom + fold);
                    FileInfo file = di.GetFiles("*.bak").OrderByDescending(p => p.CreationTime).FirstOrDefault();

                    string pathTo = _params.PathTo + fold + @"\" + dateDir;

                    bool isExist = Directory.Exists(pathTo);
                    if (!isExist)
                    {
                        Directory.CreateDirectory(pathTo);
                    }

                    file.CopyTo(Path.Combine(pathTo, file.Name));

                    messageBody += fold + "; ";
                    ServiceLogger.Info("{work}", String.Format("копирование backup-а {0} проведено", fold));
                }
                catch(Exception e)
                {
                    ServiceLogger.Info("{error}", String.Format("при копировании backup-а {0} произошла ошибка", fold));
                    ServiceLogger.Error("{error}", e.ToString());
                }
            }
            messageBody += "</p>";
        }
    }
}
