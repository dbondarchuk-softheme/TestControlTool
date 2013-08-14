
############################## VM Management Functions ##############################

##############
# Restore VM #
##############

function RestoreVM($ServerName, $UserName, $Password, $VmName, $SnapshotName)
{
	Write-Log "=== RestoreVM function started ==="
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
		Write-Log "--- RestoreVM function failed ---"
		Write-Log ""
		throw "Couldn't perform 'Restore-VMSnapshot' on '$ServerName' server"}
	Write-Log "'$VmName' machine was successfully restored to '$SnapshotName' snapshot"
	
	Write-Log "Removing remote session for '$UserName' on '$ServerName'"
	Remove-PSSession $session
	Write-Log "--- RestoreVM function succeeded ---"
	Write-Log ""
}

############
# Start VM #
############

function StartVM($ServerName, $UserName, $Password, $VmName)
{
	Write-Log "=== StartVM function started ==="
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
		Write-Log "--- StartVM function failed ---"
		Write-Log ""
		throw "Couldn't start '$VmName' machine"}
	Write-Log "'$VmName' remote machine has started successfully"
	
	Write-Log "Removing remote session for '$UserName' on '$ServerName'"
	Remove-PSSession $session
	Write-Log "--- StartVM function succeeded ---"
	Write-Log ""
}

###########
# Stop VM #
###########

function StopVM($ServerName, $UserName, $Password, $VmName)
{
	Write-Log "=== StopVM function started ==="
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
		Write-Log "--- StopVM function failed ---"
		Write-Log ""
		throw "Couldn't start '$VmName' machine"}
	Write-Log "'$VmName' remote machine has stopped successfully"
	
	Write-Log "Removing remote session for '$UserName' on '$ServerName'"
	Write-Log "--- StopVM function succeeded ---"
	Write-Log ""
	Remove-PSSession $session
}

############################ Files Management Functions #############################

##############
# Copy Files #
##############

