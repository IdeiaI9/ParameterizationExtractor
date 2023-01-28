//namespace Quipu.ParameterizationExtractor.Parser
//
//type Class1() = 
//    member this.X = "F#"

module Quipu.ParameterizationExtractor.DSL.DSLParser

open FParsec
open ParserResult
open AST
open Mapper

let str_ws s = pstring s .>> spaces1
let pstringvalue = 
    between (pstring "\"") (pstring "\"") (manySatisfy (fun x -> x <> '"')) 
 
let ptableName:Parser<TableName, unit> = spaces >>? str_ws "from" >>. pstringvalue
let pWhere:Parser<Where, unit> = spaces1 >>? str_ws "where" >>. pstringvalue 
let pProcessingOrder:Parser<ProcessingOrder,unit> = spaces1 >>? str_ws "order" >>. pstringvalue |>> fun x -> x |> int
let prootRecord:Parser<RootRecord, unit> = pipe3 ptableName (pWhere <|> preturn "") (pProcessingOrder <|> preturn 0)  (fun r w p -> {tableName= r; where= w; processingOrder=p})

let pstrgOneTable:Parser<ExtractStrategy, unit> = spaces >>. (pstring "OnlyOneTableExtract" <|> pstring "OneTable") >>% OnlyOneTableExtract 
let pstrgFKDependency:Parser<ExtractStrategy, unit> = spaces >>. (pstring "FKDependencyExtract" <|> pstring "FK") >>% FKDependencyExtract 
let pstrgParents:Parser<ExtractStrategy, unit> = spaces >>. (pstring "ParentsExtract" <|> pstring "Parents") >>% ParentsExtract 
let pstrgOChildren:Parser<ExtractStrategy, unit> = spaces >>. (pstring "ChildrenExtract" <|> pstring "Children") >>% ChildrenExtract 

let pextractStrategy = pstrgOneTable <|> pstrgFKDependency <|> pstrgParents <|> pstrgOChildren
let pFor:Parser<TableName, unit> = spaces >>. str_ws "for" >>. pstringvalue
let puqColumns:Parser<UniqueColumns, unit> = spaces >>? pstring "and" >>.spaces >>. pstring "UniqueColumns" >>. spaces >>. pstringvalue |>> fun x -> x.Split(',')

let pThrowExecptionIfNotExists:Parser<SqlBuildOption,unit> = spaces >>. (pstring "ThrowExecptionIfNotExists" <|> pstring "throw") >>% ThrowExecptionIfNotExists  
let pNoInserts:Parser<SqlBuildOption,unit> = spaces >>. pstring "NoInserts" >>% NoInserts  
let pAsIsInserts:Parser<SqlBuildOption,unit> = spaces >>. (pstring "AsIsInserts" <|> pstring "asIs") >>% AsIsInserts  

let pSqlBuildOption = pThrowExecptionIfNotExists <|> pNoInserts <|> pAsIsInserts

let pbuildSqlStr = spaces >>. pstring "build sql" >>. spaces

let pSqlBuild:Parser<SqlBuild,unit> = 
    pbuildSqlStr >>. many1 (spaces >>? pstring "with">>. spaces>>. pSqlBuildOption) 

let pExclude:Parser<Exclude,unit> = spaces >>. pstring "exclude" >>. spaces >>.pstringvalue |>> fun x -> x.Split(',')

let poneExtractTable =  pipe5 pextractStrategy pFor (puqColumns <|> preturn (Array.empty<string>)) ((attempt pExclude) <|> preturn Array.empty<string>) ((attempt pSqlBuild) <|> preturn []) (fun s fr c e sl -> {extractStrategy= s;tableName=fr;uniqueColumns=c; exclude=e ;sqlBuild = sl })

let pextractTable:Parser<TableToExtract, unit> = spaces >>? poneExtractTable

let pheader:Parser<ScriptHeader, unit> = pipe2 (spaces >>. pstringvalue) (spaces >>. str_ws "take" >>. (many1 prootRecord .>> spaces)) (fun n r -> {scriptName=n;rootRecords =r})

let pbody:Parser<ScriptBody,unit> = pstring "consider" >>. many1 pextractTable 

let pscript:Parser<Script, unit>= str_ws "for script" >>. pipe2 pheader (pbody <|> preturn []) (fun h b -> {header = h; body = b}) //.>> str_ws "end"

let parse (str:string): ParseResult = 
    match run (many1 (spaces >>? pscript)) str with
    | Success(result, _, _)   -> DslOK(result) :> ParseResult
    | Failure(errorMsg, _, _) -> Fail(errorMsg) :> ParseResult
