############################## VM Management Functions ##############################

##############
# Restore VM #
##############

function RestoreVM($ServerName, $UserName, $Password, $VmName, $SnapshotName)
{
	Write-Log "=== RestoreVM function has started ==="
	$snapshot, $session = GetSnapshot $ServerName $UserName $Password $VmName $SnapshotName
	
	Write-Log "Restoring '$VmName' machine to '$SnapshotName' snapshot"
	try {
		Invoke-Command -Session $session -ScriptBlock {
			Import-Module HyperV; $($args[0]) | Restore-VMSnapshot -force -wait
			} -ArgumentList $snapshot | Out-Null}
	catch {
		Write-Log "An error has occured while trying to restore '$VmName' to '$SnapshotName' snapshot" -type error
		Write-Log "Removing remote session for '$UserName' on '$ServerName'"
		Remove-PSSession $session
		Write-Log "--- RestoreVM function has failed ---"
		Write-Log ""
		throw "Couldn't perform 'Restore-VMSnapshot' on '$ServerName' server"}
	Write-Log "'$VmName' machine was successfully restored to '$SnapshotName' snapshot"
	
	Write-Log "Removing remote session for '$UserName' on '$ServerName'"
	Remove-PSSession $session
	Write-Log "--- RestoreVM function has succeeded ---"
	Write-Log ""
}

############
# Start VM #
############

function StartVM($ServerName, $UserName, $Password, $VmName)
{
	Write-Log "=== StartVM function has started ==="
	$session, $credentials = CreateRemoteSession $ServerName $UserName $Password
	
	Write-Log "Starting '$VmName' remote machine"
	try	{
		Invoke-Command	-Session $session -ScriptBlock {
			Import-Module HyperV; Start-VM -VM $($args[0]) -force -wait -HeartBeatTimeOut 600
			} -ArgumentList $VmName | Out-Null}
	catch {
		Write-Log "An error has occured while trying to start '$VmName' machine"
		Write-Log "Removing remote session for '$UserName' on '$ServerName'"
		Remove-PSSession $session
		Write-Log "--- StartVM function has failed ---"
		Write-Log ""
		throw "Couldn't start '$VmName' machine"}
	Write-Log "'$VmName' remote machine has started successfully"
	
	Write-Log "Removing remote session for '$UserName' on '$ServerName'"
	Remove-PSSession $session
	Write-Log "--- StartVM function has succeeded ---"
	Write-Log ""
}

###########
# Stop VM #
###########

function StopVM($ServerName, $UserName, $Password, $VmName)
{
	Write-Log "=== StopVM function has started ==="
	$session, $credentials = CreateRemoteSession $ServerName $UserName $Password
	
	Write-Log "Stopping '$VmName' remote machine"
	try	{
		Invoke-Command	-Session $session -ScriptBlock {
			Import-Module HyperV; Stop-VM -VM $($args[0]) -force -wait
			} -ArgumentList $VmName | Out-Null}
	catch {
		Write-Log "An error has occured while trying to start '$VmName' machine"
		Write-Log "Removing remote session for '$UserName' on '$ServerName'"
		Remove-PSSession $session
		Write-Log "--- StopVM function has failed ---"
		Write-Log ""
		throw "Couldn't start '$VmName' machine"}
	Write-Log "'$VmName' remote machine has stopped successfully"
	
	Write-Log "Removing remote session for '$UserName' on '$ServerName'"
	Write-Log "--- StopVM function has succeeded ---"
	Write-Log ""
	Remove-PSSession $session
}

############################ Files Management Functions #############################

##############
# Copy Files #
##############

