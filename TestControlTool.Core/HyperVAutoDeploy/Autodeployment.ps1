############################################
#                 Header                   #
############################################

Param($EnvironmentId, $FileName, $LogFolder)
############################### Additional Functions ################################
function Get-ScriptDirectory{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    try {
        Split-Path $Invocation.MyCommand.Path -ea 0
    }
    catch {
    Write-Warning 'You need to call this function from within a saved script.'
    }
}

################################# Main Script Body ##################################

Get-ScriptDirectory | Set-Location
. .\Logging.ps1 $($LogFolder)
. .\Functions.ps1
. .\PowerOff.ps1

Write-Log "Log started"

Write-Log "Initializing exception trapper"
trap [Exception] {
		write-error 'Caught exception: '
		write-error $_.Exception.ToString()
		break;
	}
Write-Log "Exception trapper initialized"

Write-Log "Loading deployment information"
[xml]$Source = Get-Content $FileName
Write-Log "Deployment information loaded"

Write-Log "Getting scripts location"
$location = Get-ScriptDirectory
Write-Log "Scripts are located at $location"

################ Deployment ################

$jobList = New-Object System.Collections.ArrayList


if ([string]::IsNullOrEmpty($EnvironmentId))
{
	$EnvironmentId = "0"
	Write-Log "Setting default environment Id: $EnvironmentId"
}

Write-Log "Getting from config file VM list with Id: $EnvironmentId"
$currentVMList = $Source.SelectSingleNode("/Source/PrepareVm/VmList[@id='" + $EnvironmentId + "']")

if ($currentVMList -eq $null)
{
	Write-Log ("Can not find virtual machines list with id " + $EnvironmentId) -type error
	throw "Can not find specified virtual machines list"
}

<#foreach ($vmList in $Source.Source.PrepareVm.VmList)
{
	if ($vmList.id -eq $envSetNumber)
	{
		$currentVMList = $vmList
	}
}#>

#PowerOffMachines($EnvironmentId)

