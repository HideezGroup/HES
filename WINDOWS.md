# Windows deployment

## Requirements
  * Internet Information Services (IIS)
  * Git
  * .NET Core (.NET Core SDK version 5.0)
  * MySQL Server (version 8.0+)

## System Preparation


### 1. If the web server is not enabled then use the [official guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-5.0#iis-configuration) to enable IIS.

### 2. Enable WebSockets on IIS according to this [guide](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-5.0#enabling-websockets-on-iis)

If the IIS installation requires a restart, restart the system.

You can perform a simple test by opening a web browser and browsing `http://<you_domain_name>` You should see a default IIS page.

### 3. Download and install [Git](https://git-scm.com/download/win)

### 4. Download and install .NET Core SDK 5.0:

- [.NET Core SDK 5.0](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-5.0.300-windows-x64-installer) 

- [Windows Hosting Bundle, which includes the .NET Core Runtime and IIS support](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-aspnetcore-5.0.6-windows-hosting-bundle-installer)

You can download the latest versions of this applications. They can be found at https://dotnet.microsoft.com/download/dotnet-core/5.0

**[Note]  You MUST have IIS installed before installing Windows Hosting Bundle** 

### 5. Download and install 

- [MySQL](https://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-8.0.25.0.msi)

Alternatively, You can also go to https://dev.mysql.com/downloads/installer and  download the latest versions of this applications 
 
 You can read the documentation for installing MySQL at [this](https://dev.mysql.com/doc/refman/8.0/en/mysql-installer.html) address.

When installing MySQL to run our software, you can select the Server only option

During installation, you may need an additional Microsoft Visual C ++ 2019 Redistributable Package. Agree,  and perform the installation

Also, during the installation process, you will be prompted to enter a strong password for the root user. Don't forget this password, we'll need it later

If the server configuration step was skipped during the installation, you can do so after installation by clicking Start / My SQL Installer Community and selecting the "Reconfigure" action


## Getting Started (fresh install)


### 1. Creating MySQL User and Database for the Hideez Enterprise Server


Start the MySQL Command Line Client (Start/MySQL 8.0 Command Line Client), enter the MySQL root user password.

The following lines create a database db, the user user with the password `<user_password>`. Сhange `<user_password>` to a strong password, otherwise you may get a password validator error.



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

run the following command on the command line:

```shell
> git clone https://github.com/HideezGroup/HES  C:\Hideez\src
```

this will create a copy of our repository in the `C:\Hideez\src` directory of your computer

### 3. Building the HES from the sources

```shell
  > cd C:\Hideez\src\HES.Web
  > dotnet publish -c release -v d -o C:\Hideez\HES --runtime win-x64 HES.Web.csproj
```

**Note:** Internet connection required to download NuGet packages

After a while (depending on the computer performance), the compilation process will be completed:

```shell
...
...
...
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:37.35
```

**Note:** Several warnings may be issued during compilation, this is ok.



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

  "Fido2": {
    "ServerDomain":"<your_domain_name>",
    "ServerName": "HES",
    "Origin": "https://<your_domain_name>",
    "TimestampDriftTolerance": 300000,
    "MDSAccessKey": null
  },

  "ServerSettings": {
    "Name": "HES",
    "Url": "https://<your_domain_name>"
  },
  
 ...
```
Replace the following settings in this file with your own:
* **`<user_password>`** - Password for the user on MySQL server

* **`<smtp_host>`** - Host name of your SMTP server (example: `smtp.example.com`)
* **`<smtp_port>`** - Port number of your SMTP server (example: `123`)
* **`<email_address>`** - Your email adress (example: `user@example.com`)
* **`<email_password>`** - Password to access the SMTP server (example: `password`)
* **`<you_domain_name>`** - you  fully qualified domain name (FQDN) of your HES site (example: `hideez.example.com`)


### 5. Configuring IIS

#### 5.1 Create a Self-Signed Certificate for IIS

**Note 1:**

**In production, you should take care of acquiring a certificate from a certificate authority. For a self-signed certificate, the browser will alert you that site has security issues.**

**Note 2:**

**if you create a certificate using IIS, CN will be the same as your server name (in the properties of your server)**

- Start **IIS Manager**. For information about starting IIS Manager, see https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN
- Click on the name of the server in the Connections column on the left. Double-click on **Server Certificates**.
- In the Actions column on the right, click on **Create Self-Signed Certificate...**
- Enter any *friendly* name (for example HES) and then click **OK**.
- You will now have an IIS Self Signed Certificate valid for 1 year listed under Server Certificates.

you can click on the created certificate and see its properties


**WARNING! The certificate common name (CN) (Issued To) is the server name. Name of Certificate is not  CN !**

An alternative way to create a certificate is to use the cmdlet `New-SelfSignedCertificate` in PowerShell, which can be used to specify the required CN:

``` 
New-SelfSignedCertificate -DnsName <you_domain_name>  -FriendlyName <friendly_name>
```
for example:
``` 
New-SelfSignedCertificate -DnsName hideez.example.com -FriendlyName HES
```

#### 5.2 Add the Web Site



- Start **IIS Manager**. For information about starting IIS Manager, see https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN
- In the **Connections** pane, right-click the **Sites** node in the tree view, and then click **Add Web Site**.
- In the **Add Web Site** dialog box, type a *friendly* name for your Web site in the **Web site name** box. "HES" would be a good choice
- In the **Physical path** box, type the *physical path* of the Web site's folder (C:\Hideez\HES), or click the browse button **(...)** to browse the file system to find the folder.
- If you want to select a different application pool than the one listed in the Application Pool box. In the **Select Application Pool** dialog box, select an application pool from the **Application Pool** list, and then click **OK**.

- The default value in the **IP address** box is **All Unassigned**. If you must specify a static IP address for the Web site, type the IP *address* in the **IP address** box.

- Optionally, type a host header name for the Web site in the **Host Header** box.
- If you do not have to make any changes to the site, and you want the Web site to be immediately available, select the **Start Web site immediately** check box.
- Click **OK**.
- In the **Bindings** pane click "Add" and  Add site Binding with type https for you hostname port 443 and with  you certificate  (In the **SSL cerificate** drop-down menu, select your certificate)
- Under the server's node, select **Application Pools**.
- Right-click the site's app pool and select **Basic Settings** from the contextual menu.
- In the **Edit Application Pool** window, set the **.NET CLR version** to **No Managed Code**.
- (optmal) In **Sites** node 
turn off "Default Web Site"


Setup is complete. The server should be accessible in a browser at the address 
`(http://<you_domain_name>`).


You must use your domain name to access the HES server (e.g. `http://hideez.example.com`).


**Warning! The CN (common name) of your certificate must match your domain.**

**Remember that if you use a self-signed certificate, you must enter the server name instead of the domain name. Otherwise, the SSL connection will not work**


## Updating

### 1. Updating the sources from the GitHub repository

```shell
  > cd C:\Hideez\src
  > git pull
```

### 2. Backing up the HES binaries

- Stop the site using the IIS console
- rename old binary folder:

```shell
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
  > dotnet publish -c release -v d -o "C:\Hideez\HES" --runtime win-x64 HES.Web.csproj
```
  * **[Note]** Requires internet connectivity to download NuGet packages

### 5. Restoring the configuration file


```shell
  > cd C:\Hideez
  > copy HES.old\appsettings.Production.json HES\appsettings.Production.json
 
```


### 6. Starting the HES

Start the site using the IIS console


**If something goes wrong, you can restore the HES server using the following commands:**


Stop the site using the IIS console, then:

```shell
> cd C:\Hideez 
> rename HES.old HES
> cd C:\Program Files\MySQL\MySQL Server 8.0\bin
> mysql -u root -p<MySQL_root_password> db < C:\Hideez\bkp\db.sql
```
change <MySQL_root_password> with your real password

And start the site using the IIS console



**After checking that the update was successful and everything works fine, you can delete copies of the database and server:**

```shell
> cd C:\Hideez
> rmdir /s HES.old
> rmdir /s bkp
```


## Possible problems and solutions


**Problem: When activating the data protection, HES requires the data protection password to be entered after each startup**

**Solution:**

The HES server stores the data protection password in RAM until it restarts. After that, HES will ask for the password again each time. To save the data protection password between HES reboots, it is possible to save this password in the file `appsettings.Production.json`:

```json
 ...
  "DataProtection": {
    "Password": "<your_data_protection_password>"
  }, 
 ...
```

After editing `appsettings.Production.json`, you need to restart HES.

To change the data protection password login to HES and go to
"Settings" - "Data Protection" - "Change Password". After this, you need to update the password in the `appsettings.Production.json` also.

**Problem:  When activating data protection, the administrator's e-mail often receives messages, such as: "Your Hideez Enterprise Server has been restarted. Please activate the data protection on the server by clicking this button:"**

**Solution:**
Typically, this problem is due to the fact that IIS stops your HES server if the application doesn't receive any request in the specified time period (default for 20 minutes)

Step 1 Cancel Idle Time-out:

- Go into the IIS Manager
- Click on Application Pools (on the left)
- Right click on your application pool
- Select `Advanced Settings`
- Change the value of `Idle Time-out (minutes)` from 20 to 0
- Click "OK"

Step 2 Cancel IIS Recycling :

- Go into the IIS Manager
- Click on Application Pools (on the left)
- Right click on your application pool
- Select `Recycling...`
- Uncheck "Regular time intervals (in minutes)" 
- Click "Next"
- Click "Finish"

Step 3 Restart the IIS
