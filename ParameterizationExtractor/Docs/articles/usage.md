---
uid: usage
title: Usage
---
Instructions what to extract must be placed in package file and set as app execution argument ( see @cli )
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

> [!TIP]
> `TableName` can be defined withe Regular Expression ```TableName="RegExp:^Cls_(.*)"```


#### TablesToProcess
`xml` contains list of `TableToExtract` the definition of how each table must be extracted( @extractor ) and processed by @sqlBuilder

> [!NOTE]
> Only tables presented in the TablesToProcess list will be considered for extraction, the rest will be ignored!

#### TableToExtract
`xml` represents definition for table `TableName` processing. `UniqueColumns` can be specified, if not the global definition will be used

> [!TIP]
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