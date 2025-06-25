#!/bin/bash
set -e

if [ -z "$1" ]; then
    echo "Usage: $0 <tag>"
    exit 1
fi

TAG="$1"

# Create tag and release
git tag "$TAG"
git push origin master "$TAG"
dotnet build -c Release


# Create release on github
gh release create "$TAG" --title "$TAG" --generate-notes
gh release upload "$TAG" OccultCrescentHelper/bin/Release/BOCCHI/latest.zip --clobber
