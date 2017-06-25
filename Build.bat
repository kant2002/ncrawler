SET VersionSuffix=%1
SET Configuration=Release
mkdir out
del /Q out\*.*
dotnet msbuild "/t:Restore" /p:Version=4.0.0-%VersionSuffix% /p:VersionSuffix=%VersionSuffix% /p:Configuration=%Configuration%
dotnet msbuild "/t:Pack" /p:Version=4.0.0-%VersionSuffix% /p:VersionSuffix=%VersionSuffix% /p:Configuration=%Configuration%

copy /Y src\NCrawler\bin\%Configuration%\NCrawler.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.HtmlProcessor\bin\%Configuration%\NCrawler.HtmlProcessor.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.EsentServices\bin\%Configuration%\NCrawler.EsentServices.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.EntityFramework\bin\%Configuration%\NCrawler.EntityFramework.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.iTextSharpPdfProcessor\bin\%Configuration%\NCrawler.iTextSharpPdfProcessor.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.MP3Processor\bin\%Configuration%\NCrawler.MP3Processor.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.SitemapProcessor\bin\%Configuration%\NCrawler.SitemapProcessor.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.RedisServices\bin\%Configuration%\NCrawler.RedisServices.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.FileStorageServices\bin\%Configuration%\NCrawler.FileStorageServices.4.0.0-%VersionSuffix%.nupkg out
copy /Y src\NCrawler.IFilterProcessor\bin\%Configuration%\NCrawler.IFilterProcessor.4.0.0-%VersionSuffix%.nupkg out
