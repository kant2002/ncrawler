param(
	[Parameter(Mandatory = $False)]
	[string]$Configuration = "Release",
	[Parameter(Mandatory = $False)]
	[string]$Version="4.0.0",
	[Parameter(Mandatory = $False)]
	[AllowEmptyString()]
	[string]$VersionSuffix = $Null
)

$OutDirectory="out/$Version"
Remove-Item $OutDirectory -Recurse -Force
New-Item -ItemType directory -Path $OutDirectory

if ($VersionSuffix -eq "")
{
	$FullVersion="$Version"
	dotnet msbuild "/t:Restore" /p:Version=$Version /p:Configuration=$Configuration
	dotnet msbuild "/t:Pack" /p:Version=$Version /p:Configuration=$Configuration
}
else
{
	$FullVersion="$Version-$VersionSuffix"
	dotnet msbuild "/t:Restore" /p:Version=$Version /p:VersionSuffix=$VersionSuffix /p:Configuration=$Configuration
	dotnet msbuild "/t:Pack" /p:Version=$Version /p:VersionSuffix=$VersionSuffix /p:Configuration=$Configuration
}

Copy-Item src\NCrawler\bin\$Configuration\NCrawler.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.HtmlProcessor\bin\$Configuration\NCrawler.HtmlProcessor.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.EsentServices\bin\$Configuration\NCrawler.EsentServices.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.EntityFramework\bin\$Configuration\NCrawler.EntityFramework.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.iTextSharpPdfProcessor\bin\$Configuration\NCrawler.iTextSharpPdfProcessor.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.MP3Processor\bin\$Configuration\NCrawler.MP3Processor.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.SitemapProcessor\bin\$Configuration\NCrawler.SitemapProcessor.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.RedisServices\bin\$Configuration\NCrawler.RedisServices.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.FileStorageServices\bin\$Configuration\NCrawler.FileStorageServices.$FullVersion.nupkg $OutDirectory
Copy-Item src\NCrawler.IFilterProcessor\bin\$Configuration\NCrawler.IFilterProcessor.$FullVersion.nupkg $OutDirectory
