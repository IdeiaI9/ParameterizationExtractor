The Buldozer application is a *CLI tool* to extraсt data from database and save it as `insert into` scripts. It was developed in order to simplify work of developer in preparation of the huge amount of parameterization scripts. The ultimate goal is to automatize creation of the SQL scripts in various scenarios

## How Data Extractor works
The data in relation DB can be presented as graph where nodes are records in tables and foreign keys are links. 
![Fig1](/ParameterizationExtractor/Docs/images/fig1.jpg)

> It is important to understand parent\child relations.For example, in given figure, table1 is parent to table 2\3, and table 2 is child of table 1 and parent to table 4.

The **extraction** is a process of visiting the graph nodes, according to start records and given list of tables. Result of walking is list of dependent records in hierarchical structure, saved in memory. In general there are 4 possible **strategy** of walking from one table to another
- Moving to all children and all parents 
- Moving only to children 
- Moving only to parents
- Stop moving

For example 
![Fig2](/ParameterizationExtractor/Docs/images/fig2.jpg)

Let imagine we are on table 2, highlighted with blue, the *walking* process can continue 
1. in both direction to table 1 and table 4 
2. only to parent, to table 1
3. only to child, to table 4
4. the process stop

in case strategy is 1 or 2 and if record has value in FK, process will be moved to table 1
![Fig3](/ParameterizationExtractor/Docs/images/fig3.jpg)

and selection of the next step repeats for table 1, and so on.

Each table, included in the process, must have **extraction strategy** defined, if not the default from config file will be picked up.

### Summary
In order to extract data, the process needs to have 
1. starting *points* - records in DB
2. list of the tables to process, the tables **must be** explicitly listed among with **extraction strategy**

the process takes record from starting ones, and *walking* from table to table according to given tables strategy.

## Extraction Strategy
Represented in xml in the following way 
```
        <ExtractStrategy xsi:type="OnlyChildrenExtractStrategy" ProcessChildren="true" ProcessParents="false" Where=""/>
        <ExtractStrategy xsi:type="FKDependencyExtractStrategy" ProcessChildren="true" ProcessParents="true" Where=""/>
        <ExtractStrategy xsi:type="OnlyParentExtractStrategy" ProcessChildren="false" ProcessParents="true" Where=""/>
        <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false" Where=""/>
```
### Where
`string` part of sql where statment, allows to limit records that will be considered when process selects next step to walk.  
```
    <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false" where=" id not in (1,2,3) ">
```


## SQL Builder
Logical part of the application responsible to convert extract hierarchical data to SQL insert\update scripts

### Algorithm
Starting point records are presented as hierarchical list, each item has list of parent records and children records. The sql builder going item by item, and creates sql text according to specified SQL build strategy. 
> [!WARNING]
> Overall rule - *first process parents records, then children*. If extract strategy is not properyly defined for the tables, sql will be generated wrongly

Example 
![Fig1](/ParameterizationExtractor/Docs/images/fig4.jpg)


Lets imagine starting point - *row 2* from *table 2*, and extraction process was done with `FKDependencyExtractStrategy`. Algorithm will be following

1. starting from parents, checking table 1 
2. record from *table 1* has also parent recods in *table 5*, so moving there
3. checking *table 5*, and generate sql according to Sql Build Strategy
4. moving back to *table 1* and generate sql, considering record from *table 5*
5. moving back to *table 2* and generate sql, considering record from *table 1*
6. moving to table *table 4* and generate sql, considering record from *table 2*


### Unqiue Columns
Each table must have unique column(s) beside PK. The combination of columns **must** uniquely identify the record among others in the table. Unique columns are defined either globaly (see @configs ) or in TableToExtract (see @usage )
The unqiue colums combination is widly used in the sql scripts generation
* to check if such record exists, ``` if not exists (select ... where ...) ```
* to initialize variable that holds primary key of newly inserted record

### SQL Generation 
The sql is generated according ot SqlBuildStrategy that must be defined for each table 
```
 <SqlBuildStrategy ThrowExecptionIfNotExists="true" NoInserts="false" AsIsInserts="false" IdentityInsert="false" FieldsToExclude="" />
```
#### ThrowExecptionIfNotExists
if `true`, the following sql will be generated
```
    declare @TableID  
    if not exists(select * from table where [unique columns] )				
    begin
       insert ... 
       set TableID ... 
    end
    else
  		RAISERROR('Record from table with unique values can not be found', 16, 1) 
```
if `false`

```
    declare @TableID  
    if not exists(select * from table where [unique columns] )				
    begin
       insert ... 
       set TableID ... 
    end
    else
    begin
       update ...

       select TableID ...
       from Table
       where [unique columns]
    end
```

