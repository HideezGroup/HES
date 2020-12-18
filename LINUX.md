# Deployment on CentOS or Ubuntu Server

## Important Notice
Server installation consists of two parts:
The first part describes the general requirements.
The second part describes the installation already for a specific site, there may be several virtual sites, so this step can be repeated several times

## System Requirements

* Can be installed on a bare metal or virtual server
* 8GB drive
* 2GB RAM
* Option 1: Clean installation of CentOS Linux x86_64 7.6, select "minimal install" option during installation
* Option 2: Clean installation of CentOS Linux x86_64 8.2, select "minimal install" option during installation
* Option 3: Clean installation of Ubuntu Server LTS 18.04
* Option 4: Clean installation of Ubuntu Server LTS 20.04

## Before you start
* You need to know how to create and edit text files in Linux. For example, you can use vim editor. Here you can find a quick start guide on [how to use the Vim editor] (https://www.control-escape.com/linux/editing-vim.html).


# 1. Preparation
  
## 1.1 System Update

*CentOS 7*
```shell
  $ sudo yum update -y
```

*CentOS 8*
```shell
  $ sudo dnf update -y
```

*Ubuntu*
```shell
  $ sudo apt update
  $ sudo apt upgrade -y  
```

Reboot system
```shell
  $ sudo reboot
```


## 1.2 Disable SELinux (CentOS only)

```shell
  $ sudo sed -i 's/SELINUX=enforcing/SELINUX=disabled/' /etc/selinux/config
  $ sudo reboot
```
 To verify that SELinux is disabled, you can type:
```shell
  $ sudo sestatus
SELinux status:                 disabled
```

**Note:** on production servers, usually after installation and verification, you need to re-enable SELinux and configure it accordingly.

## 1.3 Firewall Configuration

To access the server from the network, ports 80 and 443 should be opened:

*CentOS:*
```shell
$ sudo firewall-cmd --zone=public --permanent --add-port=80/tcp
$ sudo firewall-cmd --zone=public --permanent --add-port=443/tcp
$ sudo firewall-cmd --reload
```
*Ubuntu:*
```shell
$ sudo ufw allow 80
$ sudo ufw allow 443
$ sudo ufw enable
```

# 2. Installing Prerequisites
## 2.1 Install git

*CentOS:*
```shell
  $ sudo yum install git -y
```
*Ubuntu (usually already installed):*
```shell
  $ sudo apt install git -y
```


## 2.2 Add Microsoft Package Repository and install .NET Core

*CentOS 7:*
```shell
  $ sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
  $ sudo yum install dotnet-sdk-3.1 -y
```
*CentOS 8:*
```shell
  $ sudo dnf install dotnet-sdk-3.1 -y
```
*Ubuntu 18.04:*
```shell
  $ wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
  $ sudo dpkg -i packages-microsoft-prod.deb
  $ sudo apt update
  $ sudo apt install dotnet-sdk-3.1 -y
```
*Ubuntu 20.04:*
```shell
 $ wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
 $ sudo dpkg -i packages-microsoft-prod.deb
 $ sudo apt-get update
 $ sudo apt install dotnet-sdk-3.1 -y
```

If the installation was successful, the output of the *dotnet* command will look something like this:

```shell
  $ dotnet --version
3.1.404
```

## 2.3 Install MySQL version 8

*CentOS 7:*
```shell
  $ sudo rpm -Uvh https://dev.mysql.com/get/mysql80-community-release-el7-3.noarch.rpm
  $ sudo yum install mysql-server -y
```
*CentOS 8:*
```shell
  $ sudo dnf install mysql-server -y
```
*Ubuntu 18.04:*
```shell
  $ wget -c  https://dev.mysql.com/get/mysql-apt-config_0.8.16-1_all.deb
  $ sudo dpkg -i mysql-apt-config_0.8.16-1_all.deb
```
note:  click "ok" to confirm the server installation

```shell
  $ sudo apt update
  $ sudo apt install mysql-server -y
```
during the installation you will be prompted to enter the mysql user password. Remember this password for future use.


*Ubuntu 20.04:*
```shell
  $ sudo apt install mysql-server -y
```

## 2.4 Install Nginx

*CentOS 7:*
```shell
  $ sudo yum install epel-release -y
  $ sudo yum install nginx -y
  $ sudo systemctl enable nginx
```

*CentOS 8:*
```shell
  $ sudo dnf install nginx -y
  $ sudo systemctl enable nginx
```

*Ubuntu:*
```shell
  $ sudo apt install nginx -y
```


# 3. Configuring MySQL Server and Database
## 3.1 Enable and start MySQL service (CentOS only):

*CentOS:*
```shell
  $ sudo systemctl restart mysqld.service
  $ sudo systemctl enable mysqld.service
```

## 3.2 Verification of the Server availability 

Run the following command to check that the server is running and has the correct version:
```shell
  $ mysql -V
mysql  Ver 8.0.21 for Linux on x86_64 (Source distribution)
```

## 3.3 Setting a permanent root password and MySQL security settings

MySQL expects that your new password should consist of at least 8 characters, contain uppercase and lowercase letters, numbers and special characters (do not forget the password you set, it will come in handy later). After a successful password change, the following questions are recommended to answer "Y":

[Note] In CentOS 7, the default root password can be found using `sudo grep "A temporary password" /var/log/mysqld.log`. 
In CentOS 8, the root password is empty by default. In Ubuntu 18.04 the password was entered during installation of MySQL. In ubuntu 20.04 the password is empty after installation

```shell
  $ sudo mysql_secure_installation
```
Depending on the version of linux, the output of commands may differ slightly. The following is an example for CentOs 7:

```shell
Enter password for user root:

  The existing password for the user account root has expired. Please set a new password.

  New password:
  Re-enter new password:

  Remove anonymous users? (Press y|Y for Yes, any other key for No) : y

  Disallow root login remotely? (Press y|Y for Yes, any other key for No) : y

  Remove test database and access to it? (Press y|Y for Yes, any other key for No) : y

  Reload privilege tables now? (Press y|Y for Yes, any other key for No) : y
```


To verify that everything is correct, you can run
```shell
  $ sudo mysql -h localhost -u root -p
```

After entering password, you will see MySQL console with a prompt:

```shell
  Enter password: 
Welcome to the MySQL monitor.  Commands end with ; or \g.
Your MySQL connection id is 13
Server version: 8.0.21 MySQL Community Server - GPL

Copyright (c) 2000, 2020, Oracle and/or its affiliates. All rights reserved.

Oracle is a registered trademark of Oracle Corporation and/or its
affiliates. Other names may be trademarks of their respective
owners.

Type 'help;' or '\h' for help. Type '\c' to clear the current input statement.

mysql>
```

## 3.4 Creating a MySQL user and database for Hideez Enterprise Server

The following lines create a database `db`, the user `user` with the password `<user_password>`. Сhange `<db_password>` to a strong password, otherwise you may get a password validator error.
 
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

You should remember the user password, it will come in handy later.

To exit from the MySql console, press Ctrl+D.


# 4. Installing the HES server
## 4.1 Downloading the HES repository from the GitHub

```shell
  $ sudo git clone https://github.com/HideezGroup/HES /opt/src/HES
```

## 4.2 Building the HES from source files

```shell
  $ cd /opt/src/HES/HES.Web/
  $ sudo dotnet publish -c release -v d -o "/opt/HES" --framework netcoreapp3.1 --runtime linux-x64 HES.Web.csproj
```
**[Note]** Internet connection required to download NuGet packages

After a while (depending on the computer performance), the compilation process will be completed:

```shell
...
...
...
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:37.35
```

**[Note]** Several warnings may be issued during compilation, this is ok.

Then you need to copy Crypto_linux.dll as follows:

```shell
  $ sudo cp /opt/src/HES/HES.Web/Crypto_linux.dll /opt/HES/Crypto.dll
```

## 4.3 Configuring the HES

Edit the file `/opt/HES/appsettings.json`

```json
  {
   "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=db;uid=user;pwd=<db_password>"
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

* **db_password** - Password for the user on MySQL server

* **smtp_host** - Host name of your SMTP server (example `smtp.example.com`)
* **smtp_port** - Port number of your SMTP server (example `123`)
* **email_address** - Your email adress (example `user@example.com`)
* **email_password** - Password to access the SMTP server (example `password`)

* **url_to_you_hes_site** - URL of your HES site (example `https://hideez.example.com`)


Important note: by default, .Net Core uses ports 5000 and 5001. Therefore, if only one domain 
is running on the server, port numbers can be skipped. But if it is supposed to run a few sites
on one computer, then it is necessary to specify different ports for each site in json file. For example, for a site to listen to ports 6000 and 6001, after "AllowedHosts": "*" add the following (via comma) :
```json
,
 "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:6000"
      },
      "Https": {
        "Url": "https://localhost:6001"
      }
    }
  }

```
After saving the settings file, you can check that the HES is up and running:
```shell
  $ cd /opt/HES
  $ sudo ./HES.Web 
```
If you do not see any errors within 1-2 minutes, it means that the HES has been successfully configured and started.
Press Ctrl+C for exit.

## 4.4 Daemonizing of the Enterprise Server
Copy file `HES.service` to the `/lib/systemd/system/`:
```shell
  $ sudo cp /opt/src/HES/HES.Deploy/HES.service /lib/systemd/system/HES.service
```

Enabling autostart:
```shell
  $ sudo systemctl enable HES.service
  $ sudo systemctl restart HES.service
```

You can verify that HES server is running with the command:
```shell
  $ sudo systemctl status HES
```

The output of the command should be something like this:
```shell
● HES.service - Hideez Enterprise Service
   Loaded: loaded (/usr/lib/systemd/system/HES.service; enabled; vendor preset: disabled)
   Active: active (running) since Wed 2020-03-25 09:05:04 UTC; 34s ago
 Main PID: 2964 (HES.Web)
   CGroup: /system.slice/HES.service
           └─2964 /opt/HES/HES.Web

Mar 25 09:05:04 hesservertest systemd[1]: Started Hideez Enterprise Service.
```

# 4. Configuring Reverse Proxy Server
To access your server from the local network as well as from the Internet, you have to configure a reverse proxy. We will use the Nginx server for this.

## 4.1 Creating a Self-Signed SSL Certificate for Nginx

**Note: in production, you should take care of acquiring a certificate from a certificate authority. For a self-signed certificate, the browser will alert you that site has security issues.**

 When generating a certificate, answer a few simple questions, of which Common Name (CN) will be important - here be the name of your site, in our example it is "hideez.example.com"
```shell
 $ sudo mkdir /etc/nginx/certs
 $ sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/nginx/certs/hes.key -out /etc/nginx/certs/hes.crt
```

```shell
Country Name (2 letter code) [AU]:.
State or Province Name (full name) [Some-State]:.
Locality Name (eg, city) []:.
Organization Name (eg, company) [Internet Widgits Pty Ltd]:.
Organizational Unit Name (eg, section) []:.
Common Name (e.g. server FQDN or YOUR name) []:hideez.example.com
Email Address []:.
```

## 4.1 Restart nginx (CentOS only)
```shell
  $ sudo systemctl restart nginx
```

## 4.2 Check that nginx service is installed and started
```shell
  $ sudo systemctl status nginx
```
  The output would be something like this:
 
```shell
* nginx.service - The nginx HTTP and reverse proxy server
   Loaded: loaded (/usr/lib/systemd/system/nginx.service; enabled; vendor preset: disabled)
   Active: active (running) since Sat 2020-01-25 08:22:56 UTC; 8min ago
  Process: 1702 ExecStart=/usr/sbin/nginx (code=exited, status=0/SUCCESS)
  Process: 1700 ExecStartPre=/usr/sbin/nginx -t (code=exited, status=0/SUCCESS)
  Process: 1699 ExecStartPre=/usr/bin/rm -f /run/nginx.pid (code=exited, status=0/SUCCESS)
 Main PID: 1704 (nginx)
   CGroup: /system.slice/nginx.service
           +-1704 nginx: master process /usr/sbin/nginx
           +-1705 nginx: worker process
```

After performing these steps, the server should already be accessible from the network and respond in the browser to the ip address or its domain name. (http://<ip_or_domain_name\>)

## 4.3 Updating Nginx config

We prepared some Nginx configurations for different versions of Linux and placed them in the HES GitHub repository. You may just copy the corresponding file or you can review and edit it for your needs.

*CentOS 7:*
```shell
  $ sudo cp /opt/src/HES/HES.Deploy/CentOS7/nginx.conf /etc/nginx/nginx.conf
```

*CentOS 8:*
```shell
  $ sudo cp /opt/src/HES/HES.Deploy/CentOS8/nginx.conf /etc/nginx/nginx.conf
```

*Ubuntu 18:*
```shell
  $ sudo cp /opt/src/HES/HES.Deploy/Ubuntu18/nginx.conf /etc/nginx/nginx.conf
```
  
*Ubuntu 20:*
```shell
  $ sudo cp /opt/src/HES/HES.Deploy/Ubuntu20/nginx.conf /etc/nginx/nginx.conf
```

After saving the file, it is recommended to verify nginx settings:
```shell
  $ sudo nginx -t
```

The output should be something like this:
```shell
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
```
Otherwise, you should carefully review the settings and correct the errors.

## 4.4 Disable the Nginx default page
*Ubuntu:*
  
```shell
  $ sudo rm /etc/nginx/sites-enabled/default
```

## 4.5 Restarting Nginx and checking its status

```shell
  $ sudo systemctl restart nginx
  $ sudo systemctl status nginx
  * nginx.service - The nginx HTTP and reverse proxy server
   Loaded: loaded (/usr/lib/systemd/system/nginx.service; enabled; vendor preset: disabled)
   Active: active (running) since Sat 2020-01-25 11:23:46 UTC; 8s ago
  Process: 13093 ExecStart=/usr/sbin/nginx (code=exited, status=0/SUCCESS)
  Process: 13091 ExecStartPre=/usr/sbin/nginx -t (code=exited, status=0/SUCCESS)
  Process: 13089 ExecStartPre=/usr/bin/rm -f /run/nginx.pid (code=exited, status=0/SUCCESS)
 Main PID: 13095 (nginx)
   CGroup: /system.slice/nginx.service
           +-13095 nginx: master process /usr/sbin/nginx
           +-13096 nginx: worker process
```

# 5. Microsoft Active Directory Integration
If you plan to integrate your HES with AD you need to add AD Server's name and IP address to the `/etc/hosts` file, for example:
```conf
192.168.10.75 ad.example.com
```
*Note: the same name should be used on the HES properties page.*


If you use self-signed certificates you need to disable certificate verification. For this edit the file `/etc/ldap/ldap.conf` and add the following line:
```conf
TLS_REQCERT never
```

# 6. Final Verification
After these steps, your server should be up and running. Go to the https://<Name_Of_Domain> in the browser and verify if the site is available.
 
*Note: for a self-signed certificate, it should be a warning that your connection isn't private. Press Advanced/Proceed to ignore the warning.**


# Updating HES

## 1. Stopping HES Service

```shell
  $ sudo systemctl stop HES
```

## 2. Updating sources from GitHub repository

```shell
  $ cd /opt/src/HES
  $ sudo git pull
```

## 3. Back up the MySQL Database
The following command will create a copy of the database in file db.sql in your home directory:
```shell
  $ sudo mysqldump -uroot -p<MySQL_root_password>  db > ~/db.sql
```
change <MySQL_root_password> with your real password

## 4. Back up the HES binaries and the configuration file

```shell
  $ sudo mv /opt/HES /opt/HES.old
```

## 5. Build a new version of the HES from the sources

```shell
  $ cd /opt/src/HES/HES.Web/
  $ sudo dotnet publish -c release -v d -o "/opt/HES" --framework netcoreapp3.1 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES/HES.Web/Crypto_linux.dll /opt/HES/Crypto.dll
```

## 6. Restore the configuration file

```shell
  $ sudo cp /opt/HES.old/appsettings.json /opt/HES/appsettings.json
```

## 7. Restart the HES and check its status

```shell
  $ sudo systemctl restart HES
  $ sudo systemctl status HES

  
  ● HES-hideez.example.com.service - Hideez Enterprise Service
   Loaded: loaded (/usr/lib/systemd/system/HES-hideez.example.com.service; enabled; vendor preset: disabled)
   Active: active (running) since Wed 2020-03-25 10:48:12 UTC; 16s ago
 Main PID: 4657 (HES.Web)
   CGroup: /system.slice/HES.service
           └─4657 /opt/HES/HES.Web

Mar 25 10:48:12 hesservertest systemd[1]: Started Hideez Enterprise Service.
```
