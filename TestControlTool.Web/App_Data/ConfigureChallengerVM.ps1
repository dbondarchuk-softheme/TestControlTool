<#
.Synopsys
    This PowerShell script is used to perform specific configuration of virtual machines on Challenger.
.Description
    This Powershell script configures the following:
    - sets the computer name
	- enables automatic logon for a specified account
    - disables Windows Firewall
    - enables Windows file sharing
    - creates shared folder for Administrator account with full permissions
	- performs network configuration
    - enables Remote Desktop access to this machine
    - disables User Account Control
	- sets desired Time Zone
    - enables High Performance mode for this machine
    - enables PSRemoting
    - sets Trusted Hosts value to "*"
    The person executing the script will have to provide the desired computer name,
	user name and password for autologon, shared folder path and a time zone name.
.Notes
    Name: .\ConfigureChallengerVM.ps1
    Author: Andrey Annenko
    Last Edit: 09/23/2013
    Keywords: Configure Challenger Virtual Machine
.Links
    https://plus.google.com/113105878118847972795
#>

Param (
	[string]$newComputerName,
    [string]$autoLogonUserName,
	[string]$autoLogonPassword,
    [string]$sharedFolderPath,
	[string]$ipAddress,
	[string]$subnetMask,
	[string]$defaultGateway,
	[string]$dns1,
	[string]$dns2,
	[string]$timeZoneName,
	[string]$logName
)

$dt=Get-Date -Format "dd-MM-yyyy"
New-Item -ItemType directory log -Force | out-null

$logfilename="log\"+$dt+"_"+$logName+"_LOG.log"
[int]$errorcount=0
[int]$warningcount=0

function Write-Log
{param($message,[string]$type="info",[string]$logfile=$logfilename,[switch]$silent)
    $dt=Get-Date -Format "dd.MM.yyyy HH:mm:ss"
    $msg=$dt + "`t" + $type + "`t" + $message
    Out-File -FilePath $logfile -InputObject $msg -Append -encoding unicode
    if (-not $silent.IsPresent)
    {
        switch ( $type.toLower() )
        {
            "error"
            {
                $errorcount++
                write-host $msg -ForegroundColor red
            }
            "warning"
            {
                $warningcount++
                write-host $msg -ForegroundColor yellow
            }
            "completed"
            {
                write-host $msg -ForegroundColor green
            }
            "info"
            {
                write-host $msg
            }
            default
            {
                write-host $msg
            }
        }
    }
}

Write-Log ""
Write-Log "----- Checking if this script is running from Administrator account -----"
Write-Log ""

$user = [Security.Principal.WindowsIdentity]::GetCurrent();
$isRunninFromAdmin = (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)

if(!$isRunninFromAdmin)
{
    Write-Log ""
    Write-Log "----- Please run this script as Administrator -----" "error"
    Write-Log ""
    Read-Host
	throw
}

Write-Log ""
Write-Log "----- This script is running from Administrator account -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Setting computer name -----"
Write-Log ""

$machine = Get-WmiObject Win32_ComputerSystem
while([String]::IsNullOrEmpty($newComputerName))
{
	Write-Log "Invalid computer name is specified" "error"
	$newComputerName = Read-Host "Enter the Computer Name you want to use"
}

$result = $machine.Rename($newComputerName)
Write-Log ""
if($result.ReturnValue -eq "0")
{
	Write-Log "----- Computer name '$newComputerName' will take effect after a reboot -----"
}
else
{
	Write-Warning "Computer renaming return code is different from 0! Return code is: $result.ReturnValue" -warningaction Inquire
}

Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Enabling automatic logon -----"
Write-Log ""

Add-Type -AssemblyName system.DirectoryServices.accountmanagement
$DS = New-Object System.DirectoryServices.AccountManagement.PrincipalContext([System.DirectoryServices.AccountManagement.ContextType]::Machine)
$validation = $false
while(!$validation)
{
    if([String]::IsNullOrEmpty($autoLogonUserName))
    {
        $autoLogonUserName = Read-Host "Enter a User Name for automatic logon"
    }
	
	if([String]::IsNullOrEmpty($autoLogonPassword))
    {
        $autoLogonPassword = Read-Host "Enter a Password for automatic logon"
    }

    $validation = $DS.ValidateCredentials($autoLogonUserName, $autoLogonPassword)
	if(!$validation)
	{
		Write-Log ""
		Write-Log "Invalid user name or password!" "error"
		Write-Log ""
		
		$autoLogonUserName = $null
		$autoLogonPassword = $null
	}
}