function CopyFiles($FileNameWildcards, $SourceFolder, $SourceUserName, $SourcePassword, $RemoteFolder, $RemoteUserName, $RemotePassword, [bool]$Recurse = $true, [bool]$Newest = $true, $SizeLimit = -1, [int]$Count = 1, $Timeout = 600)
{
	Write-Log "=== CopyFiles function has started ==="
	$time = $Timeout
	
	$SourceCopyState = WaitForPathToBeAccessible $SourceFolder $SourceUserName $SourcePassword $time
	if (!$SourceCopyState) {
		Write-Log "Source folder was unreachable for $Timeout seconds" -type error
		Write-Log "--- CopyFiles function falied ---"
		throw "Source path is unreachable"
	}
	
	$DestinationCopyState = WaitForPathToBeAccessible $RemoteFolder $RemoteUserName $RemotePassword $time
	if (!$DestinationCopyState) {
		Write-Log "Destination folder was unreachable for $Timeout seconds" -type error
		Write-Log "--- CopyFiles function falied ---"
		throw "Destination path is unreachable"
	}
	
	Write-Log "Searching for files matching your request"
	$files = $null
	if($Recurse) {
		if ($SizeLimit -ne -1) {
			$files = gci $SourceFolder -Include $FileNameWildcards -Recurse | where {$_.length -le 24MB}
		}
		else{
			$files = gci $SourceFolder -Include $FileNameWildcards -Recurse
		}
	}
	else {
		if($files.Count -gt 1) {
			Write-Log "Multiple file name wildcards can be used only if Recurse option is enabled" -type error
			Write-Log "--- CopyFiles function falied ---"
			throw "Cannot handle multiple file name wildcards"
		}		
		if ($SizeLimit -ne -1) {
			$files = gci $SourceFolder -Filter $FileNameWildcards | where {$_.length -le $SizeLimit}
		}
		else{
			$files = gci $SourceFolder -Filter $FileNameWildcards
		}
	}
	if($Newest) {
		if($Count -eq $null -or $Count -eq 0) {
			$Count = 1
		}
		$files = $files | sort lastwritetime -Descending | select -First $Count
	}	
	if (($files | measure).Count -eq 0) {
		Write-Log "There are no files in '$SourceFolder' folder that match '$FileNameWildcards'" -type error
		Write-Log "--- CopyFiles function falied ---"
		throw "No files to copy"
	}
    Write-Log "Theese files will be copied to '$RemoteFolder':"
	foreach ($file in $files) {
		Write-Log "$file"
	}
    
    Write-Log "Copying files from '$SourceFolder' to '$RemoteFolder'"
	$success = $false
	$exception | Out-Null
	while ($success -ne $true -and $time -ge 0) {
	    try	{
			Copy-Item $files -Destination $RemoteFolder -Force
			Start-Sleep -Seconds 5
			$success = $true
		}
		catch {
			$exception = $_
			Start-Sleep -Seconds 2
			$time -= 2
		}
	}
	
	if (!($success)) {
		$exception | fl * -Force
		if ($time -le 0) {
			Write-Log "Waiting for files to be copied has timed out" -type error
		}
		else {
			Write-Log "Unable to copy files to '$RemoteFolder'" -type error
		}
		Write-Log "--- CopyFiles function falied ---"
		throw "Files were not copied"
	}
	
	Write-Log "Files were copied successfully"
	Write-Log "--- CopyFiles function has succeeded ---"
	Write-Log ""
}

##################
# Remote Execute #
##################

