".paket/paket.bootstrapper.exe"
".paket/paket.exe" "restore"

"./packages/FAKE/tools/FAKE.exe" %* "--fsiargs" "build.fsx" 
