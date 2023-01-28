module Quipu.ParameterizationExtractor.DSL.AST

    type TableName = string
    type Where = string
    type UniqueColumns = string[]
    type ScriptName = string
    type ProcessingOrder = int
    type RootRecord = {tableName : TableName;  where: Where; processingOrder: ProcessingOrder }

    type ExtractStrategy = 
        | OnlyOneTableExtract 
        | FKDependencyExtract
        | ParentsExtract
        | ChildrenExtract

    type SqlBuildOption = 
        | ThrowExecptionIfNotExists
        | NoInserts
        | AsIsInserts

    type SqlBuild = list<SqlBuildOption>
    type Exclude = string[]

    type TableToExtract = { extractStrategy: ExtractStrategy; tableName: TableName; uniqueColumns: UniqueColumns;  sqlBuild: SqlBuild; exclude: Exclude }
    
    type ScriptHeader = {scriptName: ScriptName; rootRecords: list<RootRecord> }
    type ScriptBody = list<TableToExtract> 

    type Script = {header: ScriptHeader; body: ScriptBody}

    type ConnectionString = string
    type Command =
        | Help
        | CheckTable of TableName
        | GetMetadata of TableName
        | Exit
        | ChangeSource of ConnectionString
        | ToBC of string * string