function RemoteExecute($MachineName, $UserName, $Password, $FileNameWildcards, $Parameters, $RemoteFolder = "C:\Share", $PsExec = ".\Utils\PsExec.exe", $DefaultWay = 2)
{
	Write-Log "=== RemoteExecute function has started ==="
	$session, $credentials = CreateRemoteSession $MachineName $UserName $Password
	
	Write-Log "Getting full file path from '$MachineName'"
	$files = Invoke-Command -Session $session -ScriptBlock {
		$fullNames = gci $($args[0]) -Include $($args[1]) -Recurse | %{$_.FullName}; $fullNames
	} -ArgumentList $RemoteFolder, $FileNameWildcards
	if(($files | measure).Count -eq 0) {
		Write-Log "There are no files in '$RemoteFolder' folder on '$MachineName' machine that match '$FileNameWildcards'. Breaking script execution." -type error
		Write-Log "Removing remote session for '$UserName' on '$MachineName'"
		Remove-PSSession $session
		Write-Log "--- RemoteExecute function has failed ---"
		throw "No files to run"
	}
		
	if ($DefaultWay -ne 1 -and $DefaultWay -ne 2 -and $DefaultWay -ne 3) {
        $DefaultWay = 2
    }

### Way 1 ###
	if($DefaultWay -eq 1) {
		foreach($file in $files) {
			#$sessionID = Invoke-Command -Session $session -ScriptBlock {Invoke-Expression -Command "query session | Select-String ""$username\s+(\d+)\s+(Active)\s"" | Foreach {$_.Matches[0].Groups[1].Value}"} -ArgumentList $UserName
			Write-Log "Executing '$file' on '$MachineName' machine"
			$command = "$Psexec -accepteula \\$MachineName -u $UserName -p $Password -i 1 ""$file"" $Parameters"
			Invoke-Expression "$command"
		}
	}

### Way 2 ###
	if($DefaultWay -eq 2) {
		foreach($file in $files) {
			Write-Log "Executing '$file $Parameters' on '$MachineName' machine"
			$command = "cmd /c ""$file $Parameters"""
			try {
			Invoke-Command -Session $session -ScriptBlock {
				Invoke-Expression $($args)} -ArgumentList $command
			}
			catch {
				Write-Log "Error launching process '$file'" -type error
				Write-Log "Removing remote session for '$UserName' on '$MachineName'"
				Remove-PSSession $session
				Write-Log "--- RemoteExecute function has failed ---"
				Write-Log ""
				throw "Error launching process '$file'"
			}
		}
	}

### Way 3 ###
	if($DefaultWay -eq 3) {
		foreach ($file in $files) {
			try {
				Write-Log "Executing '$file $Parameters' on '$MachineName' machine"
				Invoke-Command -Session $session -ScriptBlock {
					Start-Process -FilePath $($args[0]) -ArgumentList $($args[1]) -Wait -PassThru -WindowStyle Maximized
				} -ArgumentList $file, $Parameters
			}
			catch {
				Write-Log "Error launching process '$file'" -type error
				Write-Log "Removing remote session for '$UserName' on '$MachineName'"
				Remove-PSSession $session
				Write-Log "--- RemoteExecute function has failed ---"
				Write-Log ""
				throw "Error launching process '$file'"
			}
		}
	}

	Write-Log "Removing remote session for '$UserName' on '$MachineName'"
	Remove-PSSession $session
	Write-Log "--- RemoteExecute function has succeeded ---"
	Write-Log ""
}

#########################
# Stop Remote Processes #
#########################

function StopRemoteProcesses($MachineName, $UserName, $Password, $ProcessNames, $Retries = 20)
{
	Write-Log "=== StopRemoteProcesses function has started ==="
	$session, $credentials = CreateRemoteSession $MachineName $UserName $Password
	
	Write-Log "Function will stop these processes on '$MachineName' machine:"
	foreach ($process in $ProcessNames) {
		Write-Log "$process"
	}
	
	$result = Invoke-Command -Session $session -ScriptBlock {
		foreach ($process in $($args[0])) {
			for ($i = $($args[1]); ($i -gt 0) -and ((Get-Process -name $process -ErrorAction SilentlyContinue | measure).count -gt 0); $i--) {
				Stop-Process -name $process
				Start-Sleep 2
			}
		}
		
		if ((Get-Process -name $process -ErrorAction SilentlyContinue | measure).count -gt 0) {
			return $false
		}
		else {
			return $true
		}		
	} -ArgumentList $ProcessNames, $Retries
	
	if (!($result)) {
		Write-Log "Couldn't stop all the processes" -type error
		Write-Log "=== StopRemoteProcesses function has failed ==="
		throw "Remote processes were not stopped"
	}
	
	Write-Log "All the processes have been stopped successfully"
	Write-Log "Removing remote session for '$UserName' on '$MachineName'"
	Remove-PSSession $session
	Write-Log "=== StopRemoteProcesses function has succeeded ==="
	Write-Log ""
}

########################### Services Management Functions ###########################

#############################
# Wait For Service To Start #
#############################

