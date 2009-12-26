param(
    $EPiVersion="5.2.375.236",
    $DatabaseServerName='.\sqlexpress',
    $DatabaseName='LinqToEPiServer_IntegrationTests',
    $uiPath="/ui",
    $Product = "CMS"
)

task default -depends Remove-Database, Install-Database, Copy-EPiBinaries, Build, Test

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
		Remove-EPiSqlSvrDb -SqlServerName $DatabaseServerName -DatabaseName $DatabaseName -IgnoreMissingDatabase
	}
}

task Install-Database -depends Load-EPiSnapins{
	$epiProductInfo = Get-EPiProductInfo
	$dbScriptFile = Get-EPiInstallationAbsolutePath "Database\MSSQL\EPiServerRelease*.sql" | dir
	$dbScriptFilePath = $dbScriptFile.FullName

	Ensure-EPiTransaction {
		New-EPiSqlSvrDB `
		-SqlServerName $DatabaseServerName `
		-DatabaseName $DatabaseName `
		-EPiScriptPath $dbScriptFilePath `
		-EPiServerScript `
		-InstallAspNetSchema `
		-InstallWFSchema `
	
		if (![System.String]::IsNullOrEmpty($uiPath)) 
		{
			Set-EPiBuiltInPageTypePaths `
				-SqlServerName $DatabaseServerName `
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

task Fake-License{
	$licenseExists = Test-Path src\LinqToEPiServer.Tests\License.config
	if(!$licenseExists){
		"" >> src\linqtoepiserver.tests\License.config
	}
}

task Update-Config{
	$rootPath = Get-Item config | % {$_.FullName}
	Get-Item .\config\**\*.config | `
		% { @{"Source"=$_.FullName; "Dest"= $_.FullName.Replace($rootPath,"src")}} | `
		% {copy $_["Source"] $_["Dest"]}
	
}

task Build -depends Fake-License{
		msbuild src\linqtoepiserver.sln -property:Outdir=..\..\bin\
}

task Fake-MSTest{
	copy lib\nunit\nunit-console.exe lib\nunit\mstest.exe
	copy lib\nunit\nunit-console.exe.config lib\nunit\mstest.exe.config
}

task Start-MSDTC {
	net start "MSDTC"
}

task Test -depends Build, Fake-MSTest, Start-MSDTC {
	lib\nunit\mstest.exe bin\linqtoepiserver.tests.dll
}

