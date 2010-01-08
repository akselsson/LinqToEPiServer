properties {
    $EPiVersion="5.2.375.236"
    $DatabaseServer='.'
    $DatabaseName='LinqToEPiServer_IntegrationTests'
    $uiPath="/ui"
    $Product = "CMS"
	$LicenseFile = "License.config"
}

task default -depends Remove-Database, devenv, Build, Test
task devenv -depends Install-Database, Copy-EPiBinaries, Build-Config, Copy-License

function Ensure-EPiTransaction([scriptblock] $block){
	$inTransaction = Get-EPiIsBulkInstalling
	
	trap [Exception]{
		write-error $_.Exception
		if($inTransaction -eq $false){
			write-error "Rolling back transaction"
			Rollback-EPiBulkInstall
		}
	}
	
	if($inTransaction -eq $false)
	{		
		Begin-EPiBulkInstall
	}
	
	invoke-command -scriptblock $block
	
	if($inTransaction -eq $false)
	{
		Commit-EPiBulkInstall
	}
}

function Get-EPiProductInfo(){
	$epiProductInfo = Get-EPiProductInformation -ProductName $Product -ProductVersion $EpiVersion
	if (!$epiProductInfo.IsInstalled)
	{
		throw(New-Object ApplicationException($resources.GetString("ErrorInstallationDirectoryNotFound")))
	}
	return $epiProductInfo
}

function Get-EPiInstallationAbsolutePath($relativePath){
	$epiProductInfo = Get-EPiProductInfo
	return [System.IO.Path]::Combine($epiProductInfo.InstallationPath, $relativePath)
}

function Generate-Config($source,$destination){
	$sourceContent = [io.file]::ReadAllText($source) -replace "(`")", '`$1' ; 
	$expanded_config = $ExecutionContext.InvokeCommand.ExpandString($sourceContent)
	set-content -path $destination -value $expanded_config
}

task Load-EPiSnapins{
	$snapIn = Get-PSSnapin -Name EPiServer.Install.Common.1 -ErrorAction SilentlyContinue
	if ($snapIn -eq $null)
	{
		Add-PSSnapin EPiServer.Install.Common.1
	}

	$snapIn = Get-PSSnapin -Name EPiServer.Install.CMS.$EpiVersion -ErrorAction SilentlyContinue
	if ($snapIn -eq $null)
	{
		Add-PSSnapin EPiServer.Install.CMS.$EpiVersion
	}	
}

task Remove-Database -depends Load-EPiSnapins{
	Ensure-EPiTransaction {
		Remove-EPiSqlSvrDb -SqlServerName $DatabaseServer -DatabaseName $DatabaseName -IgnoreMissingDatabase
	}
}

task Install-Database -depends Load-EPiSnapins{
	$epiProductInfo = Get-EPiProductInfo
	$dbScriptFile = Get-EPiInstallationAbsolutePath "Database\MSSQL\EPiServerRelease*.sql" | dir
	$dbScriptFilePath = $dbScriptFile.FullName

	Ensure-EPiTransaction {
		New-EPiSqlSvrDB `
		-SqlServerName $DatabaseServer `
		-DatabaseName $DatabaseName `
		-EPiScriptPath $dbScriptFilePath `
		-EPiServerScript `
		-InstallAspNetSchema `
		-InstallWFSchema `
	
		if (![System.String]::IsNullOrEmpty($uiPath)) 
		{
			Set-EPiBuiltInPageTypePaths `
				-SqlServerName $DatabaseServer `
				-DatabaseName $DatabaseName `
				-UiPath "~$uiPath" `
				-AvoidDbTransaction
		}
	
	}
}

task Copy-EPiBinaries -depends Load-EPiSnapins{
	Ensure-EPiTransaction{
		$epiProductInfo = Get-EPiProductInfo
		$sourceDir = Get-EPiInstallationAbsolutePath "bin"
		$targetDir = "lib/EPiServer"
		Remove-EPiDirectory -DirectoryPath $targetDir
		Copy-EPiFiles -SourceDirectoryPath $sourceDir -DestinationDirectoryPath $targetDir
	}
}

task Copy-License{
	$licenseTarget = "src\linqtoepiserver.tests\License.config"
	$licenseExists = Test-Path $LicenseFile
	if(!$licenseExists){
		"" >> $licenseTarget
	}
	else {
		copy $LicenseFile $licenseTarget
	}
}

task Build-Config{
	$rootPath = Get-Item config
	Get-Item .\config\**\*.config | `
		% { Generate-Config $_.FullName $_.FullName.Replace($rootPath.FullName,"src") }
	
}

task Build -depends Copy-License, Build-Config{
		msbuild src\linqtoepiserver.sln -property:Outdir=..\..\bin\
}

task Impersonate-MSTest{
	copy lib\nunit\nunit-console.exe lib\nunit\mstest.exe
	copy lib\nunit\nunit-console.exe.config lib\nunit\mstest.exe.config
}

task Start-MSDTC {
	net start "MSDTC"
}

task Test -depends Build, Impersonate-MSTest, Start-MSDTC {
	lib\nunit\mstest.exe bin\linqtoepiserver.tests.dll
}

