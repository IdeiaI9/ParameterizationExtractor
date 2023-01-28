module Quipu.ParameterizationExtractor.DSL.InternalCommandParser

open FParsec
open AST
open ParserResult

let pBetweenBrackets = between (pstring "(" .>> spaces) (spaces >>. pstring ")") (manySatisfy (fun x -> x <> ')'))
let pSimpleCommand s c = spaces >>.pstringCI s >>% c
let pOneArgumentCommand s c = spaces >>. pstringCI s >>. pBetweenBrackets |>> c
let pTwoArgumentsCommand s c = spaces >>. pstringCI s >>. pBetweenBrackets >>. pipe2 (manySatisfy (fun x -> x <> ',')) (manySatisfy (fun x -> x <> ')')) c

let pHelp:Parser<Command, unit> = pSimpleCommand "Help" Help
let pCheckTable:Parser<Command, unit> = pOneArgumentCommand "CheckTable" (fun x -> CheckTable(x))
let pGetMetadata:Parser<Command, unit> = pOneArgumentCommand "GetMetadata" (fun x -> GetMetadata(x))
let pExit:Parser<Command, unit> = pSimpleCommand "Exit" Exit
let pChangeSource:Parser<Command, unit> = pOneArgumentCommand "ChangeSource" (fun x -> ChangeSource(x))
let pToBC:Parser<Command, unit> = pTwoArgumentsCommand "ToBC" (fun x y -> ToBC(x,y))

let pCommand = choice [attempt pHelp; attempt pCheckTable; attempt pGetMetadata; attempt pExit; attempt pChangeSource; attempt pToBC]

let parse str = 
    match run (pstring "#" >>. pCommand) str with
    | Success(result, _, _)   -> CommandOK(result) :> ParseResult
    | Failure(errorMsg, _, _) -> Fail(errorMsg) :> ParseResult