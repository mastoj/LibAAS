#r @"packages/FAKE/tools/FakeLib.dll"
open Fake

let createBuild proj version = 
  fun _ ->
    trace (sprintf "Building %s %s" proj version)
    let sln = !! (sprintf "./%s/%s/*.sln" proj version)
    trace (sprintf "Will build solution: %A" sln)
    sln
    |> MSBuildRelease "" "Rebuild"
    |> ignore

Target "Default" (fun _ ->
  trace "Hello stuff"
)

Target "Ex1Start" (createBuild "ex1" "start")
Target "Ex1Done" (createBuild "ex1" "done")
Target "Ex2Start" (createBuild "ex2" "start")
Target "Ex2Done" (createBuild "ex2" "done")
Target "Ex3Start" (createBuild "ex3" "start")
Target "Ex3Done" (createBuild "ex3" "done")
Target "Ex4Start" (createBuild "ex4" "start")
Target "Ex4Done" (createBuild "ex4" "done")

RunTargetOrDefault "Default"
