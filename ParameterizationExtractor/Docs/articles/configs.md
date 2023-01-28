---
uid: configs
title: Config files
---
# Config files explained 

## appsettings.json
Contains application level settings, below sections explained one by one

#### Serilog 
`json` contains configuration for *seri log*, please refer to [oficial documentation](https://github.com/serilog/serilog-settings-configuration)

#### ConnectionStrings
Contains sql connections strings from where data to be extracted, each connection string has unique key and value
```
"SourceDB": "Server=;Database;Integrated Security=True;MultipleActiveResultSets=true;Pooling=True;Max Pool Size=2500;",
```
by default the value with key SourceDB will be considered as primary one

#### Sample
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.Console" ],
    "MinimumLevel": "Debug",
    "WriteTo": [
      {
        "Name": "File",
        "Args": { "path": "serilog-configuration-sample.txt", "fileSizeLimitBytes": "5000" }
      },
      { "Name": "Console" }
    ],
    "Properties": {
      "Application": "Sample"
    }
  },
  "ConnectionStrings": {
    "SourceDB": "Server=server1;Database=;Integrated Security=True;MultipleActiveResultSets=true;Pooling=True;Max Pool Size=2500;",
    "ECUMSC": "Server=server2;Database=;Integrated Security=True;MultipleActiveResultSets=true;Pooling=True;Max Pool Size=2500;",
    "mscdw": "Server=server3;Database=;Integrated Security=True;MultipleActiveResultSets=true;Pooling=True;Max Pool Size=2500;",
    "dev3Sql12": "Server=server4;Database=;Integrated Security=True;MultipleActiveResultSets=true;Pooling=True;Max Pool Size=2500;"
  }
}

```

## ExtractConfig.xml
Contains global settings for the application logic, below sections explained one by one

#### FieldsToExclude 
represents list of the filed that globally will be ignored during data extraction
```
  <FieldsToExclude>
    <string>CreatorId</string>
    <string>Created</string>
    <string>ChangerId</string>
    <string>Changed</string>
  </FieldsToExclude>
```

#### UniqueColums
`list of UniqueColumnsCollection` represents list of the table with combination of the columns uniquely identify the record
```
  <UniqueColums>
    <UniqueColumnsCollection TableName="BusinessProcessSteps" UniqueColumns="BPTypeCode StepStartStatusId StepEndStatusId" />
    <UniqueColumnsCollection TableName="bpProcessingConditions" UniqueColumns="ConditionText" />
  </UniqueColums>
```
> [!NOTE]
> Please consider that UniqueColumns is string separated by space `Field1 Field2 Field3 `

#### DefaultExtractStrategy
Sets default extract strategy for table one of 
- OnlyChildrenExtractStrategy
- FKDependencyExtractStrategy
- OnlyParentExtractStrategy
- OnlyOneTableExtractStrategy

```
 <DefaultExtractStrategy xsi:type="OnlyChildrenExtractStrategy" ProcessChildren="true" ProcessParents="false">
  </DefaultExtractStrategy>
 
 <DefaultExtractStrategy xsi:type="FKDependencyExtractStrategy" ProcessChildren="true" ProcessParents="true">
  </DefaultExtractStrategy>

 <DefaultExtractStrategy xsi:type="OnlyParentExtractStrategy" ProcessChildren="false" ProcessParents="true">
  </DefaultExtractStrategy>

 <DefaultExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false">
  </DefaultExtractStrategy>
```

#### DefaultSqlBuildStrategy
Sets default sql build strategy

```
  <DefaultSqlBuildStrategy ThrowExecptionIfNotExists="false" NoInserts="false" AsIsInserts="false" />
```

#### ResultingScriptOptions
Sets additional parameters for script generation

#### TargetDatabase
`string` name of the database that supose to be recipient of the generated scripts. ``` USE DBName``` will be added to resulted script. Userfule when app is part of CI process and recipient DB is know uprfront.

#### Rollback
`bool` if true, in resulted script will be placed ```rollback``` instead of commit, useful while testing template