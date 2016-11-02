# IIS Web Health Monitor

A Windows Service that monitors responses from all deployed application in the local IIS and restarts hung or failing sites.

To install build the phphealthmonitor project and install the windows service with:

phphealthmonitor.exe --install

To remove the service:

phphealthmonitor.exe --uninstall

The monitor logs all activity in the System logs under "Applications and Services Logs -> DOW Site Monitor"