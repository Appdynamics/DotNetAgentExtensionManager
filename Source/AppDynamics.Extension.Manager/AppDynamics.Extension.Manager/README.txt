Thanks for downloading AppDynamics .NET Extension Manager 1.0.0.1! 

Full documentation for the .NET Extension Manager is available at http://www.appdynamics.com/community/exchange/

**************************************
Contents
**************************************

* Folders
Extensions: contains configuration data for each extension.
Logs: contains logs for each extension

* Files
AppDynamics.Extension.Manager.exe: The Extension Manager front-end. Use this to configure extensions and to install/remove the Extension Service.
ExtensionService.exe: The Extension Service. Collects and processes custom metrics/events.
AppDynamics.Extension.CCT.exe: Configures which Windows Performance Counter metrics will be sent to the AppDynamics Controller. 


**************************************
Prerequisites 
**************************************

.NET 4.0
The AppDynamics .NET Agent; specifically, the AppDynamics.Agent.Coordinator service.

 
**************************************
Getting Started
**************************************

1. Open AppDynamics.Extension.Manager.exe. 
 * Click the Install button to install the AppDynamics.Agent.Extension Windows service, then click Start to start it.
 * Click the "Manage" button next to any extension to edit the XML configuration file located in /extensions/[Extension Name].xml. Alternatively the XML editor of your choice can be used to edit and save these files. 

2. EVENT and METRIC extensions collect and send data to the controller differently.
 * EVENT extensions make use of the AppDynamics Controller REST API and require controller user credentials to be added to their configuration file. 
 * METRIC extensions create Windows Performance Counters to store metric data until it's sent to the Controller via the AppDynamics.Agent.Coordinator service. In step #4 below, we'll add these Performance Counters to the AppDynamics.Agent.Coordinator configuration file (config.xml).

3. Once you've updated the configuration for these extensions, you'll want to recycle the AppDynamics.Agent.Extension Windows Service for these changes to take effect.  Note: it could take up to a minute for the service to stop. This time is necessary to terminate and clean all the resources effectively. 
	
4. If any METRIC extensions have been enabled, use AppDynamics.Extension.CCT.exe to add the appropriate Performance Counters to the AppDynamics.Agent.Coordinator configuration file (config.xml). For example, the WindowsServiceStatusMonitor extension included out of the box will create a Performance Counter category with the same name of "WindowsServiceStatusMonitor".
 * Restart the AppDynamics.Agent.Coordinator service for the changes made in config.xml to take effect.

5. In a few minutes, you should  start to see custom EVENT or METRIC data reported to the AppDymamics Ccontroller. 
 * For EVENT extensions, you should see new custom events sent to the controller. https://docs.appdynamics.com/display/PRO41/Monitor+Events.  
 * For METRIC extensions, you can find the new metrics in the Metric Browser under Application Infrastructure Performance - [Any Tier] - Custom Metrics.

**************************************
Unattended Installation
**************************************
Extension Service can be managed using following commands,
     ExtensionService.bat [install/uninstall/start/stop/restart] 

For example we can run ExtensionService.bat install to install the service.

**************************************
Troubleshooting
**************************************
If you're not seeing metrics reported to the controller, check to make sure the custom Windows Performance Counters created by the Extension Service have been added to the AppDynamics.Agent.Coordinator configuration file (config.xml). If the Performance Counters have been added, try restarting the Coordinator to ensure the configuration has been picked up.  If this doesn't work, check the Logs folder for any errors.
