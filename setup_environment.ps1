param(
    $EPiVersion="5.2.375.236",
    $DatabaseServerName='.\sqlexpress',
    $DatabaseName='LinqToSqlServer_IntegrationTests',
    $uiPath="/ui",
    $Product = "CMS"
)

function Load-EPiSnapins(){
	write-host "Loading EPiServer snapins"
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

function Ensure-EPiTransaction([scriptblock] $block){
	$inTransaction = Get-EPiIsBulkInstalling
	
	trap [Exception]{
		if($inTransaction -eq $false){
			write-error "	Rolling back transaction"
			Rollback-EPiBulkInstall
		}
	}
	
	if($inTransaction -eq $false)
	{
		write-host "	Starting transaction"
		Begin-EPiBulkInstall
	}
	
	invoke-command -scriptblock $block
	
	if($inTransaction -eq $false)
	{
		write-host "	Committing transaction"
		Commit-EPiBulkInstall
	}
	
}

function Remove-Database(){
	write-host "Removing test database"
	Ensure-EPiTransaction {
		Remove-EPiSqlSvrDb -SqlServerName $DatabaseServerName -DatabaseName $DatabaseName -IgnoreMissingDatabase
	}
}

function Install-Database(){
	write-host "Installing test-database"
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

function Copy-EPiBinaries(){
	write-host "Copying EPiServer binaries"
	Ensure-EPiTransaction{
		$epiProductInfo = Get-EPiProductInfo
		$sourceDir = Get-EPiInstallationAbsolutePath "bin"
		$targetDir = "lib/EPiServer"
		Remove-EPiDirectory -DirectoryPath $targetDir
		Copy-EPiFiles -SourceDirectoryPath $sourceDir -DestinationDirectoryPath $targetDir
	}
}

Load-EPiSnapins
Remove-Database
Install-Database
Copy-EPiBinaries




	
	