function CopyFiles($FileNameWildcards, $SourceFolder, $SourceUserName, $SourcePassword, $RemoteFolder, $RemoteUserName, $RemotePassword, $isLinux, $Timeout = 900)
{
	Write-Log "=== CopyFiles function started ==="
	
	$time = $Timeout
	
	Write-Log "Trying to reach source folder '$SourceFolder' using '$SourceUserName' account"
	while(!(Test-Path $SourceFolder)) {
		if ((![string]::IsNullOrEmpty($SourceUserName)) -and (![string]::IsNullOrEmpty($SourcePassword))) {
		net use $SourceFolder $SourcePassword /USER:$SourceUserName | Out-Null}
		Start-Sleep -Seconds 2
		$time -= 2
		if	($time -le 0) {
			Write-Log "'$SourceFolder' path was unreachable for $Timeout seconds" -type error
			Write-Log "--- CopyFiles function falied ---"
			throw "Waiting for source folder to be reachable has timed out"}
	}
	Write-Log "Source folder reached"
	
	Write-Log "Trying to reach destination folder '$RemoteFolder' using '$RemoteUserName' account"
	while(!(Test-Path $RemoteFolder)) {
		if ((![string]::IsNullOrEmpty($RemoteUserName)) -and (![string]::IsNullOrEmpty($RemotePassword))) {
		net use $RemoteFolder $RemotePassword /USER:$RemoteUserName | Out-Null}
		Start-Sleep -Seconds 2
		$time -= 2
		if	($time -le 0) {
			Write-Log "'$RemoteFolder' path was unreachable for $Timeout seconds" -type error
			Write-Log "--- CopyFiles function falied ---"
			throw "Waiting for destination folder to be reachable has timed out"}
	}
	Write-Log "Destination folder reached"
	
    Write-Log "Searching for files matching your request"
	$files = gci $SourceFolder -Include $FileNameWildcards -Recurse | sort lastwritetime -Descending | select -First 1
	if (($files | measure).Count -eq 0) {
		Write-Log "There are no files in '$SourceFolder' folder that match '$FileNameWildcards'" -type error
		Write-Log "--- CopyFiles function falied ---"
		throw "No files to copy"}
    Write-Log "Theese files will be copied to '$RemoteFolder':"
	foreach ($file in $files) {Write-Log "$file"}
    
    Write-Log "Copying files from '$SourceFolder' to '$RemoteFolder'"
	$success = $false
	$exception | Out-Null
	while ($success -ne $true -and $time -ge 0) {
	    try	{
			Copy-Item $files -Destination $RemoteFolder -Force
			if ($isLinux -ne 0){
				$fileName = Split-Path $file -Leaf
				Rename-Item -Path $($RemoteFolder + "\" + $fileName) -NewName $("agentLinux.sh") 			
			}
			Start-Sleep -Seconds 5
			$success = $true}
		catch {
			$exception = $_
			Start-Sleep -Seconds 2
			$time -= 2}
	}
	
	if ($success -eq $true)	{
		Write-Log "Files were copied successfully"
		Write-Log "--- CopyFiles function succeeded ---"
		Write-Log ""}
	else {
		$exception | fl * -Force
		if ($time -le 0) {
			Write-Log "Waiting for files to be copied has timed out" -type error}
		else {
			Write-Log "Unable to copy files to '$RemoteFolder'" -type error}
		Write-Log "--- CopyFiles function falied ---"
		throw "Files were not copied"}
}

##################
# Remote Execute #
##################

function LinuxRemoteExecute($MachineName, $UserName, $Password){
	Write-Log "=== RemoteExecute function started ==="
	
	$dt=Get-Date -Format "dd.MM.yyyy"
	
	$command = "echo y | C:\plink.exe -ssh -P 22 -pw $Password $UserName@""$MachineName"" exit"
	
	Invoke-Expression $command
	
	$command = "C:\plink.exe -ssh -P 22 -pw $Password $UserName@""$MachineName"" ""sudo su -c '/home/administrator/reminst/agentLinux.sh -f -p 8006 -g administrator'"" > log\$dt-$MachineName.txt"
	
	Invoke-Expression $command
	
	Write-Log "--- RemoteExecute function has finished ---"
	Write-Log ""
}


function RemoteExecute($MachineName, $UserName, $Password, $FileNameWildcards, $Parameters, $ShareFolder, $PsExec = "C:\Scripts\PsTools\PsExec.exe")
{
	Write-Log "=== RemoteExecute function started ==="
	$session, $credentials = CreateRemoteSession $MachineName $UserName $Password
	
	$RemoteFolder = Invoke-Command -Session $session -ScriptBlock {Invoke-Expression -Command "(Get-WmiObject Win32_Share -filter `"Name = '$args'`").path"} -ArgumentList $ShareFolder
	
	Write-Log "Getting full file path from '$MachineName'"
	$files = Invoke-Command -Session $session -ScriptBlock {
		$fullNames = gci $($args[0]) -Include $($args[1]) -Recurse | %{$_.FullName}; $fullNames
		} -ArgumentList $RemoteFolder, $FileNameWildcards
	if (($files | measure).Count -eq 0) {
		Write-Log "There are no files in '$RemoteFolder' folder on '$MachineName' machine that match '$FileNameWildcards'. Breaking script execution." -type error
		Write-Log "Removing remote session for '$UserName' on '$MachineName'"
		Remove-PSSession $session
		Write-Log "--- RemoteExecute function failed ---"
		throw "No files to run"}
		
### Way 1 ###
		
#	foreach($file in $files) {
#		Write-Log "Executing '$file' on '$MachineName' machine"
#		$command = "$Psexec \\$MachineName -u $UserName -p $Password $file $Parameters"
#		Invoke-Expression "$command"}

### Way 2 ###

	foreach($file in $files) {
		Write-Log "Executing '$file $Parameters' on '$MachineName' machine"
		$command = "cmd /c ""$file $Parameters"""
		try {
		Invoke-Command -Session $session -ScriptBlock {
			Invoke-Expression $($args)} -ArgumentList $command}
		catch {
			Write-Log "Error launching process '$file'" -type error
			Write-Log "Removing remote session for '$UserName' on '$MachineName'"
			Remove-PSSession $session
			Write-Log "--- RemoteExecute function failed ---"
			Write-Log ""
			throw "Error launching process '$file'"}}

### Way 3 ###

#	foreach ($file in $files) {
#		try {
#			Write-Log "Executing '$file $Parameters' on '$MachineName' machine"
#			Invoke-Command -Session $session -ScriptBlock {
#				Start-Process -FilePath $($args[0]) -ArgumentList $($args[1]) -Wait -PassThru
#				} -ArgumentList $file, $Parameters}
#		catch {
#			Write-Log "Error launching process '$file'" -type error
#			Write-Log "Removing remote session for '$UserName' on '$MachineName'"
#			Remove-PSSession $session
#			Write-Log "--- RemoteExecute function failed ---"
#			Write-Log ""
#			throw "Error launching process '$file'"}}

	Write-Log "Removing remote session for '$UserName' on '$MachineName'"
	Remove-PSSession $session
	Write-Log "--- RemoteExecute function succeeded ---"
	Write-Log ""
}


########################### Windows Management Functions ############################

#############################
# Wait For Service To Start #
#############################

function WaitForServiceToStart($MachineName, $UserName, $Password, $ServiceName, $Timeout = 600)
{
	Write-Log "=== WaitForServiceToStart function started ==="
	$session, $credentials = CreateRemoteSession $MachineName $UserName $Password

	Write-Log "Waiting for '$ServiceName' service to start"	
	[string]$status = Invoke-Command -Session $session -ScriptBlock {
		$time = $($args[0])
		$service = $null
		while (($service -eq $null) -and ($time -gt 0)) {
			try {
				$service = Get-Service -Name $($args[1])}
			catch {
				Start-Sleep -Seconds 5
				$time -= 5}}
		$serviceStatusString = $service.Status.ToString()
		while (($serviceStatusString -ne "Running") -and ($time -gt 0)) {
			Start-Service $($args[1])
			$serviceStatusString = (Get-Service $($args[1])).Status.ToString()
			Start-Sleep -Seconds 5
			$time -= 5}
			return $service.Status.ToString()
		} -ArgumentList $Timeout, $ServiceName

	if ($status -eq $null) {
		Write-Log "Could not find any service with service name '$ServiceName' for $Timeout seconds" -type error
		Write-Log "Removing remote session for '$UserName' on '$MachineName'"
		Remove-PSSession $session
		Write-Log "--- WaitForServiceToStart function failed ---"
		Write-Log ""
		throw "No '$ServiceName' service is found"}

	if ($status -ne "Running") {
		Write-Log "Waiting for '$ServiceName' service Running status has timed out, service status is '$status'" -type error
		Write-Log "Removing remote session for '$UserName' on '$MachineName'"
		Remove-PSSession $session		
		Write-Log "--- WaitForServiceToStart function failed ---"
		Write-Log ""
		throw "Waiting for '$ServiceName' service has timed out"}
		
	Write-Log "'$ServiceName' service has started successfully"
	Write-Log "Removing remote session for '$UserName' on '$MachineName'"
	Remove-PSSession $session
	Write-Log "--- WaitForServiceToStart function succeeded ---"
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

################################ Secondary Functions ################################

# Create Remote Session #
function CreateRemoteSession ($MachineName, $UserName, $Password, $Timeout = 600)
{
 	Write-Log "= CreateRemoteSession function started ="
	Write-Log "Creating credentials for '$UserName'"
	$securePass = ConvertTo-SecureString -String $Password -AsPlainText -Force
	$credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $UserName, $securePass
	
	Write-Log "Creating remote session for '$UserName' on '$MachineName'"
	while($Timeout -gt 0) {
		$session = New-PSSession $MachineName -Credential $credentials -ErrorAction SilentlyContinue
		Write-Log " This ssession '$session'"
		if($session -eq $null) {
			$Timeout -= 5
			Start-Sleep -Seconds 5}
		else {
			Write-Log "Session has been created successfully, returning session"
			Write-Log "- CreateRemoteSession function succeeded -"
			return $session, $credentials}}
	
	Write-Log "Failed to connect to '$MachineName'" -type error
	Write-Log "- CreateRemoteSession function falied -"
	throw "Script execution failed due to remote session creation error"
}

# Get Snapshot #
function GetSnapshot($ServerName, $UserName, $Password, $VmName, $SnapshotName)
{
	Write-Log "= GetSnapshot function started ="
	$session, $credentials = CreateRemoteSession $ServerName $UserName $Password
	
	Write-Log "Getting '$SnapshotName' snapshot for '$VmName' machine"
	$snapshot = Invoke-Command -Session $session -ScriptBlock {
		Import-Module HyperV; Get-VMSnapshot -VM $($args[0]) -Name $($args[1])} -ArgumentList $VmName, $SnapshotName
	if($snapshot -eq $null)	{
		Write-Log "Snapshot '$SnapshotName' is not found" -type error
		Write-Log "Removing remote session for '$UserName' on '$ServerName'"
		Remove-PSSession $session
		Write-Log "- GetSnapshot function failed -"
		throw "Validate parameters"}
	Write-Log "Returning snapshot"
	Write-Log "- GetSnapshot function succeeded -"
	return $snapshot, $session
}

############################### Function Combinations ###############################

# Prepare Environment #
function PrepareEnvironment($Server, $ServerUserName, $ServerPassword, $SnapshotName, $VmName, $MachineName, $MachineUserName, $MachinePassword, $MachineShare, $FileToExecute, $SourcePath, $SourcePathUserName, $SourcePathPassword, $Arguments, $ServiceToWait, $isLinux, $Action, $ShareFolder)
{
	if (($Action -eq "Deploy") -or  ($Action -eq "DeployInstall")){
		Write-Log "Starting deploy job"

		RestoreVM $Server $ServerUserName $ServerPassword $VmName $SnapshotName
		StartVM $Server $ServerUserName $ServerPassword $VmName
	}
	if (($Action -eq "Install") -or ($Action -eq "DeployInstall")){
		Write-Log "Starting installation job"
		
		CopyFiles $FileToExecute $SourcePath $SourcePathUserName $SourcePathPassword $MachineShare $MachineUserName $MachinePassword $isLinux
		if ($isLinux -eq 0){
			RemoteExecute $MachineName $MachineUserName $MachinePassword $FileToExecute $Arguments $ShareFolder 
		}
		else{
			LinuxRemoteExecute $MachineName $MachineUserName $MachinePassword
		}
		WaitForServiceToStart $MachineName $MachineUserName $MachinePassword $ServiceToWait
	}
}