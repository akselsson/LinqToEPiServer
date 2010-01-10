properties {
    $EPiVersion="5.2.375.236"
    $DatabaseServer='.'
    $DatabaseName='LinqToEPiServer_IntegrationTests'
    $uiPath="/ui"
    $Product = "CMS"
	$LicenseFile = "License.config"
}

task default -depends clean, devenv, build, Test
task clean -depends Clean-Database, Clean-Solution, Clean-EPiBinaries
task build -depends  Build-Config, Copy-License, Build-Solution
task devenv -depends Install-Database, Copy-EPiBinaries, Build-Config, Copy-License

function Ensure-EPiTransaction([scriptblock] $block){
	$inTransaction = Get-EPiIsBulkInstalling
    if($inTransaction){
        invoke-command -scriptblock $block
        return
    }
    
    try{
    	Begin-EPiBulkInstall
        invoke-command -scriptblock $block
		Commit-EPiBulkInstall
    }
    catch{
		Rollback-EPiBulkInstall
        throw
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
	Remove-PSSnapin EPiServer.Install.Common.1 -ErrorAction SilentlyContinue
	Add-PSSnapin EPiServer.Install.Common.1

	Remove-PSSnapin -Name EPiServer.Install.CMS.$EpiVersion -ErrorAction SilentlyContinue
	Add-PSSnapin EPiServer.Install.CMS.$EpiVersion
}

task Clean-Database -depends Load-EPiSnapins{
	Ensure-EPiTransaction {
		Remove-EPiSqlSvrDb -SqlServerName $DatabaseServer -DatabaseName $DatabaseName -IgnoreMissingDatabase
	}
}

task Install-Database -depends Load-EPiSnapins{
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

task Copy-EPiBinaries -depends Clean-EPiBinaries, Load-EPiSnapins{
	Ensure-EPiTransaction{
		$sourceDir = Get-EPiInstallationAbsolutePath "bin"
		$targetDir = "lib/EPiServer"
		Copy-EPiFiles -SourceDirectoryPath $sourceDir -DestinationDirectoryPath $targetDir
	}
}

task Clean-EPiBinaries {
    rm -recurse lib\EPiServer -ErrorAction SilentlyContinue 
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

task Build-Solution{
	msbuild src\linqtoepiserver.sln -property:Outdir=..\..\bin\
}

task Clean-Solution {
    msbuild src\linqtoepiserver.sln -t:Clean
}

task Impersonate-MSTest{
	copy lib\nunit\nunit-console.exe lib\nunit\mstest.exe
	copy lib\nunit\nunit-console.exe.config lib\nunit\mstest.exe.config
}

task Start-MSDTC {
	net start "MSDTC"
}

task Test -depends build, Impersonate-MSTest, Start-MSDTC {
	lib\nunit\mstest.exe bin\linqtoepiserver.tests.dll
}