New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name "AutoAdminLogon" -Value 1 -Force
New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name "DefaultPassword" -Value $autoLogonPassword -Force
New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name "DefaultUserName" -Value $autoLogonUserName -Force

Write-Log ""
Write-Log "----- Automatic logon has been enabled for $autoLogonUserName -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Disabling the Windows Firewall -----"
Write-Log ""

netsh advfirewall set allprofiles state off

Write-Log ""
Write-Log "----- Windows Firewall has been disabled -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Enabling file sharing -----"
Write-Log ""

netsh firewall set service type=fileandprint mode=enable profile=all

Write-Log ""
Write-Log "----- File Sharing has been enabled -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Creating a shared folder for Administrator account -----"
Write-Log ""

$isSharedFolderPathOk = [String]::IsNullOrEmpty($sharedFolderPath)
if(!($isSharedFolderPathOk))
{
    $sharedFolderPath = Read-Host "Enter a path for a shared folder to be created at"
	if(!([String]::IsNullOrEmpty($sharedFolderPath)))
	{
		$isSharedFolderPathOk = $true
	}
	else
	{
		Write-Log "Shared folder path cannot be an empty string" "error"
	}
}

if(!(Test-path $sharedFolderPath))
{
    New-Item -Path $sharedFolderPath -Type Directory -ErrorAction Stop

    Write-Log ""
    Write-Log "----- Folder '$sharedFolderPath' has been created -----"
    Write-Log ""
}
else
{
    Write-Log ""
    Write-Log "----- Folder '$sharedFolderPath' already exists -----"
    Write-Log ""
}

$shareName = Split-Path $sharedFolderPath -Leaf

$sd = ([WMIClass]"Win32_SecurityDescriptor").CreateInstance()
$name = "Administrator"

#Share with the user
$ACE = ([WMIClass] "Win32_ACE").CreateInstance()
$Trustee = ([WMIClass] "Win32_Trustee").CreateInstance()
$Trustee.Name = $name
$Trustee.Domain = $null
#original example assigned this, but I found it worked better if I left it empty
#$Trustee.SID = ([wmi]"win32_userAccount.Domain='york.edu',Name='$name'").sid   
$ace.AccessMask = 2032127 # 2032127 - full control, 1245631 - change, 1179817 - read
$ace.AceFlags = 3 #Should almost always be three. Really. don't change it.
$ace.AceType = 0 # 0 = allow, 1 = deny
$ACE.Trustee = $Trustee 
$sd.DACL += $ACE.psObject.baseobject

$Shares = [WMICLASS]"Win32_Share"
$InParams = $Shares.psbase.GetMethodParameters("Create")
$InParams.Access = $sd
$InParams.Description = $shareName
$InParams.MaximumAllowed = $null
$InParams.Name = $shareName
$InParams.Password = $null
$InParams.Path = $sharedFolderPath
$InParams.Type = [uint32]0

$R = $Shares.PSBase.InvokeMethod("Create", $InParams, $Null)
$returnValue = $R.ReturnValue
switch ($($R.ReturnValue))
{
        0 {$returnValue = "Success"; break}
        2 {$returnValue = "Access Denied"; break}
        8 {$returnValue = "Unknown Failure"; break}
        9 {$returnValue = "Invalid Name"; break}
        10 {$returnValue = "Invalid Level"; break}
        21 {$returnValue = "Invalid Parameter"; break}
        22 {$returnValue = "Duplicate Share"; break}
        23 {$returnValue = "Redirected Path"; break}
        24 {$returnValue = "Unknown Device or Directory"; break}
        25 {$returnValue = "Network Name Not Found"; break}
        default {$returnValue = "*** Unknown Error ***"; break}
}

$message = "Windows share '$shareName' creation result is:"

Write-Log ""
if($returnValue -ne "Success")
{
	Write-Warning "$message $returnValue" -warningaction Inquire
}
else
{
	Write-Log "----- $message $returnValue -----"
}

Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Performing network configuration -----"
Write-Log ""

[System.Net.IPAddress]$ipObj = $null
while(!([System.Net.IPAddress]::TryParse($ipAddress, [ref]$ipObj)))
{
	Write-Log "Invalid IP address has been specified" "error"
    $ipAddress = Read-Host "Enter IP address"
}

