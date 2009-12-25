param(
    $EPiVersion="5.2.375.236",
    $DatabaseServerName='.\sqlexpress',
    $DatabaseName='LinqToSqlServer_IntegrationTests',
    $uiPath="/ui",
    $Product = "CMS"
)

function Load-EPiSnapins(){
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

function Ensure-EPiTransaction([scriptblock] $block){
	trap [Exception]{
		write-error "Install failed"
		write-error "$_.Exception"
		Rollback-EPiBulkInstall
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

function Remove-Database(){
	Ensure-EPiTransaction {
		Remove-EPiSqlSvrDb -SqlServerName $DatabaseServerName -DatabaseName $DatabaseName -IgnoreMissingDatabase
	}
}

function Install-Database(){
	$epiProductInfo = Get-EPiProductInfo
	$dbScriptFile = [System.IO.Path]::Combine($epiProductInfo.InstallationPath, "Database\MSSQL\EPiServerRelease*.sql") | dir
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
	$epiProductInfo = Get-EPiProductInfo
	$sourceDir = [System.IO.Path]::Combine($epiProductInfo.InstallationPath,"bin")
	$targetDir = "lib/EPiServer"
	Remove-EPiDirectory -DirectoryPath $targetDir
	Copy-EPiFiles -SourceDirectoryPath $sourceDir -DestinationDirectoryPath $targetDir
}

Load-EPiSnapins
Ensure-EPiTransaction{
	Remove-Database
	Install-Database
	Copy-EPiBinaries
}



	
	