foreach ($machine in $currentVMList.machine)
{
	$isLinux = 0;
	$deployMessage = "Creating deployment job for " + $machine.vmname
	Write-Log $deployMessage
	Write-Log "Determining the installer to use"
	$serviceName | Out-Null
	switch($machine.type) {
		"Core" {
			$fileToExecute = $source.Source.PrepareVm.CoreInstallerWildcard
			$arguments = $Source.Source.PrepareVm.CoreSilentKey
			$serviceName = $source.Source.PrepareVm.CoreServiceName
			Write-Log "Core installer will be used"
		}
		"Agent64" {
			$fileToExecute = $source.Source.PrepareVm.Agent64InstallerWildcard
			$arguments = $Source.Source.PrepareVm.AgentSilentKey
			$serviceName = $source.Source.PrepareVm.AgentServiceName
			Write-Log "64-bit agent installer will be used"
		}
		"Agent32" {
			$fileToExecute = $source.Source.PrepareVm.Agent32InstallerWildcard
			$arguments = $Source.Source.PrepareVm.AgentSilentKey
			$serviceName = $source.Source.PrepareVm.AgentServiceName
			Write-Log "32-bit agent installer will be used"
		}
		"AgentUbuntu32" {
			$fileToExecute = $source.Source.PrepareVm.AgentUbuntu32InstallerWildcard
			$arguments = $Source.Source.PrepareVm.AgentUbuntuSilentKey
			$serviceName = $source.Source.PrepareVm.AgentUbuntuServiceName
			$isLinux = 1;
			Write-Log "32-bit ubuntu agent installer will be used"
		}
		"AgentCentOS64" {
			$fileToExecute = $source.Source.PrepareVm.AgentCentOS64InstallerWildcard
			$arguments = $Source.Source.PrepareVm.AgentCentOSSilentKey
			$serviceName = $source.Source.PrepareVm.AgentCentOSServiceName
			$isLinux = 1;
			Write-Log "64-bit CentOS agent installer will be used"
		}
		"AgentOpenSuse64" {
			$fileToExecute = $source.Source.PrepareVm.AgentOpenSuse64InstallerWildcard
			$arguments = $Source.Source.PrepareVm.AgentOpenSuseSilentKey
			$serviceName = $source.Source.PrepareVm.AgentOpenSuseServiceName
			$isLinux = 1;
			Write-Log "64-bit OpenSuse agent installer will be used"
		}
		"AgentUbuntu64" {
			$fileToExecute = $source.Source.PrepareVm.AgentUbuntu64InstallerWildcard
			$arguments = $Source.Source.PrepareVm.AgentUbuntuSilentKey
			$serviceName = $source.Source.PrepareVm.AgentUbuntuServiceName
			$isLinux = 1;
			Write-Log "64-bit ubuntu agent installer will be used"
		}
		default {
			Write-Log ("Cannot determine the type of installer to use: "+$machine.type) -type error
			throw "Machine type must be 'core', 'agent32', 'agent64' or 'agentUbuntu32"
		}
	}
	
	Write-Log ("Running new deployment job for "+$machine.vmname+" on "+$source.Source.PrepareVm.ServerName+" server")
	if	(!([string]::IsNullOrEmpty($machine.ip))) {
		$job = Start-Job {
			Set-Location $($args[0])
			. .\Logging.ps1 $($args[17]) $($args[5])
			. .\Functions.ps1
			Write-Log "Log started"
			PrepareEnvironment $($args[1]) $($args[2]) $($args[3]) $($args[4]) $($args[5]) $($args[6]) $($args[7]) $($args[8]) $($args[9]) $($args[10]) $($args[11]) $($args[12]) $($args[13]) $($args[14]) $($args[15]) $($args[16]) $($args[18]) $($args[19])
			Write-Log "Total errors - $errorcount, warnings - $warningcount"
			Write-Log "End of log"
		} -ArgumentList $location, $source.Source.PrepareVm.ServerName, $source.Source.PrepareVm.ServerUserName, $source.Source.PrepareVm.ServerPassword, $machine.snapshot, $machine.vmname, $machine.ip, $machine.username, $machine.password, ("\\" + $machine.ip + "\" + $machine.share), $fileToExecute, $source.Source.PrepareVm.InstallerSourcePath, $source.Source.PrepareVm.SourcePathUserName, $source.Source.PrepareVm.SourcePathPassword, $arguments, $serviceName, $isLinux, $LogFolder, $source.Source.PrepareVm.Action, $machine.share
	}
	else {
		$job = Start-Job {
			Set-Location $($args[0])
			. .\Logging.ps1 $($args[17]) $($args[5])
			. .\Functions.ps1
			Write-Log "Log started"
			PrepareEnvironment $($args[1]) $($args[2]) $($args[3]) $($args[4]) $($args[5]) $($args[6]) $($args[7]) $($args[8]) $($args[9]) $($args[10]) $($args[11]) $($args[12]) $($args[13]) $($args[14]) $($args[15]) $($args[16]) $($args[18]) $($args[19])
			Write-Log "Total errors - $errorcount, warnings - $warningcount"
			Write-Log "End of log"
		} -ArgumentList $location, $source.Source.PrepareVm.ServerName, $source.Source.PrepareVm.ServerUserName, $source.Source.PrepareVm.ServerPassword, $machine.snapshot, $machine.vmname, $machine.name, $machine.username, $machine.password, ("\\" + $machine.name + "\" + $machine.share), $fileToExecute, $source.Source.PrepareVm.InstallerSourcePath, $source.Source.PrepareVm.SourcePathUserName, $source.Source.PrepareVm.SourcePathPassword, $arguments, $serviceName, $isLinux, $LogFolder, $source.Source.PrepareVm.Action, $machine.share
	}
	
	$jobList.Add($job) | Out-Null
}

Write-Log "All deployment jobs have started successfully. See job logs for details."

WaitWhileJobsRun ($jobList)

Write-Log "End of log"