function WaitForServiceToStart($MachineName, $UserName, $Password, $ServiceName, $Timeout = 300)
{
	Write-Log "=== WaitForServiceToStart function has started ==="
	$session, $credentials = CreateRemoteSession $MachineName $UserName $Password

	Write-Log "Waiting for '$ServiceName' service to start"	
	[string]$status = Invoke-Command -Session $session -ScriptBlock {
		$time = $($args[0])
		$service = $null
		while (($service -eq $null) -and ($time -gt 0)) {
			try {
				$service = Get-Service $($args[1])}
			catch {
				Start-Sleep -Seconds 5
				$time -= 5}}
		while (((Get-Service $($args[1])).Status.ToString() -ne "Running") -and ($time -gt 0)) {
			Start-Service $($args[1])
			Start-Sleep -Seconds 5
			$time -= 5}
			return (Get-Service $($args[1])).Status.ToString()
		} -ArgumentList $Timeout, $ServiceName

	if ($status -eq $null) {
		Write-Log "Could not find any service with service name '$ServiceName' for $Timeout seconds" -type error
		Write-Log "Removing remote session for '$UserName' on '$MachineName'"
		Remove-PSSession $session
		Write-Log "--- WaitForServiceToStart function has failed ---"
		Write-Log ""
		throw "No '$ServiceName' service is found"}

	if ($status -ne "Running") {
		Write-Log "Waiting for '$ServiceName' service Running status has timed out, service status is '$status'" -type error
		Write-Log "Removing remote session for '$UserName' on '$MachineName'"
		Remove-PSSession $session
		Write-Log "--- WaitForServiceToStart function has failed ---"
		Write-Log ""
		throw "Waiting for '$ServiceName' service has timed out"}
		
	Write-Log "'$ServiceName' service has started successfully"
	Write-Log "Removing remote session for '$UserName' on '$MachineName'"
	Remove-PSSession $session
	Write-Log "--- WaitForServiceToStart function has succeeded ---"
	Write-Log ""
}

############################# Jobs Management Functions #############################

###########################
# Wait for jobs to finish #
###########################

function WaitWhileJobsRun ([System.Collections.ArrayList]$Jobs)
{
	Write-Log "Waiting for jobs to finish"
	while ($Jobs.Count -gt 0)
	{
		for ($i = 0; $i -lt $jobs.Count; $i++)
		{
			$state = $jobs[$i].State
			if ($state -ne "Running")
			{
				$message1 = $jobs[$i].Name + " job has finished with status " + $jobs[$i].State
				Write-Log $message1
				$message2 = "Removing " + $jobs[$i].Name + " job"
				Write-Log $message2
				$jobId = $jobs[$i].Id
				Remove-Job -Id $jobId
				Write-Log "Job removed"
				$jobs[$i] = $null
			}
		}
		
		Write-Log "Waiting..."
		
		Start-Sleep -s 10
		
		$Jobs = $Jobs -ne $null
	}
	
	Write-Log "All jobs have finished"
}

####################### Scheduled Tasks Management Functions ########################

########################
# Wait While Task Runs #
########################

function WaitWhileTaskRuns([string]$TaskName) {
	Write-Log "Connecting to scheduled tasks management service"
	$ts = New-Object -ComObject Schedule.Service
	$ts.Connect()

	Write-Log "Waiting for '$TaskName' task to finish"
	while (Get-TaskRunningState $ts $TaskName) {
		start-sleep -seconds 5
	}	
	Write-Log "Task has finished"
}

##########################
# Get Task Running State #
##########################

function Get-TaskRunningState([object]$taskScheduler, [string]$taskNames){
    [bool]$result = $false
    $taskNames.split(",") |
            foreach {
                $task = $_
                $res = $taskScheduler.GetRunningTasks(1) | Where-Object { $_.Name -eq $task }
 
                if($res -ne $null){
                    Write-Log $task " currently running"
                    $result = $true;
                }
            }
    $result
}

############################# Time Management Functions #############################

####################
# Get Shifted Time $
####################

function GetShiftedTime([int]$TimeShiftInMinutes)
{
	return ('{0:HH:mm}' -f (Get-Date).AddMinutes($TimeShiftInMinutes))
}

################################ Secondary Functions ################################

