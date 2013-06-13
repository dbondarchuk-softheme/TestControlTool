 param ($destination, $user, $password, $command)
	 $securePass = ConvertTo-SecureString -String $password -AsPlainText -Force
	 $credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $user, $securePass

	 $session = New-PSSession $destination -Credential $credentials
	 $result = Invoke-Command -Session $session -ScriptBlock {Invoke-Expression $($args[0])} -ArgumentList $command
	
	 $result.Value
	 Remove-PSSession $session