#### NoInserts
if `true`, insert\update will not be generated
```
    declare @TableID  
    if not exists(select * from table where [unique columns] )				
  		RAISERROR('Record from table with unique values can not be found', 16, 1) 
    declare @TableID  
    select TableID ...
    from Table
    where [unique columns]
```

#### AsIsInserts
if `true`, trigger will be disabled before processing the table

#### IdentityInsert
if `true`   SET IDENTITY_INSERT ON\OFF will be used before insert

#### FieldsToExclude
String with of the columns separated by spaces, to be exluded from processing, it may be useful if data strture of the source and destination DB is slightly different. 

#### DeleteExistingRecords
if `true` resulted script will call SP `deleter` that deletes all related records from all tables before insert. SP will be created in the begining of script and dropped at the end

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

Instructions what to extract must be placed in package file and set as app execution argument
The package file is xml structure that contains all needed parameters to extract data and build sql.

## Package structure
`xml` contains defintion how resulted scripts are extracted. 

```
<?xml version="1.0" encoding="utf-16"?>
<Package xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Scripts>
    <SourceForScript Order="" ScriptName="">
      <RootRecords>
        <RecordsToExtract TableName="" Where="" ProcessingOrder=""/>
        <RecordsToExtract TableName="" Where="" ProcessingOrder=""/>
        ...

      </RootRecords>
      
      <TablesToProcess>

        <TableToExtract TableName="" UniqueColumns="">
          <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false" Where=""/>
          <SqlBuildStrategy ThrowExecptionIfNotExists="false" NoInserts="false" AsIsInserts="false" FieldsToExclude=""/>
        </TableToExtract>
        
        <TableToExtract TableName="" UniqueColumns="">
          <ExtractStrategy xsi:type="FKDependencyExtractStrategy" ProcessChildren="true" ProcessParents="true" Where=""/>
          <SqlBuildStrategy ThrowExecptionIfNotExists="true" NoInserts="false" AsIsInserts="false" IdentityInsert="false" FieldsToExclude="" />
        </TableToExtract>
  
        ...

      </TablesToProcess>
    </SourceForScript>
     
    ...
 </Scripts>
</Package>
```

The package can contain many `scripts`, each `scripts` element has execution `Order` and `ScriptName` of resulted script, file name will be formed by the following mask: `{Order}_p_{ScriptName}.sql`
```
010_p_Misc.sql
016_p_Misc2.sql
038_p_Subscribers.sql
110_p_Roles.sql
```

### Scripts
`xml` element consist of two parts 
- RootRecords
- TablesToProcess
#### RootRecords
`xml` contains list of `RecordsToExtract` the references to certain records in database from which extraction will start. 
`RecordsToExtract` will be sorted by `ProcessingOrder` and query like the following will be constructed
```
select [list of columns according to given parameters] 
from [TableName]
where [the where string]
```
if `where` is empty, it will be ignored

> `TableName` can be defined withe Regular Expression ```TableName="RegExp:^Cls_(.*)"```


#### TablesToProcess
`xml` contains list of `TableToExtract` the definition of how each table must be extracted and processed by Sql Builder

> [!NOTE]
> Only tables presented in the TablesToProcess list will be considered for extraction, the rest will be ignored!

#### TableToExtract
`xml` represents definition for table `TableName` processing. `UniqueColumns` can be specified, if not the global definition will be used

> `TableName` can be defined withe Regular Expression ```TableName="RegExp:^Cls_(.*)"```


##### DefaultExtractStrategy
Defines Extract strategy for table one of 
- OnlyChildrenExtractStrategy
- FKDependencyExtractStrategy
- OnlyParentExtractStrategy
- OnlyOneTableExtractStrategy
for more see - @extractor

#### SqlBuildStrategy
Defines SQL Build strategy, see more in @sqlBuilder

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
* **-n** connection string name from the appsettings, default is `SourceDB` 
* **-p** path to package, mandatory
* **-d** database name, if specified, server name must be set (-s)
* **-s** server name, if specified, database name must be set (-d)
* **-o** path to output folder, default is `Output`

By default as source for data extraction will be used connection string with key `SourceDB`. However, sometimes comfortable to create a list of *frequently used* connection strings and keep it in config file, so by **-n** it will be easy to swtich between the DBs. 

**-s** **-d** are used if app is a part of *CI\CD* process and soruce DB is uknown till certain moment. 

> Command Line Arguments have highest priority comparing to appsettings.json

### Task
To create package to extract parameterization for XAML Presentations 

![Fig1](/ParameterizationExtractor/Docs/images/er_example.png)

### Solution

#### Step 0, Scope 
Out task is to extract only parameterization related to XAML presentations, tables `MenuItems`, `BpTypeStepPresentations` and `ProductPresentations` must be out of processing , otherwise the tool will extract half DB. Therefore we have to limit ourselves to the following 
tables
* PresentationGroups
* PesentationValidationRules
* Presentations