# Wait For Path To Be Accessible #
function WaitForPathToBeAccessible ($Path, $UserName, $Password, $Timeout)
{
	Write-Log "= WaitForPathToBeAccessible function has started ="
	$time = $Timeout
	if ((![string]::IsNullOrEmpty($UserName)) -and (![string]::IsNullOrEmpty($Password))) {
		Write-Log "Trying to reach '$Path' path using '$UserName' account"
		while(!(Test-Path $Path)) {
			net use $Path $Password /user:$UserName | Out-Null
			Start-Sleep -Seconds 2
			$time -= 2
			if	($time -le 0) {
				Write-Log "'$Path' path was unreachable for more than $Timeout seconds" -type error
				Write-Log "= WaitForPathToBeAccessible function has failed ="
				return $false
			}
		}
	}
	else {
		Write-Log "Trying to reach '$Path' path"
		while(!(Test-Path $Path)) {
			Start-Sleep -Seconds 2
			$time -= 2
			if	($time -le 0) {
				Write-Log "'$Path' path was unreachable for more than $Timeout seconds" -type error
				Write-Log "= WaitForPathToBeAccessible function has failed ="
				return $false
			}
		}
	}
	
	Write-Log "$Path path is reachable"
	Write-Log "= WaitForPathToBeAccessible function has succeeded ="
	return $true
}

# Create Remote Session #
function CreateRemoteSession ($MachineName, $UserName, $Password, $Timeout = 600)
{
 	Write-Log "= CreateRemoteSession function has started ="
	Write-Log "Creating credentials for '$UserName'"
	$securePass = ConvertTo-SecureString -String $Password -AsPlainText -Force
	$credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $UserName, $securePass
	
	Write-Log "Creating remote session for '$UserName' on '$MachineName'"
	while($Timeout -gt 0) {
		$session = New-PSSession $MachineName -Credential $credentials -ErrorAction SilentlyContinue
		if($session -eq $null) {
			$Timeout -= 5
			Start-Sleep -Seconds 5}
		else {
			Write-Log "Session has been created successfully, returning session"
			Write-Log "- CreateRemoteSession function has succeeded -"
			return $session, $credentials}}
	
	Write-Log "Failed to connect to '$MachineName'" -type error
	Write-Log "- CreateRemoteSession function falied -"
	throw "Failed to create remote session"
}

# Get Snapshot #
function GetSnapshot($ServerName, $UserName, $Password, $VmName, $SnapshotName)
{
	Write-Log "= GetSnapshot function has started ="
	$session, $credentials = CreateRemoteSession $ServerName $UserName $Password
	
	Write-Log "Getting '$SnapshotName' snapshot for '$VmName' machine"
	$snapshot = Invoke-Command -Session $session -ScriptBlock {
		Import-Module HyperV; Get-VMSnapshot -VM $($args[0]) -Name $($args[1])} -ArgumentList $VmName, $SnapshotName
	if($snapshot -eq $null)	{
		Write-Log "Snapshot '$SnapshotName' is not found" -type error
		Write-Log "Removing remote session for '$UserName' on '$ServerName'"
		Remove-PSSession $session
		Write-Log "- GetSnapshot function has failed -"
		throw "Failed to get snapshot '$SnapshotName'"}
	Write-Log "Returning snapshot"
	Write-Log "- GetSnapshot function has succeeded -"
	return $snapshot, $session
}

############################### Function Combinations ###############################

# Prepare Environment #
function PrepareEnvironment($Server, $ServerUserName, $ServerPassword, $SnapshotName, $VmName, $MachineName, $MachineUserName, $MachinePassword, $MachineShare, $FileToExecute, $SourcePath, $SourcePathUserName, $SourcePathPassword, $Arguments, $ServiceToWait)
{
	RestoreVM $Server $ServerUserName $ServerPassword $VmName $SnapshotName
	StartVM $Server $ServerUserName $ServerPassword $VmName
	if ($FileToExecute -ne $null) {
	CopyFiles $FileToExecute $SourcePath $SourcePathUserName $SourcePathPassword $MachineShare $MachineUserName $MachinePassword
	RemoteExecute $MachineName $MachineUserName $MachinePassword $FileToExecute $Arguments
	WaitForServiceToStart $MachineName $MachineUserName $MachinePassword $ServiceToWait }
}