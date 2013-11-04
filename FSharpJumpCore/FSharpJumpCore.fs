namespace FSharpJumpCore
open Microsoft.VisualStudio.Text
open System
open Microsoft.VisualStudio.Text.Editor
open System.Text
open System.Text.RegularExpressions

type Kw = Ns | Mod | Type | Let | And | Member | StaticMember | Marker

type JumpItem(name:string,keyword:Kw,line:ITextSnapshotLine,level:int) =
    member x.Name = name
    member x.Keyword = keyword
    member x.Line = line
    member x.Level = level
    member x.Text = line.GetText()

module Analyzer =

    let private firstCharIndex (inp:string) = inp.IndexOfAny([|'l';'s';'o';'t';'m';'n';'a';'[';'/'|])
    let private rgStaticMember = new Regex("^\s*(\[\<[^\>]\>\]\s*)?static\s+member\s+((private|public|internal)\s+)?(\w+)")
    let private rgMember = new Regex("^\s*(\[\<[^\>]\>\]\s*)?(override|member)\s+[^\.]+\.\s*(\w+|(\([^\)]*\)))")
    let private rgAllExceptMember = new Regex("^\s*(\[\<[^\>]\>\]\s*)?(//#m|let!|let\s+mutable|let|type|module|namespace|and(^\s+set]))\s+((private|public|internal)\s+)?(\w+|(\([^\)]*\))|(``.*?``))")

    let (|AllExceptMember|_|) inp = 
        let m = rgAllExceptMember.Match(inp)
        match m.Success with
        | true ->
            let len = firstCharIndex inp
            let kw = m.Groups.[2].Value
            let nameToken = m.Groups.[6].Value
            let kwType = match kw with "//#m"->Kw.Marker|"type"->Kw.Type|"module"->Kw.Mod|"namespace"->Kw.Ns|"and"->Kw.And|x when x.StartsWith("let") -> Kw.Let|_->failwith "keyword"
            (kwType,nameToken,len) |> Some
        | false -> None

    let (|Member|_|) inp =
        let m = rgMember.Match(inp)
        match m.Success with
        | true ->
            let len = firstCharIndex inp
            let nameToken = m.Groups.[3].Value
            (Kw.Member,nameToken,len) |> Some
        | false -> None

    let (|StaticMember|_|) inp =
        let m = rgStaticMember.Match(inp)
        match m.Success with
        | true ->
            let len = firstCharIndex inp
            let nameToken = m.Groups.[4].Value
            (Kw.StaticMember,nameToken,len) |> Some
        | false -> None

    let (|MatchKeyword|_|) inp =
        match inp with
        | AllExceptMember lineInfo -> lineInfo |> Some
        | Member lineInfo -> lineInfo |> Some
        | StaticMember lineInfo -> lineInfo |> Some
        | _ -> None

    let Analyse (textView:IWpfTextView) =
        let t =  textView.TextSnapshot.Lines
        t
        |> Seq.choose (fun l -> match l.GetText() with  MatchKeyword (kw,n,len) -> (kw,n,len,l) |> Some | _ -> None)
        |> Seq.scan
            (fun (prevLevels,j) (kw,name,currLen,line) ->
                match j with 
                | Some (m:JumpItem) ->
                    let prevLen = match prevLevels with h::t -> snd h | _ -> failwith "this should not happen"
                    let level = 
                        match prevLen with
                        | x when x < currLen -> m.Level + 1
                        | x when x = currLen -> m.Level
                        | y -> prevLevels |> List.find (fun l -> snd l <= currLen) |> fst
                    ((level,currLen)::prevLevels,Some(new JumpItem(name,kw,line,level)))
                | None -> ([(0,currLen)],Some(new JumpItem(name,kw,line,0))))
            ([],None)
        |> Seq.choose (fun (_,j) -> j)