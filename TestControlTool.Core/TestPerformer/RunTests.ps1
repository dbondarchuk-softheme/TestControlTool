###################################### Header #######################################

Param($Machine, $UserName, $Password, $ShareFolder, $FileName, $PsExec, $LogFolder)

trap [Exception] {
		write-error 'Caught exception: '
		write-error $_.Exception.ToString()
		break;
	}

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

################################# Preparation Part ##################################

Get-ScriptDirectory | Set-Location
. .\Logging.ps1 $($LogFolder)
. .\Functions.ps1 

################################ Tests Running Part #################################

$SourceFolder = Split-Path $FileName -parent
$FileName = Split-Path $FileName -leaf

Write-Log "Local machine information:"
Write-Log "Computer name is '$env:COMPUTERNAME'"
$execPolicy = Get-ExecutionPolicy
$trustedHosts = Get-Item wsman:\localhost\client\trustedhosts
Write-Log "Local execution policy parameter is set to '$execPolicy'"
Write-Log "Local TrustedHosts parameter is set to '$trustedHosts'"

Write-Log "Removing old Test Performer"
$session, $credentials = CreateRemoteSession $($Machine) $($UserName) $($Password)
rv credentials

$localPath = Invoke-Command -Session $session -ScriptBlock {Invoke-Expression -Command "(Get-WmiObject Win32_Share -filter `"Name = '$args'`").path"} -ArgumentList $ShareFolder

Write-Log "Stopping processes (if any) that could block tests execution"
StopRemoteProcesses $($Machine) $($UserName) $($Password) "IEDriverServer.exe", "chromedriver.exe", "TestPerformer.exe" 30

Write-Log "Remove old files"
Invoke-Command -Session $session -ScriptBlock { Remove-Item "$($args)\*" -recurse -force } -ArgumentList $localPath

Write-Log "Creating Reports folder"
Invoke-Command -Session $session -ScriptBlock { New-Item -Path "$args" -Name Reports -ItemType Directory -Force} -ArgumentList $localPath

Write-Log "Copying WebGuiAutomation project to the execution environment"
CopyFiles "*" $(Get-ScriptDirectory) $null $null $("\\" + $Machine + "\" + $ShareFolder) $($UserName) $($Password) $true $false

Write-Log "Copying test suites to the execution environment"
CopyFiles $($FileName) $($SourceFolder) $null $null $("\\" + $Machine + "\" + $ShareFolder) "Administrator" "123asdQ" $true $false
		
Start-Sleep -s 60
		
Write-Log "Executing tests on $Machine"
$arguments = """Name"" ""$localPath\Reports"" ""//"" ""Load"" ""$localPath\WebGuiAutomationTrunk.Scripts.dll"" ""//"" ""Run"" ""$localPath\$FileName"" ""quiet"""
RemoteExecute $($Machine) $($UserName) $($Password) "TestPerformer.exe" $arguments "$localPath" $PsExec 1
		
Write-Log "Getting reports and logs"
CopyFiles "*" $("\\" + $Machine + "\" + $ShareFolder + "\Reports") $($UserName) $($Password) $($LogFolder) $null $null $true $false

Write-Log "Removing remote session"
Remove-PSSession $session

Write-Log "Tests execution is completed"