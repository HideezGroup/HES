This instruction shows how to install the HES server using docker containers on Linux. Examples of commands are given for CentOS 7 and Ubuntu 18.04, other versions of the Linux was not tested.

First of all, you need to decide what URL will be for your future HES server. It can be something like hideez.yurcompany.com. Hereinafter, this name is indicated as <your_domain_name>. You can copy this instruction into any text editor and replace all instances of the <your_domain_name> with your name. After that, you can execute most of the commands just copying them from the editor.

You need to add your domain name to the DNS settings of your hosting provider. 

# 1. Update the system and install necessary packages 

CentOS 7
```shell
# yum update -y
# yum install git -y
```
Ubuntu 18.04
```shell
# apt update
# apt upgrade -y
# apt install apt-transport-https ca-certificates curl software-properties-common -y
```

# 2. Enable and install Docker CE Repository 

CentOS 7
```shell
# yum install -y yum-utils device-mapper-persistent-data lvm2
# yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
# yum install docker-ce -y
# systemctl start docker
# systemctl enable docker
```
Ubuntu 18.04  
```shell
# curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
# add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu bionic stable"
# apt update
# apt-cache policy docker-ce
# apt install docker-ce -y
``` 

To verify installed docker version run the following command:
```shell
# docker --version
Docker version 19.03.8, build afacb8b
```


# 3. Install Docker Compose
```shell
# curl -L https://github.com/docker/compose/releases/download/1.25.4/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose
# chmod +x /usr/local/bin/docker-compose
```
Note: Replace “1.25.4” with docker compose version that you want to install but at this point of time this is the latest and stable version of the docker compos. You can see all releases of docker-compose [here](https://github.com/docker/compose/releases).

Test the installation.
```shell
# docker-compose --version
docker-compose version 1.25.4, build 1110ad01
```

# 4. Clone the HES repository
```shell
# git clone https://github.com/HideezGroup/HES.git /opt/src/HES
# cd /opt/src/HES/HES.Docker
```

# 5. Create folders for HES and copy appsettings.json
```shel
# mkdir /opt/HES
# cp -r /opt/src/HES/HES.Docker/* /opt/HES
# cp /opt/src/HES/HES.Web/appsettings.json /opt/HES/hes-site/appsettings.Production.json
```

# 6. Configure the HES
Edit the file `/opt/HES/hes-site/appsettings.Production.json`

The following is an example of how to open a configuration file for editing using the vi editor:
```shell
# vi /opt/HES/hes-site/appsettings.Production.json
```

This file contains configuration and security settings, required to run the HES server. It looks like this:

```json
  {
  "ConnectionStrings": {
    "DefaultConnection": "server=<mysql_server>;port=<mysql_port>;database=<db_name>;uid=<db_user>;pwd=<db_user_password>"
  },

  "EmailSender": {
    "Host": "<smtp_server_host>",
    "Port": "<smpt_server_port>",
    "EnableSSL": true,
    "UserName": "<smtp_server_name>",
    "Password": "<smtp_server_password>"
  },
  
 "Fido2": {
    "ServerDomain":"<your_domain_name>",
    "ServerName": "HES",
    "Origin": "https://<your_domain_name>",
    "TimestampDriftTolerance": 300000,
    "MDSAccessKey": null
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
You need to modify values embraced in "<>". These are grouped into DB Settings, SMTP Server Settings and Data Protection Settings. 

## 6.1 DB Settings
**<mysql_server>** - put here "hes-db". This name must be the same as the MySQL container name in the `/opt/HES/docker-compose.yml` which is "hes-db" by default.

**<mysql_port>** - put here "3306". This is default value.

**<db_name>** - name of the DB (e.g. "db").

**<db_user>** - username to access the DB (e.g. "user").

**<db_user_password>** - user password to access the DB. (default "password")

## 6.2 SMTP Server Settings
SMTP server credentials required for HES to be able to send email notifications to the admins. This is essential functionality of the server and you need to provide valid values:

**<smtp_server_host>** - SMTP Server host name or IP address (e.g. "smtp.gmail.com")

**<smpt_server_port>** - SMTP Server port (e.g. "465").

**<smtp_server_name>** - your SMTP server account (e.g. "user@gmail.com").

**<smtp_server_password>** - your SMTP server password.

## 6.3 Data Protection Settings
**<data_protection_password>** - Your password for database encryption. Leave this field blank. Later on, when you have installed the HES, goto Settings -> Data Protection and read carefully the instructions. If you will decide to enable the Data Protection, you can store the password in this field.

# 6. Configure the Docker (Optional)
Open the `/opt/HES/docker-compose.yml` file for editing. In this file you need to modify several parameters:

**MYSQL_DATABASE** - put here the same name as <db_name> from the 6.1 (e.g. "db").

**MYSQL_USER** - put here the same name as <db_user> from the 6.1 (e.g. "user").

**MYSQL_PASSWORD** - put here the same password as <db_user_password> from the 6.1.

**MYSQL_ROOT_PASSWORD** - put here the password for 'root' account.
	  
# 7. Configure the Nginx 
Open the `/opt/HES/nginx/nginx.conf` file for editing. Replace all instances of <your_domain_name> with your name.

# 8. Create a SSL Certificate
Here we providing instruction on how to get a self-signed certificate for SSL encryption. It can be used for test or demo purposes. For the production server, you need to acquire a certificate from a certificate authority. For a self-signed certificate, the browser will alert you that the site has security issues. 

Run the following command and answer a few simple questions:
```shel
# openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /opt/HES/nginx/certs/hes.key -out /opt/HES/nginx/certs/hes.crt
```

The certificate will be generated and copied to the HES directory. 

# 9. Run the Server
Finally, when config files updated and certificate ready you can run the server:

```shell
# cd /opt/HES
# docker-compose up -d --build
```

Restart  containers:
```shell
# docker-compose down && docker-compose up -d
```

# 10. Check the status
You can check the status of the docker containers running the command:
```shell
# docker-compose ps
```

To make sure that everything is configured correctly, open the URL of your site in a browser (`https://<your_domain_name>`). You should see the server authorization page. Log in using the default login 'admin@hideez.com' and default password 'admin'.

In case you cannot log in to the HES, see log files located in '/opt/HES/hes-site/logs' 

# 11. How to update HES server
To update the server from the latest sources, run commands: 
```shell
# cd /opt/HES/
# docker-compose down
# docker rmi hes_hes
# docker-compose up --build -d
```

# 12. Next Steps
See the <a href="https://support.hideez.com/hideez-enterprise-server" target="_blank">User Manuals</a> for futher settings. 

