@ECHO OFF

REM   #############################################################################
REM   AppDynamics Manager for .Net Agent Extension
REM   NAME: ExtensionService.bat
REM   
REM   Author: Anurag Bajpai @ AppDynamics Inc. (abajpai@appdynamics.com)
REM   
REM   Overview:  This script will perform following operations-
REM   1- Manage extension service (start/stop/install/uninstall) for unattended installation
REM   2- Encrypt password to use with event based extensions
REM  
REM   VERSION HISTORY
REM   Version: 1.5.0
REM   Release date: Dec 12 2017
REM  
REM   1- Use of encrypted password for controller communication.
REM  
REM  Copyright 2015 AppDynamics LLC
REM  
REM  Licensed under the Apache License, Version 2.0 (the "License");
REM  you may not use this file except in compliance with the License.
REM  You may obtain a copy of the License at
REM  
REM      http://www.apache.org/licenses/LICENSE-2.0
REM  
REM  Unless required by applicable law or agreed to in writing, software
REM  distributed under the License is distributed on an "AS IS" BASIS,
REM  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
REM  See the License for the specific language governing permissions and
REM  limitations under the License.
REM   #############################################################################

REM Variables 

SET SERVICE_NAME=AppDynamics.Agent.Extension_Service
SET DISPLAY_NAME=AppDynamics.Agent.Extension
SET SCRIPT_DIR=%~dp0
SET EXTENSION_EXE_PATH=%SCRIPT_DIR%ExtensionService.exe
SET DESCRIPTION=AppDynamics Extension Service for reporting custom metrics and events to AppDynamics controller with help of AppDynamics .NET agent.
SET COORDINATOR_SERVICE_NAME=AppDynamics.Agent.Coordinator_service
SET DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
SET PATH=%PATH%;%DOTNETFX2%

REM The script for managing the extension service lifecycle

IF "%1" == "install" (
	GOTO installExtensionService
)
IF "%1" == "start" (
    GOTO startExtensionService
) 
IF "%1" == "stop" (
	GOTO stopExtensionService
) 
IF "%1" == "restart" (
	CALL :stopExtensionService
	CALL :startExtensionService
	GOTO END
)
IF "%1" == "restart-coordinator" (
	GOTO restartCoordinatorService
)
IF "%1" == "uninstall" (
    GOTO uninstallExtensionService
)
IF "%1" == "encrypt" (
ECHO going to encrypting
IF "%2" == "" (
	ECHO Please provide data to encrypt in following format- 
	ECHO ExtensionService.bat encrypt MyP@ssw0rd
	GOTO END
)
	GOTO encrypt
)

GOTO badOption

:badOption
SC QUERY %SERVICE_NAME%
ECHO -----##################################-----
ECHO.
ECHO Please provide one of following options-
ECHO 	1- start 
ECHO 	2- stop  
ECHO 	3- restart 
ECHO 	4- install 
ECHO 	5- uninstall
ECHO 	6- encrypt <data-to-encrypt>
ECHO Usage:
ECHO ExtensionService.bat start
ECHO ExtensionService.bat encrypt MyP@ssw0rd
GOTO END

:installExtensionService
SC create %SERVICE_NAME% binpath="%EXTENSION_EXE_PATH%" displayname="%DISPLAY_NAME%" start="auto" 
if %ERRORLEVEL% NEQ 0 (
	ECHO Unable to install extension Service
	GOTO END
)
SC description %SERVICE_NAME% "%Description%"
if %ERRORLEVEL% NEQ 0 (
	ECHO Unable to install extension Service
	GOTO END
)
ECHO AppDynamics Extension service installed successfully. Run "ExtensionService.bat start" to start the service.
GOTO END

:startExtensionService
NET START %SERVICE_NAME%
if %ERRORLEVEL% NEQ 0 ECHO Unable to start extension Service
GOTO END

:stopExtensionService
NET STOP %SERVICE_NAME%
if %ERRORLEVEL% NEQ 0 ECHO Unable to stop extension Service
GOTO END

:restartCoordinatorService
NET STOP %COORDINATOR_SERVICE_NAME% 
if %ERRORLEVEL% NEQ 0 (
	ECHO Unable to stop extension Service
	GOTO END
)
NET START %COORDINATOR_SERVICE_NAME% 
if %ERRORLEVEL% NEQ 0 ECHO Unable to start extension Service
GOTO END

:uninstallExtensionService
CALL :stopExtensionService
SC DELETE %SERVICE_NAME%
ECHO AppDynamics Extension service uninstalled successfully.

if %ERRORLEVEL% NEQ 0 ECHO Unable to uninstall extension Service
GOTO END

:encrypt

CALL "%EXTENSION_EXE_PATH%" -%1 %2
if %ERRORLEVEL% NEQ 0 ECHO Unable to encrypt. 

GOTO END

:END
endlocal
