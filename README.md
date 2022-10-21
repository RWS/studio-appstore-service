# AppStoreIntegrationServiceManagement & AppStoreIntegrationSeriveAPI

## Table of contents 

1. [How to publish AppStoreIntegrationServiceManagement on local folder path](#publishing-PAAdmin-on-local-folder-path)
2. [How to publish AppStoreIntegrationServiceAPI on local folder path](#publishing-API-on-local-folder-path)
3. [How to publish AppStoreIntegrationServiceManagement on IIS Server](#publishing-PAAdmin-on-IIS)
4. [How to publish AppStoreIntegrationServiceAPI on IIS Server](#publishing-API-on-IIS)

<a name="publishing-PAAdmin-on-local-folder-path"/>

## 1. Publishing AppStoreIntegrationServiceManagement (PA Admin) on local folder path (.exe)

### Configuration of the local folder path
+ Go to release section and download the latest version available
  ![alt text][Release]
+ The .zip file contains the source code, two json files with the formats used by studio and a file with possible configuration for appsettings.json
+ Download, unzip and open the solution in Visual Studio
+ From the files three under the AppStoreIntegrationServiceManagement project open appsettings.json
+ Go to the .zip file you just downloaded and copy the content of the file "appsettings-local.json" into the appsettings.json from Visual studio and save it
+ Now, the AppStoreIntegrationServiceManagement is set for local folder path deployment
	
### Publishing to local folder path
+ From the Solution Explorer right-click on AppStoreIntegrationServiceManagement and select Publish
+ On the Publish window, click new to create a new Publish profile
  ![alt text][New-publish-profile]
+ Select Folder from the prompt
  ![alt text][Publish-folder-path]
+ Make sure you choose an easy to find location becase this is where your .exe file will be published
+ Click Finish

### Running the PA Admin
+ Go to the folder where you published your application
+ Double-click AppStoreIntegrationServiceManagement.exe
+ A console will open that will tell you "The application has started"
+ In the same console you will find the port where you can view your application (e.g. https://localhost:5005)
+ Copy the link in the browser and the application should start

### Login in the PA Admin
	- The application has a build in user with administrator privillege with the following credentials:
		- Username: Admin
		- Password: Administrator
	- Pass the credentials in the login form and click sign-in

<a name="publishing-API-on-local-folder-path"/>

## 2. Publishing AppStoreIntegrationServiceAPI on local folder path (.exe)

### Configuration of the local folder path
+ Go to release section and download the latest version available
  ![alt text][Release]
+ The .zip file contains the source code, a json file with the formats used by studio and a file with possible configuration for appsettings.json
+ Open the solution in Visual Studio
+ From the files three under the AppStoreIntegrationServiceAPI project open appsettings.json
+ Go to the .zip file you just downloaded and copy the content of the file "appsettings-local.json" into the appsettings.json from Visual studio and save it
+ Now, the AppStoreIntegrationServiceAPI is set for local folder path deployment

### Publishing to local folder path
+ From the Solution Explorer right-click on AppStoreIntegrationServiceAPI and select Publish
+ On the Publish window, click new to create a new Publish profile
  ![alt text][New-publish-profile]
+ Select Folder from the prompt
  ![alt text][Publish-folder-path]
+ Make sure you choose an easy to find location becase this is where your .exe file will be published
+ Click Finish

### Running the API
+ Go to the folder where you published your application
+ Double-click AppStoreIntegrationServiceAPI.exe
+ A console will open that will tell you "The application has started"
+ In the same console you will find the port where you can view your application (e.g. https://localhost:5005)
+ Copy the link in the browser and the response should apper

<a name="publishing-PAAdmin-on-IIS"/>

## 3. Publishing AppStoreIntegrationServiceManagement (PA Admin) on IIS server

### Prerequisites
+ Go to [this page](https://msdn.microsoft.com/en-us/windowsserver2012r2.aspx) and download IIS Server
+ Install the IIS server, or follow [this link](https://www.guru99.com/deploying-website-iis.html) for instalation steps
+ Download the [Windows Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-3.1.3-windows-hosting-bundle-installer), otherwise your IIS server will not work!

### Configuration of the IIS Server
+ Open the IIS server you installed
+ In the left-hand side you will find the name of your computer (e.g. WSAMZN-FGLJ96PK)
  ![alt text][IIS-computer-name]
+ Exapand or right-click to create a new webiste
  ![alt text][Add-website]
+ In the New Website prompt enter your site data
  ![alt text][Site-data]

### Publishing to IIS
+ Replace the content of appsettings.json in Visual Studio with the content from appsettings-local.json
+ In Visual Studio, from the solution explorer right-click on AppStoreIntegrationServiceManagement and select Publish
+ On the Publish window click new to create a new Publish profile
  ![alt text][New-publish-profile]
+ Select Web Server (IIS) from the list
  ![alt text][IIS-server]
+ Select Web Deploy in the following window
  ![alt-text][Web-deploy]
+ In the next window enter the details of your webiste you created in IIS Server
	- Server: localhost
	- Site name: the name of the site you set in IIS Server APP
	- Destination URL: http://localhost:80/Plugins (if you enter https instead of htpp you will have to enter your credentials)
+ Click on validate connection
+ If the connection succedeed you can click finish
+ Wait for the publish to end and the application should open in browser

<a name="publishing-API-on-IIS"/>

## 4. Publishing AppStoreIntegrationServiceAPI on IIS Server

### Prerequisites
+ Go to [this page](https://msdn.microsoft.com/en-us/windowsserver2012r2.aspx) and download IIS Server
+ Install the IIS server, or follow [this link](https://www.guru99.com/deploying-website-iis.html) for instalation steps
+ Download the [Windows Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-3.1.3-windows-hosting-bundle-installer), otherwise your IIS server will not work!

### Configuration of the IIS Server
+ Open the IIS server you installed
+ In the left-hand side you will find the name of your computer (e.g. WSAMZN-FGLJ96PK)
  ![alt text][IIS-computer-name]
+ Exapand or right-click to create a new webiste
  ![alt text][Add-website]
+ In the New Website prompt enter your site data
  ![alt text][Site-data]

### Publishing to IIS
+ Replace the content of appsettings.json in Visual Studio with the content from appsettings-local.json
+ In Visual Studio, from the solution explorer right-click on AppStoreIntegrationServiceAPI and select Publish
+ On the Publish window click new to create a new Publish profile
  ![alt text][New-publish-profile]
+ Select Web Server (IIS) from the list
  ![alt text][IIS-server]
+ Select Web Deploy in the following window
  ![alt-text][Web-deploy]
+ In the next window enter the details of your webiste you created in IIS Server
	- Server: localhost
	- Site name: the name of the site you set in IIS Server APP
	- Destination URL: http://localhost:80/Plugins (if you enter https instead of htpp you will have to enter your credentials)
+ Click on validate connection
+ If the connection succedeed you can click finish
+ Wait for the publish to end and the application should open in browser


[Release]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Release.png
[New-publish-profile]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/New-publish-profile.png
[Publish-folder-path]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Publish-folder-path.png
[IIS-computer-name]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/IIS-computer-name.png
[Add-website]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Add-website.png
[Site-data]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Site-data.png
[IIS-server]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/IIS-serer.png
[Web-deploy]: https://github.com/RWS/studio-appstore-service/blob/SDLCOM-3929-AddCategoriesTable/Images/Web-deploy.png