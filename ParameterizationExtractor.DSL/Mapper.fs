module Quipu.ParameterizationExtractor.DSL.Mapper

open Quipu.ParameterizationExtractor.Logic.Configs
open System.Collections.Generic
open AST
open Quipu.ParameterizationExtractor.Logic.Model

let MapRecordToExtract(rootRecord: RootRecord) = 
    new RecordsToExtract(tableName = rootRecord.tableName, where = rootRecord.where, ProcessingOrder = rootRecord.processingOrder) 

let MapRootRecords (rootRecords:list<RootRecord>): List<RecordsToExtract> =
    rootRecords |> List.map(fun x -> MapRecordToExtract(x)) |> List<RecordsToExtract>

let MapExtractStrategy strg = 
    match strg with
        | OnlyOneTableExtract -> new OnlyOneTableExtractStrategy() :> ExtractStrategy
        | FKDependencyExtract -> new FKDependencyExtractStrategy() :> ExtractStrategy
        | ParentsExtract -> new OnlyParentExtractStrategy() :> ExtractStrategy
        | ChildrenExtract -> new OnlyChildrenExtractStrategy() :> ExtractStrategy

let MapSqlBuildStrategy (strg:SqlBuild) = 
     let s = new SqlBuildStrategy() 
     s.AsIsInserts <- strg |> List.exists(fun x -> x = AsIsInserts)
     s.NoInserts <- strg |> List.exists(fun x -> x = NoInserts)
     s.ThrowExecptionIfNotExists <- strg |> List.exists(fun x -> x = ThrowExecptionIfNotExists)
     s
//    new SqlBuildStrategy(strg |> List.exists(fun x -> x = ThrowExecptionIfNotExists),
//                         strg |> List.exists(fun x -> x = NoInserts), 
//                         strg |> List.exists(fun x -> x = AsIsInserts))

let PrepareUniqueColumns (uniqueColumns: string[]) =
    uniqueColumns |> Array.filter(fun x -> x <> "")  |> List<string>

let MapTableToProcess (tableToExtract:AST.TableToExtract) =
    let t = new TableToExtract()
    t.TableName <- tableToExtract.tableName
    t.ExtractStrategy <-tableToExtract.extractStrategy |> MapExtractStrategy |> fun x -> x.DependencyToExclude <- tableToExtract.exclude |> List<string>
                                                                                         x
    t.UniqueColumns <- tableToExtract.uniqueColumns |> PrepareUniqueColumns
    t.SqlBuildStrategy <- tableToExtract.sqlBuild |> MapSqlBuildStrategy
    t

let MapTablesToProcess(tablesToExtract: list<AST.TableToExtract>): List<TableToExtract> =
    tablesToExtract |> List.map(fun x -> MapTableToProcess x) |> List<TableToExtract>
    
let MapToSourceForScript (script:Script):SourceForScript =
    new SourceForScript(ScriptName = script.header.scriptName, RootRecords = MapRootRecords( script.header.rootRecords), TablesToProcess = MapTablesToProcess(script.body))

let MapListScripts (scripts:list<Script>) =
    scripts |> List.map(fun x -> MapToSourceForScript(x)) |> List<SourceForScript>

let MapResult (scripts:list<Script>) : Package =
    new Package(Scripts = MapListScripts(scripts))
  