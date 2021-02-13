#!/usr/bin/env bash

if [ $# -lt 1 ] || [ $# -gt 2 ]; then
    echo "Usage:"
    echo "build.sh Ex1Start | Ex1Done | Ex2Start | Ex2Done | Ex3Start | Ex3Done | Ex4Start | Ex4Done [test|restore]"
    exit 1
fi

set -eu
set -o pipefail

dotnet tool restore
dotnet fake build --target "$@"
#dotnet fake "$@"