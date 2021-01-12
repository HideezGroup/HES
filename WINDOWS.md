# Windows deployment

## Requirements
  * Internet Information Services (IIS)
  * Git
  * .NET Core (.NET Core SDK version 3.1)
  * MySQL Server (version 8.0+)

## System Preparation


1. If the web server is not enabled then use the [official guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-3.1#iis-configuration) to enable IIS.

2. Enable WebSockets on IIS according to this [guide](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1#enabling-websockets-on-iis)

If the IIS installation requires a restart, restart the system.

You can perform a simple test by opening a web browser and browsing http://localhost You should see a default IIS page.

3. Download and install [Git](https://git-scm.com/download/win)

4. Download and install .NET Core SDK 3.1:

- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.404-windows-x64-installer) 

- [Windows Hosting Bundle, which includes the .NET Core Runtime and IIS support](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-aspnetcore-3.1.10-windows-hosting-bundle-installer)

You can download the latest versions of this applications. They can be found at https://dotnet.microsoft.com/download/dotnet-core/3.1

**[Note]  You MUST have IIS installed before installing Windows Hosting Bundle** 

5. Download and install 

- [MySQL](https://dev.mysql.com/downloads/installer/)

- [Docs MySQL initial setup](https://dev.mysql.com/doc/refman/8.0/en/mysql-installer.html)

When installing MySQL to run our software, you can select the Server only option

During the installation process, you will be prompted to enter a strong password for the root user. Don't forget this password, we'll need it later

## Getting Started (fresh install)


### 1. Creating MySQL User and Database for the Hideez Enterprise Server

Tthe following lines create a database db, the user user with the password <user_password>. Сhange <user_password> to a strong password, otherwise you may get a password validator error.

in MySQL Command Line Client:


```sql
  ### CREATE DATABASE
  mysql> CREATE DATABASE db;

  ### CREATE USER ACCOUNT
  mysql> CREATE USER 'user'@'127.0.0.1' IDENTIFIED BY '<user_password>';

  ### GRANT PERMISSIONS ON DATABASE
  mysql> GRANT ALL ON db.* TO 'user'@'127.0.0.1';

  ###  RELOAD PRIVILEGES
  mysql> FLUSH PRIVILEGES;
```


### 2. Cloning the HES GitHub repository

run the following commands (step by step) on the command line:

```shell
  > cd C:\
  > md Hideez
  > cd Hideez
  > git clone https://github.com/HideezGroup/HES src
```

### 3. Building the HES from the sources

```shell
  > cd C:\Hideez\src\HES.Web
  > dotnet publish -c release -v d -o "C:\Hideez\HES" --framework netcoreapp3.1 --runtime win-x64 HES.Web.csproj
```
   [Note] Requires internet connectivity to download NuGet packages

 Several warnings may be issued during compilation, this is ok.
### 4. Configuring the HES

Copy appsettings.json to appsettings.Production.json

```shell
  > cd C:\Hideez\HES
  > copy appsettings.json appsettings.Production.json
```

Edit the file C:\Hideez\HES\appsettings.Production.json:

```shell
  > cd C:\Hideez\HES
  > notepad appsettings.Production.json
```

```json
 {
   "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=db;uid=user;pwd=<user_password>"
  },

  "EmailSender": {
    "Host": "<smtp_host>",
    "Port": "<smtp_port>",
    "EnableSSL": true,
    "UserName": "<email_address>",
    "Password": "<email_password>"
  },

  "ServerSettings": {
    "Name": "HES",
    "Url": "<url_to_your_hes_site>"
  },
  
 ...
```


Replace the following settings in this file with your own:

* **user_password** - Password for the user on MySQL server

* **smtp_host** - Host name of your SMTP server (example `smtp.example.com`)
* **smtp_port** - Port number of your SMTP server (example `123`)
* **email_address** - Your email adress (example `user@example.com`)
* **email_password** - Password to access the SMTP server (example `password`)

* **url_to_you_hes_site** - URL of your HES site (example `https://hideez.example.com`)


### 5. Configuring IIS

Create a Self-Signed Certificate for IIS

- Start **IIS Manager**. For information about starting IIS Manager, see https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN
- Click on the name of the server in the Connections column on the left. Double-click on **Server Certificates**.
- In the Actions column on the right, click on **Create Self-Signed Certificate...**
- Enter any *friendly* name and then click **OK**.
- You will now have an IIS Self Signed Certificate valid for 1 year listed under Server Certificates. 

**WARNING! The certificate common name (Issued To) is the server name.**


Add the Web Site

- Start **IIS Manager**. For information about starting IIS Manager, see https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN
- In the **Connections** pane, right-click the **Sites** node in the tree view, and then click 

- (optmal) In **Sites** node 
turn off "Default Web Site"

**Add Web Site**.
- In the **Add Web Site** dialog box, type a *friendly* name for your Web site in the **Web site name** box. "HES" would be a good choice
- If you want to select a different application pool than the one listed in the Application Pool box. In the **Select Application Pool** dialog box, select an application pool from the **Application Pool** list, and then click **OK**.
- In the **Physical path** box, type the *physical path* of the Web site's folder (C:\Hideez\HES), or click the browse button **(...)** to browse the file system to find the folder.
- Select the protocol for the Web site from the **Type** list.
- The default value in the **IP address** box is **All Unassigned**. If you must specify a static IP address for the Web site, type the IP *address* in the **IP address** box.
- Type a port number in the **Port** text box.
- Optionally, type a host header name for the Web site in the **Host Header** box.
- If you do not have to make any changes to the site, and you want the Web site to be immediately available, select the **Start Web site immediately** check box.
- Click **OK**.
- In the **Bindings** pane click "Add" and  Add site Binding with type https for you hostname port 443 and with  you certificate
- Under the server's node, select **Application Pools**.
- Right-click the site's app pool and select **Basic Settings** from the contextual menu.
- In the **Edit Application Pool** window, set the **.NET CLR version** to **No Managed Code**.

Setup is complete. The server should be accessible in a browser at the address `https://<Server_Name>`

**Warning!
Remember that if you use a self-signed certificate, you must enter the server name instead of the domain name. Otherwise, the SSL connection will not work**


## Updating

### 1. Updating the sources from the GitHub repository

```shell
  > cd C:\Hideez\src
  > git pull
```

### 2. Backing up the HES binaries

```shell
  > cd %windir%\system32\inetsrv
  > appcmd stop site /site.name:HES
  > cd C:\Hideez 
  > rename HES HES.old
```

If you get an error that some files are busy, you may need to wait a while (up to 10 minutes)

### 3. Backuping MySQL Database (optional)
The following commands will create a copy of the database in file db.sql in  directory `C:\Hideez\bkp`:
```shell
 > cd C:\Hideez
 > md bkp
 > cd C:\Program Files\MySQL\MySQL Server 8.0\bin
 > mysqldump -u root -p<MySQL_root_password>  db > C:\Hideez\bkp\db.sql 
```
change <MySQL_root_password> with your real password


### 4. Building the HES from the sources

```shell
  > cd C:\Hideez\src\HES.Web
  > dotnet publish -c release -v d -o "C:\Hideez\HES" --framework netcoreapp3.1 --runtime win-x64 HES.Web.csproj
```
  * **[Note]** Requires internet connectivity to download NuGet packages

### 5. Restoring the configuration file

```shell
  > cd C:\Hideez
  > copy HES.old\appsettings.Production.json HES\appsettings.Production.json
 
```


### 6. Starting the HES

```shell
  > cd %windir%\system32\inetsrv
  > appcmd start site /site.name:HES
```

**If something goes wrong, you can restore the HES server using the following commands:**

```shell
> cd %windir%\system32\inetsrv
> appcmd stop site /site.name:HES
> cd C:\Hideez 
> rename HES.old HES
> cd C:\Program Files\MySQL\MySQL Server 8.0\bin
> mysqldump -u root -p<MySQL_root_password> db < C:\Hideez\bkp\db.sql
```
change <MySQL_root_password> with your real password

**After checking that the update was successful and everything works fine, you can delete copies of the database and server:**

```shell
> cd C:\Hideez
> rmdir /s HES.old
> rmdir /s bkp
```
