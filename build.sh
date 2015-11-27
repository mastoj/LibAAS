#!/usr/bin/env bash

function run() {
  mono "$@"
}

run .paket/paket.bootstrapper.exe
run .paket/paket.exe restore

run packages/FAKE/tools/FAKE.exe "$@" $FSIARGS build.fsx