[System.Net.IPAddress]$maskObj = $null
while(!([System.Net.IPAddress]::TryParse($subnetMask, [ref]$maskObj)))
{
	Write-Log "Invalid subnet mask has been specified" "error"
    $subnetMask = Read-Host "Enter subnet mask address"
}

[System.Net.IPAddress]$gatewayObj = $null
while(!([System.Net.IPAddress]::TryParse($defaultGateway, [ref]$gatewayObj)))
{
	Write-Log "Invalid default gateway has been specified" "error"
    $defaultGateway = Read-Host "Enter default gateway address"
}

[System.Net.IPAddress]$mainDnsObj = $null
while(!([System.Net.IPAddress]::TryParse($dns1, [ref]$mainDnsObj)))
{
	Write-Log "Invalid primary DNS server address has been specified" "error"
    $dns1 = Read-Host "Enter primary DNS server address"
}

[System.Net.IPAddress]$alternateDnsObj = $null
while(!([System.Net.IPAddress]::TryParse($dns2, [ref]$alternateDnsObj)))
{
	Write-Log "Invalid alternate DNS server address has been specified" "error"
    $dns2 = Read-Host "Enter alternate DNS server address"
}

$NIC = Get-WMIObject Win32_NetworkAdapterConfiguration -computername . | where {$_.Description.Contains("Network") -and $_.IPEnabled}
if(($NIC | measure).Count -gt 1) {$NIC = $NIC[0]}
[System.Array]$dnsServers = "$dns1", "$dns2"
$NIC.EnableStatic("$ipAddress", "$subnetMask")
$NIC.SetGateways("$defaultGateway")
$NIC.SetDnsServerSearchOrder($dnsServers)
$NIC.SetDynamicDNSRegistration($false)

Write-Log ""
Write-Log "----- Network configuration script has been created -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Enabling Remote Desktop -----"
Write-Log ""

set-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -name "fDenyTSConnections" -Value 0 -erroraction silentlycontinue
if (-not $?) {new-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -name "fDenyTSConnections" -Value 0 -PropertyType dword}

set-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -name "UserAuthentication" -Value 1 -erroraction silentlycontinue 
if (-not $?) {new-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -name "UserAuthentication" -Value 1 -PropertyType dword}

Write-Log ""
Write-Log "----- Remote Desktop has been enabled -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Disabling User Account Control -----"
Write-Log ""

set-ItemProperty -Path 'HKLM:\Software\Microsoft\Windows\CurrentVersion\policies\system' -name "EnableLUA" -Value 0 -erroraction silentlycontinue
if (-not $?) {new-ItemProperty -Path 'HKLM:\Software\Microsoft\Windows\CurrentVersion\policies\system' -name "EnableLUA" -Value 0 -PropertyType dword}

Write-Log ""
Write-Log "----- User Account Control has been disabled -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Setting the time zone -----"
Write-Log ""

while ([string]::IsNullOrEmpty($timeZoneName))
{
	Write-Log "Invalid time zone has been specified" "error"
	Write-Log "To get the list of all time zones execute 'tzutil /l' command in PowerShell"
	$timeZoneName = Read-Host "Enter a Time Zone name (`"Eastern Standard Time`" is by default, use `"FLE Standard Time`" for GMT +2:00)."
}

tzutil /s $timeZoneName

Write-Log ""
Write-Log "----- Time zone has been set -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Enabling High Performance power plan -----"
Write-Log ""

Get-WmiObject -Class Win32_PowerPlan -Namespace root\cimv2\power -Filter "ElementName='High performance'" | Invoke-WmiMethod -Name Activate

Write-Log ""
Write-Log "----- High Performance power plan has been enabled -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Enabling PSRemoting -----"
Write-Log ""

Enable-PSRemoting -Force

Write-Log ""
Write-Log "----- PSRemoting has been enabled -----"
Write-Log ""
Write-Log "================================================="

Write-Log ""
Write-Log "----- Setting Trusted hosts to "*" -----"
Write-Log ""

Set-Item WSMan:\localhost\Client\TrustedHosts * -Force

Write-Log ""
Write-Log "----- Trusted hosts has been set to "*" -----"
Write-Log ""
Write-Log "================================================="

Write-Log "" "completed"
Write-Log "----- Machine configuration is finished -----" "completed"
Write-Log "" "completed"
Write-Log "=================================================" "completed"