param([string]$LogFolder = "", [string]$LogFileName = "")

$dt=Get-Date -Format "dd-MM-yyyy"
New-Item -ItemType directory log -Force | out-null

$global:logfilename=$LogFolder+"\"+$dt+"_"+$LogFileName+"_LOG.log"
[int]$global:errorcount=0
[int]$global:warningcount=0

function global:Write-Log
{param($message,[string]$type="info",[string]$logfile=$global:logfilename,[switch]$silent)
    $dt=Get-Date -Format "dd.MM.yyyy HH:mm:ss"
    $msg=$dt + "`t" + $type + "`t" + $message
    Out-File -FilePath $logfile -InputObject $msg -Append -encoding unicode
    if (-not $silent.IsPresent)
    {
        switch ( $type.toLower() )
        {
            "error"
            {
                $global:errorcount++
                write-host $msg -ForegroundColor red
            }
            "warning"
            {
                $global:warningcount++
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