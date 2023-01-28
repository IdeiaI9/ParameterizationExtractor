---
uid: example
title: Example
---
### Task
To create package to extract parameterization for XAML Presentations 

![Fig1](../images/er_example.png)

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

Additionally please check @usage

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
Additionally please check @usage

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
> [!TIP]
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

Additionally please check @extractor

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

Additionally please check @sqlBuilder

### Samples Repository
More samples can be found in [TFS](https://dev.azure.com/quipu-cwnet/SQL%20Bulldozer/_versionControl?path=%24%2FSQL%20Bulldozer%2FTemplatesRepo)
