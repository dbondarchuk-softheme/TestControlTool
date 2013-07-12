function ShutDownMachine ([string]$machineName, [System.Management.Automation.Runspaces.PSSession]$session)
{
	Invoke-Command -Session $session -ScriptBlock {
		Import-Module HyperV; Stop-VM -VM $($args[0]) -Force -Wait;
		Remove-Module HyperV;
	} -ArgumentList $machineName -ErrorAction SilentlyContinue
}

function GetMachineState ([string]$machineName, [System.Management.Automation.Runspaces.PSSession]$session)
{
	Invoke-Command -Session $session -ScriptBlock {
		Import-Module HyperV; Get-VMSummary -VM $($args[0])
		Remove-Module HyperV;
	} -ArgumentList $machineName
}

function PowerOffMachines($EnviromentId)
{
	Write-Log "Check that virtual machines from Environment #$EnviromentId are running"
	[xml]$Source = Get-Content .\AutodeploymentSource.xml
	
	$password = $Source.Source.PrepareVm.ServerPassword
	$user = $Source.Source.PrepareVm.ServerUserName
	$destination = $Source.Source.PrepareVm.ServerName
	$securePass = ConvertTo-SecureString -String $password -AsPlainText -Force
	$credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $user, $securePass
	$session = New-PSSession $destination -Credential $credentials


	$currVMList = $Source.Source.PrepareVm.VmList | where {$_.id -eq $EnviromentId}
	$allVmRunning = $true
		
	foreach($machine in $currVMList.machine)
	{	
		$result = GetMachineState $machine.vmname $session
		if($result.EnabledState.Value -ne 'Running')
		{
			$allVmRunning = $false
			Write-Log "Not all machines running"
			break
		}
	}

	if($allVmRunning -eq $false)
	{
		if($EnviromentId -eq 0)
		{$vmlist = @(1,2,3,4)}
		if($EnviromentId -eq 1)
		{$vmlist = @(2,3)}
		if($EnviromentId -eq 2)
		{$vmlist = @(3,4)}
		if($EnviromentId -eq 3)
		{$vmlist = @(1,4)}
		if($EnviromentId -eq 4)
		{$vmlist = @(1,2)}
		foreach($val in $vmlist)
		{
			$shutDownVMList = $Source.Source.PrepareVm.VmList | where {$_.id -eq $val}
			Write-Log "Turning off environment # $val:"

				foreach($machine in $shutDownVMList.machine)
				{
					Write-Log $machine.vmname
					ShutDownMachine $machine.vmname $session 
				}		
		}
	}
	Remove-PSSession $session
}