#### Step 1, Package

First of all we have to create package file, xml file with extract instructions.
```
<?xml version="1.0" encoding="utf-16"?>
<Package xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Scripts>

 </Scripts>
</Package>
```


#### Step 2, Script 
We have to name script and set order 
```
<?xml version="1.0" encoding="utf-16"?>
<Package xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Scripts>
    <SourceForScript Order="10" ScriptName="Presentations">
    
    </SourceForScript>
 </Scripts>
</Package>
```

The resulted script will be named **10_p_Presentations.sql**

#### Step 3, Root Record
The root record, from which we start extraction, is record from table `Presentations`. 
```
<?xml version="1.0" encoding="utf-16"?>
<Package xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Scripts>
    <SourceForScript Order="10" ScriptName="Presentations">
        <RecordsToExtract TableName="Presentations" Where="presentationid in (1,2,3)" ProcessingOrder="5"/>    
    </SourceForScript>
 </Scripts>
</Package>
```

> It is possible to defined as many root records as needed. 
> 
Additionally please check @usage

#### Step 4, Extract Strategy and Unique Columns
Considering given ER model, the strategy may be the following

* Presentations - starting point, so we have to go via parents and children, `FKDependencyExtractStrategy`. Unique Columns are `Name` and `Code` 
* PresentationGroups - we need only one record, and extraction must not go to parents\children otherwise all presentations with the same group will be extracted, therefore here is `OnlyOneTableExtractStrategy`. Unique Column is `Code` 
* PesentationValidationRules - we need only recrods for specified presentationid, therefore `OnlyOneTableExtractStrategy`. Unique Column are `ViewModelProperty` `ValidationType` `PresentationID` `DescrEng` 

```
<?xml version="1.0" encoding="utf-16"?>
<Package xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Scripts>
    <SourceForScript Order="10" ScriptName="Presentations">
        <RecordsToExtract TableName="Presentations" Where="presentationid in (1,2,3)" ProcessingOrder="5"/>    
    </SourceForScript>
    <TablesToProcess>

        <TableToExtract TableName="PresentationGroups" UniqueColumns="Code">
          <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false"/>
        </TableToExtract>


        <TableToExtract TableName="Presentations" UniqueColumns="Code Name">
          <ExtractStrategy xsi:type="FKDependencyExtractStrategy" ProcessChildren="true" ProcessParents="true"/>
        </TableToExtract>

        <TableToExtract TableName="PresentationValidationRules" UniqueColumns="ViewModelProperty ValidationType PresentationID DescrEng">
          <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false"/>
        </TableToExtract>

    </TablesToProcess>

 </Scripts>
</Package>
```

#### Step 5, SQL Build Strategy 

* Presentations - ThrowExecptionIfNotExists = false, NoInserts = false and AsIsInserts = false we need to have insert or update. IdentityInsert is false also, no FieldsToExclude to exlcude.
* PresentationGroups - ThrowExecptionIfNotExists = false, NoInserts = false and AsIsInserts = false we need to have insert or update. FieldsToExclude="ParentPresentationGroupId", we do not want to extract parent records, so just exlude this field
* PesentationValidationRules - ThrowExecptionIfNotExists = false, NoInserts = false and AsIsInserts = false we need to have insert or update. IdentityInsert is false also, no FieldsToExclude to exlcude.

```
<?xml version="1.0" encoding="utf-16"?>
<Package xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Scripts>
    <SourceForScript Order="10" ScriptName="Presentations">
        <RecordsToExtract TableName="Presentations" Where="presentationid in (1,2,3)" ProcessingOrder="5"/>    
    </SourceForScript>
    <TablesToProcess>

        <TableToExtract TableName="PresentationGroups" UniqueColumns="Code">
          <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false"/>
          <SqlBuildStrategy ThrowExecptionIfNotExists="false" NoInserts="false" AsIsInserts="false" FieldsToExclude="ParentPresentationGroupId"/>   
        </TableToExtract>


        <TableToExtract TableName="Presentations" UniqueColumns="Code Name">
          <ExtractStrategy xsi:type="FKDependencyExtractStrategy" ProcessChildren="true" ProcessParents="true"/>
          <SqlBuildStrategy ThrowExecptionIfNotExists="false" NoInserts="false" AsIsInserts="false" />
        </TableToExtract>

        <TableToExtract TableName="PresentationValidationRules" UniqueColumns="ViewModelProperty ValidationType PresentationID DescrEng">
          <ExtractStrategy xsi:type="OnlyOneTableExtractStrategy" ProcessChildren="false" ProcessParents="false"/>
          <SqlBuildStrategy ThrowExecptionIfNotExists="false" NoInserts="false" AsIsInserts="false" />
        </TableToExtract>

    </TablesToProcess>

 </Scripts>
</Package>
```
