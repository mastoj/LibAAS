#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing.XUnit2

let testDir = ".test"

let createBuildAndTest proj version = 
  let basePath = sprintf "./%s/%s" proj version
  let build() = 
      trace (sprintf "Building %s %s" proj version)
      let sln = !! (basePath </> "*.sln")
      trace (sprintf "Will build solution: %A" sln)
      sln
      |> MSBuildDebug "" "Rebuild"
      |> ignore

  let test() =     
    let testDlls = !!(basePath </> "*.Tests/bin/Debug/*.Tests.dll")
    testDlls 
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
    
  let restorePackages() = 
    basePath </> "**/packages.config"
    |> RestorePackage (fun p ->
        { p with
            Sources = p.Sources
            OutputPath = (basePath </> "packages")
            Retries = 4 })
    |> ignore
 
  fun _ ->
    restorePackages()
    build()
    test()

Target "Default" (fun _ ->
  trace "Hello default"
)

// Target "Ex1Start" (createBuildAndTest "ex1" "start")
// Target "Ex1Done" (createBuildAndTest "ex1" "done")
// Target "Ex2Start" (createBuildAndTest "ex2" "start")
// Target "Ex2Done" (createBuildAndTest "ex2" "done")
// Target "Ex3Start" (createBuildAndTest "ex3" "start")
// Target "Ex3Done" (createBuildAndTest "ex3" "done")
// Target "Ex4Start" (createBuildAndTest "ex4" "start")
// Target "Ex4Done" (createBuildAndTest "ex4" "done")

let targetName = getBuildParam "target"
let (proj,version) = (targetName.Substring(0,3), targetName.Substring(3))
let basePath = sprintf "./%s/%s" proj version

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
  |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)

"RestorePackages"
  ==> "Build"
  ==> "Test"
  ==> "Default"

Run "Default"