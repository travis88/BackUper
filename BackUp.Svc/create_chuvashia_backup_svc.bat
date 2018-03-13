

sc create chuv_backup_svc binPath= "D:\projects\ChuvashiaBackUp\BackUp.Svc\bin\Release\BackUp.Svc.exe" start= auto DisplayName= chuv_backup_svc
sc start chuv_backup_svc

pause