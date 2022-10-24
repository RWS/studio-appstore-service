# AppStoreIntegrationServiceManagement & AppStoreIntegrationSeriveAPI

## Table of contents 

  1. [How to publish AppStoreIntegrationServiceManagement on local folder path](#publishing-PAAdmin-on-local-folder-path)
  2. [How to publish AppStoreIntegrationServiceAPI on local folder path](#publishing-API-on-local-folder-path)
  3. [How to publish AppStoreIntegrationServiceManagement on IIS Server](#publishing-PAAdmin-on-IIS)
  4. [How to publish AppStoreIntegrationServiceAPI on IIS Server](#publishing-API-on-IIS)

<a name="publishing-PAAdmin-on-local-folder-path"/>

## 1. Publishing AppStoreIntegrationServiceManagement (PA Admin) on local folder path (.exe)

### Prerequisite
+ Download [SqlLocalDb](https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi)

### Configuration of the local folder path

+ Go to release section and download the latest version available

	> ![alt text][Release]
+ The .zip file contains the _source code_, two _json files_ with the formats used by studio and a _folder with possible configuration for appsettings.json_
+ Download and unzip the files
+ Run Visual Studio as __administrator__ and open the solution
+ Open _Windows PowerShell_, navigate to the folder containing the solution (e.g. D:\Coding\AppStoreServer\AppStoreIntegrationService) and run the command `dotnet restore` to make sure all the dependencies are installed
+ From the files three under the __AppStoreIntegrationServiceManagement__ project open _appsettings.json_
+ Go to the .zip file you just downloaded and copy the content of the file __appsettings-local.json__ into the __appsettings.json__ from project solution and save it
+ Now, the __AppStoreIntegrationServiceManagement__ is set for local folder path deployment
	
### Publishing to local folder path
+ From the _Solution Explorer_ right-click on __AppStoreIntegrationServiceManagement__ and select _Publish_
+ On the __Publish window__, click new to create a new __Publish profile__

	> ![alt text][New-publish-profile]
+ Select __Folder__ from the prompt
  	> ![alt text][Publish-folder-path]
+ Make sure you choose an easy to find location becase this is where your .exe file will be published
+ Click __Finish__

### Running the PA Admin
+ Go to the folder where you published your application
+ Double-click __AppStoreIntegrationServiceManagement.exe__
+ A console will open that will tell you _"The application has started"_
+ In the same console you will find the link where you can view your application (e.g. https://localhost:5005)
+ Copy the link in the browser and the application should start

### Login in the PA Admin
+ The application has a build in user with administrator privillege with the following credentials:

	> Username: Admin

	> Password: administrator

+ Pass the credentials in the login form and click sign-in

<a name="publishing-API-on-local-folder-path"/>

## 2. Publishing AppStoreIntegrationServiceAPI on local folder path (.exe)

### Configuration of the local folder path

+ Go to release section and download the latest version available

	> ![alt text][Release]
+ The .zip file contains the _source code_, two _json files_ with the formats used by studio and a _folder with possible configuration for appsettings.json_
+ Download and unzip the files
+ Run Visual Studio as __administrator__ and open the solution
+ Open _Windows PowerShell_, navigate to the folder containing the solution (e.g. D:\Coding\AppStoreServer\AppStoreIntegrationService) and run the command `dotnet restore` to make sure all the dependencies are installed
+ From the files three under the __AppStoreIntegrationServiceAPI__ project open _appsettings.json_
+ Go to the .zip file you just downloaded and copy the content of the file __appsettings-local.json__ into the __appsettings.json__ from project solution and save it
+ Now, the __AppStoreIntegrationServiceAPI__ is set for local folder path deployment
	
### Publishing to local folder path
+ From the _Solution Explorer_ right-click on __AppStoreIntegrationServiceAPI__ and select _Publish_
+ On the __Publish window__, click new to create a new __Publish profile__

	> ![alt text][New-publish-profile]
+ Select __Folder__ from the prompt
  	> ![alt text][Publish-folder-path]
+ Make sure you choose an easy to find location becase this is where your .exe file will be published
+ Click __Finish__

### Running the API
+ Go to the folder where you published your application
+ Double-click __AppStoreIntegrationServiceAPI.exe__
+ A console will open that will tell you _"The application has started"_
+ In the same console you will find the link where you can view your application (e.g. https://localhost:5005)
+ Copy the link in the browser and the application should start

<a name="publishing-PAAdmin-on-IIS"/>

## 3. Publishing AppStoreIntegrationServiceManagement (PA Admin) on IIS server

### Prerequisite
+ Download [SqlLocalDb](https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi)

### Prerequisites
+ Go to [this page](https://msdn.microsoft.com/en-us/windowsserver2012r2.aspx) and download the IIS Server on your computer
+ Install the IIS server, or follow [this link](https://www.guru99.com/deploying-website-iis.html) for instalation steps
+ Download the [Windows Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-3.1.3-windows-hosting-bundle-installer), otherwise your IIS server will not work!

### Configuration of the IIS Server
+ Open the __IIS server__ you installed
+ In the left-hand side you will find the name of your computer (e.g. WSAMZN-FGLJ96PK)

  	> ![alt text][IIS-computer-name]
+ Exapand or right-click to create a new webiste

 	> ![alt text][Add-website]
+ In the New Website prompt enter your site data

  	> ![alt text][Site-data]

### Publishing to IIS
+ Open _Windows PowerShell_, navigate to the folder containing the solution (e.g. D:\Coding\AppStoreServer\AppStoreIntegrationService) and run the command `dotnet restore` to make sure all the dependencies are installed
+ Run Visual Studio as __administrator__!
+ Replace the content of __appsettings.json__ in Visual Studio with the content from __appsettings-local.json__
+ In __Visual Studio__, from the solution explorer right-click on __AppStoreIntegrationServiceManagement__ and select __Publish__
+ On the __Publish window__ click new to create a new Publish profile

   	> ![alt text][New-publish-profile]
+ Select __Web Server (IIS)__ from the list

  	> ![alt text][IIS-server]
+ Select Web Deploy in the following window

  	> ![alt-text][Web-deploy]
+ In the next window enter the details of your webiste you created in __IIS Server__

	> Server: localhost
	
	> Site name: the name of the site you set in IIS Server APP
	
	> Destination URL: http://localhost:80/Plugins (if you enter https instead of http you will have to enter your credentials)
	
+ Click on _Validate connection_
+ If the connection succedeed you can click finish
+ Wait for the publish to end and the application should open in browser

<a name="publishing-API-on-IIS"/>

## 4. Publishing AppStoreIntegrationServiceAPI on IIS Server

### Prerequisites
+ Go to [this page](https://msdn.microsoft.com/en-us/windowsserver2012r2.aspx) and download the IIS Server on your computer
+ Install the IIS server, or follow [this link](https://www.guru99.com/deploying-website-iis.html) for instalation steps
+ Download the [Windows Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-3.1.3-windows-hosting-bundle-installer), otherwise your IIS server will not work!

### Configuration of the IIS Server
+ Open the __IIS server__ you installed as __administrator__!
+ In the left-hand side you will find the name of your computer (e.g. WSAMZN-FGLJ96PK)

  	> ![alt text][IIS-computer-name]
+ Exapand or right-click to create a new webiste

 	> ![alt text][Add-website]
+ In the New Website prompt enter your site data

  	> ![alt text][Site-data]

### Publishing to IIS
+ Open _Windows PowerShell_, navigate to the folder containing the solution (e.g. D:\Coding\AppStoreServer\AppStoreIntegrationService) and run the command `dotnet restore` to make sure all the dependencies are installed
+ Run Visual Studio as __administrator__!
+ Replace the content of __appsettings.json__ in Visual Studio with the content from __appsettings-local.json__
+ In __Visual Studio__, from the solution explorer right-click on __AppStoreIntegrationServiceAPI__ and select __Publish__
+ On the __Publish window__ click new to create a new Publish profile

   	> ![alt text][New-publish-profile]
+ Select __Web Server (IIS)__ from the list

  	> ![alt text][IIS-server]
+ Select Web Deploy in the following window

  	> ![alt-text][Web-deploy]
+ In the next window enter the details of your webiste you created in __IIS Server__

	> Server: localhost
	
	> Site name: the name of the site you set in IIS Server APP
	
	> Destination URL: http://localhost:80/Plugins (if you enter https instead of http you will have to enter your credentials)
	
+ Click on _Validate connection_
+ If the connection succedeed you can click finish
+ Wait for the publish to end and the application should open in browser


[Release]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Release.PNG
[New-publish-profile]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/New-publish-profile.png
[Publish-folder-path]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Publish-folder-path.PNG
[IIS-computer-name]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/IIS-computer-name.PNG
[Add-website]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Add-website.PNG
[Site-data]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Site-data.PNG
[IIS-server]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/IIS-server.PNG
[Web-deploy]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Web-deploy.PNG