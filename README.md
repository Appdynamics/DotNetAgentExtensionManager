<p>
  <span>AppDynamics Manager for .Net Agent Extension works with the .Net Agent to capture and report metrics not gathered out of the box. It can be used to capture metrics such as: Windows Event Log data, current Windows Service states, Windows reboot times, or kick off custom scripts to gather metrics from any source.</span></p>
<h2><span>Contents</span></h2>
<ul>
  <li style="list-style-type: none;">
    <span style="font-weight: bold;">Folders</span>
    <ul>
      <li style="padding-left: 10px;"><span>/Extensions</span><span>- contains configuration data for each extension.</span></li>
      <li style="padding-left: 10px;"><span>/Logs</span><span>- contains logs for each extension</span></li>
    </ul>
  </li>
</ul>
<ul>
  <li style="list-style-type: none;"><span style="font-weight: bold;">Files</span>
    <ul>
      <li style="padding-left: 10px;"><b>* <span>AppDynamics.Extension.Manager.exe</span></b><span>- The Extension Manager UI, use this to enable and configure extensions and to install/remove the AppDynamics.Agent.Extension Windows service.</span></li>
    <li style="padding-left: 10px;"><b>* <span>ExtensionService.exe</span></b><span>- The AppDynamics.Agent.Extension Windows service executable, a lightweight Windows service which continuously collects metric data for all .NET extensions and stores this data in Windows Performance Counters. These Performance Counters are polled continuously and their values are sent to the Controller by the AppDynamics.Agent.Coordinator service.</span></li>
     <li style="padding-left: 10px;"><b>* <span>AppDynamics.Extension.CCT.exe</span></b><span>- Configures which Windows Performance Counter metrics will be sent to the AppDynamics Controller via the AppDynamics.Agent.Coordinator Windows service.</span></li>
      <li style="padding-left: 10px;"><b>* <span>ExtensionService.bat</span></b><span>- Manages extension service without UI. This file can be used for unattended installation and managing of extension service.</span></li></ul></li></ul>

<h2><span>Prerequisites</span></h2>
<ul><li><span>The AppDynamics .NET Agent</span></li><li><span>.NET 4.5 or later</span></li></ul>
<li><img src="https://github.com/Appdynamics/DotNetAgentExtensionManager/blob/master/Source/AppDynamics.Extension.Manager/Picture%201.png"></li><li>

<h2><span>Installation</span></h2>
<p><span>Download and unzip the archive to any fixed location, such as %ProgramData%\Appdynamics or just C:\</span> <br> <span>Once unzipped, you should see previously mentioned files and folders, along with other required configuration or library files.</span></p>


<h2><span>Overview of Extension Types</span></h2>
<p>
  <span>Presently, the .NET agent extension service supports following extensions types:</span>
</p>

<ul>
  <li><span><b>Event extensions</b> - Event extensions are used to send custom Events to the controller.</span><a href="https://docs.appdynamics.com/21.10/en/appdynamics-essentials/monitor-events">https://docs.appdynamics.com/21.10/en/appdynamics-essentials/monitor-events</a><span>.</span></li><li><span><b>Metric extensions</b>- Metric extensions are used to extend the capability of agent to collect any metric from the machine or any other running application. These metrics can then be seen in the controller Metric Browser under following path: <br> Application Infrastructure Performance - [Any Tier] - Custom Metrics - Performance Monitor - [Extensioin Name]. <br> Additionally health rules can be created for these metrics to trigger Alerts when necessary.</span></li></ul>
  
<p>
  <span>Some commonly used extensions have been bundled along with the Extension Manager.</span>
</p>
<ol>
  <li><span>Windows Event log Watcher (Event extension)</span></li>
  <li><span>Windows Reboot Watcher (Event extension)</span></li>
  <li><span>Windows Service Status Monitor (Metric extension)</span></li>
  <li><span>Script Monitor (Metric extension)</span></li>
</ol>
<p>
  <span style="font-weight: bold;">By default, all provided extensions are disabled.</span>
  <span>You can enable them by editing their corresponding XML configuration file. Please visit their respective extension pages for more details to enable.c    </span>
</p>

<h2><span>Getting Started</span></h2>
<ol start="1"><li><span>Open AppDynamics.Extension.Manager.exe</span><ul><li>* <span>Click the Install button to install the AppDynamics.Agent.Extension Windows service.</span><img alt="AppDynamics Dotnet Agent ExtensionManager" src="https://www.appdynamics.com/media/uploaded-files/1491249335/extensionmanager.png" style="padding: 8px 0;"></li><li>* <span>By defualt we dont have any extensions loaded. We need to create a new folder for any extension, under [Extension Manager]/Extensions directory. Extension folder must have valid extension.xml file in it. Once copied we should be able to see extension loaded upon relaunching extension manager ui.</span></li></ul><ul><li>* <span>Click the "Manage" button next to any extension to edit the XML configuration file located as</span><span style="">/Extensions/[Extension Name]/extension.xml</span><span>. Alternatively the XML editor of your choice can be used to edit and save these files.</span></li><li><img src="https://www.appdynamics.com/media/uploaded-files/1491249335/extensionservice-start.png"></li><li>* <span>Now click <i>Start</i> to start extension service.</span></li></ul></li></ol>

