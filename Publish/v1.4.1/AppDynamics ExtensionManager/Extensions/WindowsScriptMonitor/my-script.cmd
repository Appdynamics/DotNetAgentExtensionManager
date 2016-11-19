@Echo off

REM		Each line of the output of script file will be parsed every minute to read metric name, 
REM		instance name and value. Out put should be in default format or as per the LineFormat
REM		provided in extension.xml.

Echo "metric1 | instance1 , value = 32350"
Echo "metric2 | instance1 , value = 355"

Echo "metric1 | instance2 , value = 12350"
Echo "metric2 | instance2 , value = 155"