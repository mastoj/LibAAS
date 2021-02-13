#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"
#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.initEnvironment ()

Target.create "Clean" (fun _ ->
    !! "./**/bin"
    ++ "./**/obj"
    |> Shell.cleanDirs 
)

let build target =
  !! (target + "/*.sln")
  |> Seq.iter (DotNet.build id)

let test target =
  !! (target + "/*.sln")
  |> Seq.iter (DotNet.test id)

let restore target =
  !! (target + "/*.sln")
  |> Seq.iter (DotNet.restore id)

let buildAndTest (context:TargetParameter) target =
  // Trace.log "--- Context ---"
  // Trace.log (context.ToString())
  // Trace.log "---------------"
  let cmd = 
    if not context.Context.Arguments.IsEmpty then
      context.Context.Arguments.Head.ToLower()
    else ""
  match cmd with   
  | "test" -> test target
  | "restore" -> restore target
  | _ -> build target
    

Target.create "Ex1Start" (fun x -> buildAndTest x "ex1/start")
Target.create "Ex1Done" (fun x -> buildAndTest x "ex1/done")
Target.create "Ex2Start" (fun x -> buildAndTest x "ex2/start")
Target.create "Ex2Done" (fun x -> buildAndTest x "ex2/done")
Target.create "Ex3Start" (fun x -> buildAndTest x "ex3/start")
Target.create "Ex3Done" (fun x -> buildAndTest x "ex3/done")
Target.create "Ex4Start" (fun x -> buildAndTest x "ex4/start")
Target.create "Ex4Done" (fun x -> buildAndTest x "ex4/done")

Target.create "All" ignore

"Clean"
  ==> "Ex1Start" 
"Clean"
  ==> "Ex1Done" 
"Clean"
  ==> "Ex2Start"
"Clean"
  ==> "Ex2Done" 
"Clean"
  ==> "Ex3Start"
"Clean"
  ==> "Ex3Done" 
"Clean"
  ==> "Ex4Start"
"Clean"
  ==> "Ex4Done" 
"Clean"
  ==> "Ex4Done"==> "Ex4Start" 
  <=> "Ex3Done" <=> "Ex3Start"
  <=> "Ex2Done" <=> "Ex2Start"
  <=> "Ex1Done" <=> "Ex1Start"
  ==> "All"

Target.runOrDefaultWithArguments "All"