<ol start="2"><li><span>EVENT and METRIC extensions collect and send data to the controller differently.<br></span><ul><li>EVENT extensions make use of the AppDynamics Controller REST API and require controller user credentials to be added to their configuration file.</li><li>METRIC extensions create Windows Performance Counters to store metric data until it's sent to the Controller via the AppDynamics.Agent.Coordinator service. In step #4 below, we'll add these Performance Counters to the AppDynamics.Agent.Coordinator configuration file (config.xml).</li></ul></li></ol>

<ol start="3"><li><span>Once you've updated the configuration for these extensions, you'll want to recycle the AppDynamics.Agent.Extension Windows Service for these changes to take effect.</span><br> <span>Note</span><span>: it could take up to a minute for the service to stop. This time is necessary to terminate and clean all the resources effectively.</span></li></ol>

<ol start="4"><li><span>If any METRIC extensions have been enabled, it will add all generated metrics to coordinator's configuration file (config.xml). We can use <i>AppDynamics.Extension.CCT.exe</i> to edit/remove the appropriate Performance Counters to the AppDynamics.Agent.Coordinator configuration file (config.xml).</span><ul><li><span>Restart the AppDynamics.Agent.Coordinator service for the changes made in config.xml to take effect.</span></li></ul></li></ol>

<ol start="5"><li><span>In a few minutes, you should start to see custom EVENT or METRIC data reported to the AppDymamics Controller.</span><ul><li><span>For EVENT extensions, you should see new custom events sent to the controller. https://docs.appdynamics.com/21.10/en/appdynamics-essentials/monitor-events.</span></li><li><span>For METRIC extensions, you can find the new metrics in the Metric Browser under Application Infrastructure Performance - [Any Tier] - Custom Metrics - Performance Monitor - [Extensioin Name].</span></li></ul></li></ol>

<h2><span>Unattended Installation</span></h2>
<p><span>Extension Service can be managed using following commands-</span><br></p>
<div><ol style="list-style-type:none;"><li>start</li><li>stop</li><li>restart</li><li>install</li><li>uninstall</li><li>encrypt [data-to-encrypt]</li></ol></div>
<div>Usage:</div>

<h2><span>Encrypt Password</span></h2>
<p><span>To avoid using password in plain text, we can use encrypted passwords in extension.xml for any event type extensions.</span></p>
<div>Encrypt password-</div>
<ol style="list-style-type:none;"><li><span>Run following command:</span></li><li><div style="margin-left:15px;background-color:#ddd;float:none;width:300px;">ExtensionService.bat encrypt mYp@ssW0rd!</div></li><li>Output would be similar to:</li><li><div style="margin-left:15px;background-color:#ddd;float:none;width:300px;">Encrypting password -&gt; mYp@ssW0rd! <br> JDZwM2ZvNlc=</div></li><li>Here <span style="text-decoration:underline;">JDZwM2ZvNlc=</span> is the encrypted value for given password, which we can copy and use in extension.xml</li></ol>

<div>Once we get encrypted password, we need to copy it in extension.xml along with attribute encrypted="true". As following</div>

<ol style="list-style-type:none;"><li><div style="margin-left:15px;background-color:#ddd;float:none;">&lt;controller-info user="admin" account="anurag" password="JDZwM2ZvNlc=" encrypted="true" /&gt;</div></li></ol>

<div>Default value for attribute encrypted is false. To use password in plain text, set the encrypted to false or remove the attribute.</div>
<h2><span>Troubleshooting</span></h2>
<p><span>If you're not seeing metrics reported to the controller, check to make sure the custom Windows Performance Counters created by the Extension Service have been added to the AppDynamics.Agent.Coordinator configuration file (config.xml). If the Performance Counters have been added, try restarting the Coordinator to ensure the configuration has been picked up. If this doesn't help, check the Logs folder for any errors.</span></p>
<h2><span>Upgrade</span></h2>
<ol start="1">
  <li><span>Download and extract latest version of extension manager</span></li>
  <li><span>Stop extension service and uninstall</span></li>
  <li><span>Copy Extensions directory from old install location</span></li>
  <li><span>Launch Appdynamics.Extension.Manager.exe from new installation and install extension service.</span></li>
  <li><span>Start Extension Service to use latest version.</span></li>
</ol>

<h2><span>Releases Notes</span></h2>
<div>
  <div>1.4.2</div>
     <ol start="1" style="padding-left:25px;"><li><span>Support for custom location for config.xml file.</span></li><li><span>Use of encrypted password for controller communication.</span></li></ol><div>1.5.0</div><ol start="1" style="padding-left:25px;"><li><span>Support for TLS1.2.</span></li><li><span>Upgraded project to .NET 4.5.</span></li></ol><div>1.5.1</div><ol start="1" style="padding-left:25px;"><li><span>Improved and fixed issues with encryption.</span></li><li><span>Improved logging for event based extension.</span></li></ol><div>1.5.2</div><ol start="1" style="padding-left:25px;"><li><span>Windows event properties (eventid, eventsource, machinename) are now part of controller properties.</span></li><li><span>Removed event message limit.</span></li>
  </ol></div>
  
<h2><span>Notice and Disclaimer</span></h2>
<div><div>All Extensions published by AppDynamics are governed by the Apache License v2 and are excluded from the definition of covered software under any agreement between AppDynamics and the User governing AppDynamics Pro Edition, Test &amp; Dev Edition, or any other Editions.</div></div>
