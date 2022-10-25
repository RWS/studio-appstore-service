# AppStoreIntegrationServiceManagement & AppStoreIntegrationSeriveAPI

## Table of contents 

  1. [How to publish AppStoreIntegrationServiceManagement on local folder path](#publishing-PAAdmin-on-local-folder-path)
  2. [How to publish AppStoreIntegrationServiceAPI on local folder path](#publishing-API-on-local-folder-path)
  3. [How to publish AppStoreIntegrationServiceManagement on IIS Server](#publishing-PAAdmin-on-IIS)
  4. [How to publish AppStoreIntegrationServiceAPI on IIS Server](#publishing-API-on-IIS)
  5. [How to publish AppStoreIntegrationServiceManagement on Azure](#publishing-PAAdmin-on-Azure)
  6. [How to publish AppStoreIntegrationServiceAPI on Azure](#publishing-API-on-Azure)

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
+ Download and Install [Url Rewrite](https://www.iis.net/downloads/microsoft/url-rewrite)
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
+ Download and Install [Url Rewrite](https://www.iis.net/downloads/microsoft/url-rewrite)
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

<a name="publishing-PAAdmin-on-Azure"/>

## 5. Publishing AppStoreIntegrationServiceManagement on Azure

### Configure Azure Resources for AppStoreIntegrationServiceManagement
+ Go to [Microsoft Azure](https://portal.azure.com/#home)
+ Click on __Resource Groups__ icon:

	> ![alt-text][Resource-groups]
+ Click on __AppService__ in the list
+ On the AppService page, make sure the __Overview__ tab is selected from the navigation bar:

	> ![alt-text][Overview]
+ In the __Resources__ list clik on your resource you want to use to host the application
+ On your application resource click on __Configuration__ in the left side navigation bar
+ The __Application settings__ section should be configured with the following key-value pairs
```
APPSTOREINTEGRATION_BLOBNAME: _the blobl used for your application_ (e.g. managementpluginblob)
APPSTOREINTEGRATION_PLUGINSFILENAMEV1: _the file used to store the plugins_ (e.g. pluginsV1.json)
APPSTOREINTEGRATION_PLUGINSFILENAMEV2: _the file used to store the plugins_ (e.g. pluginsV2.json)
APPSTOREINTEGRATION_MAPPINGFILENAME: _the file used to store the name mappings_ (e.g. mappings.json) 
APPSTOREINTEGRATION_PRODUCTSFILENAME: _the file used to store the products_ (e.g. productsFile.json)  
APPSTOREINTEGRATION_SETTINGSFILENAME: _the file used to store the site settings_ (e.g. settingsFile.json)   
APPSTOREINTEGRATION_STORAGE_ACCOUNTKEY: _the account key from your Azure_
APPSTOREINTEGRATION_STORAGE_ACCOUNTNAME: _the account name of your Azure_
```
+ The __Connection string__ section should be configured with the following key-value pair:
```
AppStoreIntegrationServiceContextConnection: _your data base connection string_
```
+ _Note:_ The __Application settings__ and __Connection string__ sections from both _AppStoreIntegrationServiceAPI_ and _AppStoreIntegrationServiceManagement_ should always be in sync
+ Don't forget to click __Save__ otherwise your configuration will be lost:

	> ![alt-text][Save]	
### Download publish profile for AppStoreIntegraionServiceManagement
+ Go to [Microsoft Azure](https://portal.azure.com/#home)
+ Click on __Resource Groups__ icon:

	> ![alt-text][Resource-groups]
+ Click on __AppService__ in the list
+ On the AppService page, make sure the __Overview__ tab is selected from the navigation bar:

	> ![alt-text][Overview]
+ On the top of the page click on __Get publish profile__ to download the profile locally:

	> ![alt-text][Download-publish-profile]
### Prepare publish profile in Visual Studio for AppStoreIntegrationServiceManagement
+ __Warning:__ Before proceeding to publishing steps make sure you copy the content from _appsettings-azure.json_ into _appsettings.json_ from the solution explorer
+ Open the solution containg the __AppStoreIntegrationServiceManagement__ in Visual Studio
+ Right-click on __AppStoreIntegrationServiceManagement__ and select _Publish_
+ On the Publish page press _New:_

	> ![alt-text][New-publish-profile]
+ In the promted window select _Import file:_

	> ![alt-text][Import-profile]
+ Click _Next_ and browse your computer for the publish profile you downloaded at _step 2_
+ Click _Finish_
+ In the __Settings__ tab on __Publish__ page click on _Show all settings_
+ Under _File Publish Option_ make your you check:

	> ![alt-text][Settings-check]
+ Click __Publish__ and wait for the process to finish. If there are no errors during the process the application should open in browser

<a name="publishing-API-on-Azure"/>

## 6. Publishing AppStoreIntegrationServiceAPI on Azure

### Configure Azure Resources for AppStoreIntegrationServiceManagement
+ Go to [Microsoft Azure](https://portal.azure.com/#home)
+ Click on __Resource Groups__ icon:

	> ![alt-text][Resource-groups]
+ Click on __AppService__ in the list
+ On the AppService page, make sure the __Overview__ tab is selected from the navigation bar:

	> ![alt-text][Overview]
+ In the __Resources__ list clik on your resource you want to use to host the application
+ On your application resource click on __Configuration__ in the left side navigation bar
+ The __Application settings__ section should be configured with the following key-value pairs
```
APPSTOREINTEGRATION_BLOBNAME: _the blobl used for your application_ (e.g. managementpluginblob)
APPSTOREINTEGRATION_PLUGINSFILENAMEV1: _the file used to store the plugins_ (e.g. pluginsV1.json)
APPSTOREINTEGRATION_PLUGINSFILENAMEV2: _the file used to store the plugins_ (e.g. pluginsV2.json)
APPSTOREINTEGRATION_MAPPINGFILENAME: _the file used to store the name mappings_ (e.g. mappings.json) 
APPSTOREINTEGRATION_PRODUCTSFILENAME: _the file used to store the products_ (e.g. productsFile.json)  
APPSTOREINTEGRATION_SETTINGSFILENAME: _the file used to store the site settings_ (e.g. settingsFile.json)   
APPSTOREINTEGRATION_STORAGE_ACCOUNTKEY: _the account key from your Azure_
APPSTOREINTEGRATION_STORAGE_ACCOUNTNAME: _the account name of your Azure_
```
+ The __Connection string__ section should be configured with the following key-value pair:
```
AppStoreIntegrationServiceContextConnection: _your data base connection string_
```
+ _Note:_ The __Application settings__ and __Connection string__ sections from both _AppStoreIntegrationServiceAPI_ and _AppStoreIntegrationServiceManagement_ should always be in sync
+ Don't forget to click __Save__ otherwise your configuration will be lost:

	> ![alt-text][Save]
### Download publish profile for AppStoreIntegraionServiceAPI
+ Go to [Microsoft Azure](https://portal.azure.com/#home)
+ Click on __Resource Groups__ icon:

	> ![alt-text][Resource-groups]
+ Click on __AppService__ in the list
+ On the AppService page, make sure the __Overview__ tab is selected from the navigation bar:

	> ![alt-text][Overview]
+ On the top of the page click on __Get publish profile__ to download the profile locally:

	> ![alt-text][Download-publish-profile]
### Prepare publish profile in Visual Studio for AppStoreIntegrationServiceAPI
+ __Warning:__ Before proceeding to publishing steps make sure you copy the content from _appsettings-azure.json_ into _appsettings.json_ from the solution explorer
+ Open the solution containg the __AppStoreIntegrationServiceAPI__ in Visual Studio
+ Right-click on __AppStoreIntegrationServiceAPI__ and select _Publish_
+ On the Publish page press _New:_

	> ![alt-text][New-publish-profile]
+ In the promted window select _Import file:_

	> ![alt-text][Import-profile]
+ Click _Next_ and browse your computer for the publish profile you downloaded at _step 2_
+ Click _Finish_
+ In the __Settings__ tab on __Publish__ page click on _Show all settings_
+ Under _File Publish Option_ make your you check:

	> ![alt-text][Settings-check]
+ Click __Publish__ and wait for the process to finish. If there are no errors during the process the application should open in browser

[Release]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Release.PNG
[New-publish-profile]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/New-publish-profile.png
[Publish-folder-path]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Publish-folder-path.PNG
[IIS-computer-name]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/IIS-computer-name.PNG
[Add-website]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Add-website.PNG
[Site-data]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Site-data.PNG
[IIS-server]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/IIS-server.PNG
[Web-deploy]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Web-deploy.PNG
[Resource-groups]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Resource-groups.png
[Overview]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Overview.png
[Download-publish-profile]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Download-publish-profile.png
[Settings-check]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Settings-check.png
[Save]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Save.png
[Import-profile]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM3898-UIValidation/Images/Import-profile.png