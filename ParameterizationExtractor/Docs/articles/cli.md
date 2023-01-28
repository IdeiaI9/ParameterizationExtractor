---
uid: cli
title: Command Line Interface
---
## Command Line Interface
The application supports CLI(command line interface) also it can be used in powershell scripts
```
[CmdletBinding()]
param (
    [Parameter()]
    [string]$sqlServerName,
    [string]$databaseName
)


function Run-Buldozer {
  [cmdletbinding()]
  Param(
      [Parameter()]
      [String[]]$a   
  )
  Write-Host "Starting..."
  Write-Host $a   
  $p = Start-Process -NoNewWindow -PassThru -Wait -FilePath "ParameterizationExtractor.exe" -ArgumentList $a    
  if ($p.ExitCode -lt 0) {    
    Write-Host "Error occurs, powershell pipeline will be terminated"   
    throw New-Object System.Management.Automation.PipelineStoppedException 
  }
    
}


Run-Buldozer "-p 01_AllClss.xml -s $sqlServerName -d $databaseName" | 
Run-Buldozer "-p 02_misc.xml -s $sqlServerName -d $databaseName" 
```

### Supported Arguments
* **-n** connection string name from the appsettings, default is `SourceDB`, see @configs 
* **-p** path to package, mandatory
* **-d** database name, if specified, server name must be set (-s)
* **-s** server name, if specified, database name must be set (-d)
* **-o** path to output folder, default is `Output`

By default as source for data extraction will be used connection string with key `SourceDB`. However, sometimes comfortable to create a list of *frequently used* connection strings and keep it in config file, so by **-n** it will be easy to swtich between the DBs. 

**-s** **-d** are used if app is a part of *CI\CD* process and soruce DB is uknown till certain moment. 

> [!NOTE]
> Command Line Arguments have highest priority comparing to appsettings.json
> 