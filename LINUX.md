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
* Option 2: Clean installation of CentOS Linux x86_64 8.1, select "minimal install" option during installation
* Option 3: Clean installation of Ubuntu Server LTS 18.04
* Option 4: Clean installation of Ubuntu Server LTS 20.04

## Before you start
* You need to know how to create and edit text files in Linux. For example, you can use vim editor. Here you can find a quick start guide on [how to use the Vim editor] (https://www.control-escape.com/linux/editing-vim.html).



# 1. Preparation (Run once)
  
## 1.1 System Update
  
  (if not yet updated)

*CentOS*
```shell
  $ sudo yum update -y
  $ sudo reboot
```
*Ubuntu*
```shell
  $ sudo apt update
  $ sudo apt upgrade -y
  $ sudo reboot
```

## 1.2 Disable SELinux (CentOS only)

```shell
  $ sudo sed -i 's/SELINUX=enforcing/SELINUX=disabled/' /etc/selinux/config
  $ sudo reboot
```

## 1.3 Install git

*CentOS*
```shell
  $ sudo yum install git -y
```
*Ubuntu*
```shell
  $ sudo apt install git -y
 ```
## 1.4 Download HES repository from GitHub

```shell
  $ sudo git clone https://github.com/HideezGroup/HES /opt/src/HES
```

## 1.5 Add Microsoft Package Repository and install .NET Core

*CentOS 7*
```shell
  $ sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
  $ sudo yum install dotnet-sdk-3.1 -y
```
*CentOS 8*
```shell
  $ sudo dnf install dotnet-sdk-3.1 -y
```
*Ubuntu 18.04*
```shell
  $ wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
  $ sudo dpkg -i packages-microsoft-prod.deb
  $ sudo apt update
  $ sudo apt install dotnet-sdk-3.1 -y
```
*Ubuntu 20.04*
```shell
 $ wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
 $ sudo dpkg -i packages-microsoft-prod.deb
 $ sudo apt-get update
 $ sudo apt install dotnet-sdk-3.1 -y
```

If the installation was successful, the output of the *dotnet* command will look something like this:

```shell
  $ dotnet --version
3.1.301
```

## 1.6 Install MySQL version 8:

*CentOS 7*
```shell
  $ sudo rpm -Uvh https://dev.mysql.com/get/mysql80-community-release-el7-3.noarch.rpm
  $ sudo yum install mysql-server -y
```
*CentOS 8*
```shell
  $ sudo dnf install mysql-server -y
```
*Ubuntu 18.04*
```shell
  $ wget -c https://dev.mysql.com/get/mysql-apt-config_0.8.14-1_all.deb
  $ sudo dpkg -i mysql-apt-config_0.8.14-1_all.deb
  $ sudo apt update
  $ sudo apt install mysql-server -y
```
*Ubuntu 20.04*
```shell
  $ sudo apt install mysql-server -y
```

### 1.6.1 Enable and start MySQL service (CentOS only):

*CentOS*
```shell
  $ sudo systemctl restart mysqld.service
  $ sudo systemctl enable mysqld.service
```

### 1.6.2 After installing MySQL, if everything went well, you can check the version of the program:

```shell
  $ mysql -V
mysql  Ver 8.0.17 for Linux on x86_64 (Source Distribution)
```

### 1.6.3 Setting a permanent real root password and MySQL security settings

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
  $ mysql -h localhost -u root -p
```

After entering password, you will see MySQL console with a prompt:

```shell
  Enter password: 
Welcome to the MySQL monitor.  Commands end with ; or \g.
Your MySQL connection id is 13
Server version: 8.0.19 MySQL Community Server - GPL

Copyright (c) 2000, 2020, Oracle and/or its affiliates. All rights reserved.

Oracle is a registered trademark of Oracle Corporation and/or its
affiliates. Other names may be trademarks of their respective
owners.

Type 'help;' or '\h' for help. Type '\c' to clear the current input statement.

mysql>
```

To exit from mySql console, press Ctrl+D.

## 1.7 Install Nginx

*CentOS 7*
```shell
  $ sudo yum install epel-release -y
  $ sudo yum install nginx -y
  $ sudo systemctl enable nginx
```

*CentOS 8*
```shell
  $ sudo dnf install nginx -y
  $ sudo systemctl enable nginx
```

*Ubuntu*
```shell
  $ sudo apt install nginx -y
```

### 1.7.1 Modifying nginx.conf
Open file /etc/nginx/nginx.conf and add the following text to the **http** section of the file. 

```conf
...
    map $http_upgrade $connection_upgrade {
                default Upgrade;
                ''      close;
    }
...
```

### 1.7.2 Restart nginx
```shell
  $ sudo systemctl restart nginx
```

### 1.7.3 Check that nginx service is installed and started
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

## 1.8 Firewall Configuration

To access the server from the network, ports 22, 80, and 443 should be opened:
*CentOS*
```shell
$ sudo firewall-cmd --zone=public --permanent --add-port=22/tcp
$ sudo firewall-cmd --zone=public --permanent --add-port=80/tcp
$ sudo firewall-cmd --zone=public --permanent --add-port=443/tcp
$ sudo firewall-cmd --reload
```
*Ubuntu*
```shell
$ sudo ufw allow 22
$ sudo ufw allow 80
$ sudo ufw allow 443
$ sudo ufw enable
```

After performing these steps, the server should already be accessible from the network and respond in the browser to the ip address or its domain name. (http://<ip_or_domain_name>)

Now the preparation is complete.

# 2. Installing the HES server (can be repeated for each new virtual domain)

## 2.1 Creating a MySQL user and database for Hideez Enterprise Server

**Starting the MySQL Server Console**

```shell
  mysql -h localhost -u root -p
```

```sql
  ### CREATE DATABASE
  mysql> CREATE DATABASE <your_db>;

  ### CREATE USER ACCOUNT
  mysql> CREATE USER '<your_user>'@'127.0.0.1' IDENTIFIED BY '<your_secret>';

  ### GRANT PERMISSIONS ON DATABASE
  mysql> GRANT ALL ON <your_db>.* TO '<your_user>'@'127.0.0.1';
 
  ###  RELOAD PRIVILEGES
  mysql> FLUSH PRIVILEGES;
```
You should remember database name, username and password, they will come in handy later.

(Press Ctrl + D to exit the MySQL console)

## 2.2 Installing Hideez Enterprise Server from source

here is an example for the case when our site will be in folder "/opt/HES/<Name_Of_Domain>". Replace <Name_Of_Domain> with the domain name you chose.

```shell
  $ cd /opt/src/HES/HES.Web/
  $ sudo mkdir -p /opt/HES/<Name_Of_Domain>
  $ sudo dotnet publish -c release -v d -o "/opt/HES/<Name_Of_Domain>" --framework netcoreapp3.1 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES/HES.Web/Crypto_linux.dll /opt/HES/<Name_Of_Domain>/Crypto.dll
```
**[Note]** Internet connection required to download NuGet packages


## 2.3 Hideez Enterprise Server Configuration

Edit the file `/opt/HES/<Name_Of_Domain>/appsettings.json`

```json
  {
  "ConnectionStrings": {
    "DefaultConnection": "server=<mysql_server>;port=<mysql_port>;database=<your_db>;uid=<your_user>;pwd=<your_secret>"
  },

  "EmailSender": {
    "Host": "<email_host>",
    "Port": "<email_port>",
    "EnableSSL": true,
    "UserName": "<your_email_name>",
    "Password": "<your_email_password>"
  },

  "ServerSettings": {
    "Name": "HES",
    "Url": "<url_to_your_hes_site>"
  },
  
  "DataProtection": {
    "Password": "<data_protection_password>"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Information"
    }
  },

  "AllowedHosts": "*"
```

* **mysql_server** - MySQL server ip address (example `127.0.0.1`)
* **mysql_port** - MySQL server port (example `3306`)
* **your_db** - The name of your database on the MySQL server (example `hes`)
* **your_user** - MySQL database username (example `admin`)
* **your_secret** - Password from database user on MySQL server (example `password`)
* **email_host** - Host your email server (example `smtp.example.com`)
* **email_port** - Port your email server (example `123`)
* **your_email_name** - Your email name (example `user@example.com`)
* **your_email_password** - Your email name (example `password`)
* **protection_password** - Your password for database encryption (example `password`)



Important note. By default, .net Core uses ports 5000 and 5001. Therefore, if only one domain 
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
After saving the settings file, you can check that HES server is up and running:
```shell
  $ cd /opt/HES/<Name_Of_Domain>
  $ sudo ./HES.Web 
```
If you do not see any errors, this means that HES server is successfully configured and started.
Press Ctrl+C for exit

## 2.4 Daemonizing of Enterprise Server

Create the file `/lib/systemd/system/HES-<Name_Of_Domain>.service`  with the following content

```conf
[Unit]
  Description=<Name_Of_Domain> Hideez Enterprise Service

[Service]

  User=root
  Group=root

  WorkingDirectory=/opt/HES/<Name_Of_Domain>
  ExecStart=/opt/HES/<Name_Of_Domain>/HES.Web 
  Restart=on-failure
  ExecReload=/bin/kill -HUP $MAINPID
  KillMode=process
  # SyslogIdentifier=dotnet-sample-service
  # PrivateTmp=true

[Install]
  WantedBy=multi-user.target
```

**enabling autostart (using hideez.example.com as an example)**

```shell
  $ sudo systemctl enable HES-<Name_Of_Domain>.service
  $ sudo systemctl restart HES-<Name_Of_Domain>.service
```

You can verify that HES server is running with the command

```shell
  $ sudo systemctl status HES-<Name_Of_Domain>.service
```

The output of the command should be something like this:

```shell
● HES-hideez.example.com.service - hideez.example.com Hideez Enterprise Service
   Loaded: loaded (/usr/lib/systemd/system/HES-hideez.example.com.service; enabled; vendor preset: disabled)
   Active: active (running) since Wed 2020-03-25 09:05:04 UTC; 34s ago
 Main PID: 2964 (HES.Web)
   CGroup: /system.slice/HES-hideez.example.com.service
           └─2964 /opt/HES/hideez.example.com/HES.Web

Mar 25 09:05:04 hesservertest systemd[1]: Started hideez.example.com Hideez Enterprise Service.
```

## 2.5 Reverse proxy configuration
To access your server from the local network as well as from the Internet, you have to configure a reverse proxy.

 Creating a Self-Signed SSL Certificate for Nginx
 Note For a "real" site, you should take care of acquiring a certificate from a certificate authority.
 For self-signed certificate, browser will alert you that site has security issues.
 Replace <Name_Of_Domain> with you domain name
 (when generating a certificate, answer a few simple questions)
```shell
 $ sudo mkdir /etc/nginx/certs
 $ sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/nginx/certs/<Name_Of_Domain>.key -out /etc/nginx/certs/<Name_Of_Domain>.crt
```

** Virtual site configuration on Nginx reverse proxy **

You can configure virtual sites in the **http** section of /etc/nginx/nginx.conf or by creating separate configuration files. In this example, we will add new sections to /etc/nginx/nginx.conf:

```conf
 # redirect all traffic to https
 server {
          server_name <Name_Of_Domain>;
          listen 80;
          listen [::]:80;
          if ($host = <Name_Of_Domain>) {
                return 301 https://$host$request_uri;
          }
          return 404;
    }

  server {
          server_name <Name_Of_Domain>;
          listen [::]:443 ssl ;
          listen 443 ssl;
          ssl_certificate "certs/<Name_Of_Domain>.crt";
          ssl_certificate_key "certs/<Name_Of_Domain>.key";

          location / {
                 proxy_pass https://localhost:5001;
                 proxy_http_version 1.1;
                 proxy_set_header Upgrade $http_upgrade;
                 proxy_set_header  Connection $connection_upgrade;
                 proxy_set_header Host $host;
                 proxy_cache_bypass $http_upgrade;
                 proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                 proxy_set_header X-Forwarded-Proto $scheme;
          }

    }
```

*  Replace <Name_Of_Domain> with you domain name
*  Port numbers should match the settings specified in /opt/HES/<Name_Of_Domain>/appsettings.json (defauls is 5000 for http and 5001 for https)
*  note we added a map directive with the "connection_upgrade" variable declaration during the initial nginx configuration


After saving the file, it is recommended to check nginx settings:
```shell
  $ sudo nginx -t
```
The output should be something like this:
```shell
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
```
Otherwise, you should carefully review the settings and correct the errors.

**Restarting Nginx and checking its status**

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

## 2.6 Final verification
After these steps, your server should be up and running. Go to the https://<Name_Of_Domain> in the browser and verify if the site is available. 
Note: for a self-signed certificate, it should be a warning that your connection isn't private. Press Advanced/Proceed to ignore the warning. 


## Updating HES

### 1. Updating sources from GitHub repository

```shell
  $ cd /opt/src/HES
  $ sudo git pull
```

### 2. Back up MySQL Database (optional)

```shell
  $ sudo mkdir /opt/backups
  $ cd /opt/backups
  $ sudo mysqldump -u <your_user> -p<your_secret> <your_db> | gzip -c > <your_db>.sql.gz
```

### 3. Back up Hideez Enterprise Server

```shell
  $ sudo systemctl stop HES-<Name_Of_Domain>
  $ sudo mv /opt/HES/<Name_Of_Domain> /opt/HES/<Name_Of_Domain>.old
```

### 4. Build a new version of Hideez Enterprise Server from sources

```shell
  $ sudo mkdir /opt/HES/<Name_Of_Domain>
  $ cd /opt/src/HES/HES.Web/
  $ sudo dotnet publish -c release -v d -o "/opt/HES/<Name_Of_Domain>" --framework netcoreapp3.1 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES/HES.Web/Crypto_linux.dll /opt/HES/<Name_Of_Domain>/Crypto.dll
```

### 5. Restore your configuration file

```shell
  $ sudo cp /opt/HES/<Name_Of_Domain>.old/appsettings.json /opt/HES/<Name_Of_Domain>/appsettings.json
```

### 6. Restart Hideez Enterprise Server and check its status

```shell
  $ sudo systemctl restart HES-<Name_Of_Domain>
  $ sudo systemctl status HES-<Name_Of_Domain>
  
  ● HES-hideez.example.com.service - hideez.example.com Hideez Enterprise Service
   Loaded: loaded (/usr/lib/systemd/system/HES-hideez.example.com.service; enabled; vendor preset: disabled)
   Active: active (running) since Wed 2020-03-25 10:48:12 UTC; 16s ago
 Main PID: 4657 (HES.Web)
   CGroup: /system.slice/HES-thideez.example.com.service
           └─4657 /opt/HES/hideez.example.com/HES.Web

Mar 25 10:48:12 hesservertest systemd[1]: Started hideez.example.com Hideez Enterprise Service.
