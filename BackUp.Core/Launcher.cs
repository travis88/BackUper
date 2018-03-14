﻿using System;
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
        /// Параметры 
        /// </summary>
        private static ConfigParams Params { get; set; }

        /// <summary>
        /// Запускает основную логику
        /// </summary>
        public static void Start(ConfigParams _params)
        {
            Params = _params;
            messageBody = null;
            if (CreateBackup())
            {
                messageBody = "<p><strong>Создание BackUp-ов chuvashia.com завершилось успешно</strong></p>";
                CopyBackups();
            }
            else
            {
                messageBody = "<p><strong>Создание BackUp-ов chuvashia.com завершилось с ошибкой</strong></p>";
            }

            SendEmail(messageBody);
        }

        /// <summary>
        /// Выполняет скрипт по созданию backUp
        /// </summary>
        private static bool CreateBackup()
        {
            bool result = false;

            try
            {
                string[] listOfDirectories = GetDirectoryListFromServer();
                foreach (string folder in Params.Folders)
                {
                    bool isExists = listOfDirectories.Contains(folder);
                    if (!isExists)
                    {
                        CreateDirectoryOnServer(folder);
                    }
                }

                ServiceLogger.Info("{work}", "запуск скрипта по созданию backup");
                using (var db = new DbModel(connection))
                {
                    if (db.Command != null)
                    {
                        db.Command.CommandTimeout = 1800000;
                        db.backup_service(Params.StrFolders);
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
        private static void SendEmail(string body)
        {
            try
            {
                ServiceLogger.Info("{work}", "рассылка оповещения");
                foreach (var emailTo in Params.EmailToAddresses)
                {
                    if (!String.IsNullOrEmpty(emailTo))
                    {
                        var from = new MailAddress(Params.EmailFromAddress, Params.EmailFromName);
                        var to = new MailAddress(emailTo);

                        var message = new MailMessage(from, to);
                        message.Subject = "BackUp chuvashia.com";
                        message.Body = body;
                        message.IsBodyHtml = true;

                        var smtp = new SmtpClient(Params.EmailHost, Params.EmailPort);
                        smtp.Credentials = new NetworkCredential(Params.EmailFromAddress, Params.EmailPassword);
                        smtp.EnableSsl = Params.EmailEnableSsl;

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
        private static void CopyBackups()
        {
            messageBody += "<p>Список скопированных backup-ов:</p><p>";
            string dateDir = DateTime.Now.ToString("yyyyMMdd");

            foreach (string fold in Params.Folders)
            {
                try
                {
                    string pathTo = Params.PathTo + fold + @"\" + dateDir;
                    bool isExist = Directory.Exists(pathTo);
                    if (!isExist)
                    {
                        Directory.CreateDirectory(pathTo);
                    }

                    DownloadFileFTP(fold, fold + "_" + dateDir + ".bak", pathTo);
                    
                    messageBody += fold + "; ";
                    ServiceLogger.Info("{work}", String.Format("скачивание backup-а {0} проведено", fold));
                }
                catch (Exception e)
                {
                    ServiceLogger.Info("{error}", String.Format("при скачивании backup-а {0} произошла ошибка", fold));
                    ServiceLogger.Error("{error}", e.ToString());
                }
            }
            messageBody += "</p>";
        }

        /// <summary>
        /// Создаёт директорию на сервере
        /// </summary>
        /// <returns></returns>
        private static void CreateDirectoryOnServer(string folderName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://chuvashia.com/" + folderName);
            request.Credentials = new NetworkCredential(Params.FtpUserName, Params.FtpUserPassword);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (var response = (FtpWebResponse)request.GetResponse())
            {
                ServiceLogger.Info("{work}", String.Format("создана папка: {0} статус: {1}",
                                                            folderName, response.StatusCode.ToString()));
            }
        }

        /// <summary>
        /// Возвращает список директорий
        /// </summary>
        /// <returns></returns>
        private static string[] GetDirectoryListFromServer()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://chuvashia.com/");
            request.Credentials = new NetworkCredential(Params.FtpUserName, Params.FtpUserPassword);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            using (FtpWebResponse directoryListResponse = (FtpWebResponse)request.GetResponse())
            {
                using (StreamReader directoryListResponseReader = new StreamReader(directoryListResponse.GetResponseStream()))
                {
                    string responseString = directoryListResponseReader.ReadToEnd();
                    string[] results = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    return results;
                }
            }
        }

        /// <summary>
        /// Скачивает файл с сервера
        /// </summary>
        private static void DownloadFileFTP(string folderName, string fileName, string pathTo)
        {
            string ftpFullPath = "ftp://chuvashia.com/" + folderName + "/" + fileName;
            string downloadFilePath = Path.Combine(pathTo, fileName);
            
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFullPath);
            request.Credentials = new NetworkCredential(Params.FtpUserName, Params.FtpUserPassword);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (Stream ftpStream = request.GetResponse().GetResponseStream())
            using (Stream fileStream = File.Create(downloadFilePath))
            {
                ftpStream.CopyTo(fileStream);
            }
        }
    }
}
