#r @"packages/FAKE/tools/FakeLib.dll"
open System
open Fake
open Fake.Testing.XUnit2

let testDir = ".test"

let targetName = getBuildParam "target"
trace (sprintf "Target name: %s" targetName)
let targets = ["Ex1Start";"Ex1Done";"Ex2Start";"Ex2Done";"Ex3Start";"Ex3Done";"Ex4Start";"Ex4Start"] |> List.map (fun s -> s.ToLower())

if targets |> List.contains (targetName.ToLower()) |> not then
    let targetNames = String.Join("|", targets)
    let msg = sprintf "Missing target, use: ./build.sh <%s>" targetNames
    targets |> String.concat "|" |> sprintf "Missing target, use: ./build.sh <%s>" |> traceError
    exit -1
let (proj,version) = (targetName.Substring(0,3), targetName.Substring(3))
let basePath = sprintf "./%s/%s" proj version

// Add targets so they are found by the Ionide FAKE plugin
Target "Ex1Start" ignore
Target "Ex1Done" ignore
Target "Ex2Start" ignore
Target "Ex2Done" ignore
Target "Ex3Start" ignore
Target "Ex3Done" ignore
Target "Ex4Start" ignore
Target "Ex4Done" ignore

Target "Default" (fun _ ->
  trace "Hello default"
)

Target "RestorePackages" (fun _ ->
  let packagesFolder = basePath </> "packages"
  !!(basePath </> "**/packages.config")
  |> Seq.iter
      (RestorePackage (fun parameters ->
                        { parameters with
                            OutputPath = packagesFolder}))
)

Target "Build" (fun _ ->
  trace (sprintf "Building %s %s" proj version)
  let sln = !! (basePath </> "*.sln")
  trace (sprintf "Will build solution: %A" sln)
  sln
  |> MSBuildDebug "" "Rebuild"
  |> ignore
)

Target "Test" (fun _ ->
  let testDlls = !!(basePath </> "*.Tests/bin/Debug/*.Tests.dll")
  testDlls
  |> xUnit2 (fun p -> p)
)

"RestorePackages"
  ==> "Build"
  ==> "Test"
  ==> "Default"

Run "Default"
