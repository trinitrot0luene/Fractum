#!/bin/sh
set -e

export VSINSTALLDIR="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community"
export VisualStudioVersion="15.0"

docfx ./docfx_project/docfx.json

SOURCE_DIR=$PWD
TEMP_REPO_DIR=$PWD/../my-project-gh-pages

echo "Ensure temp docs directory is empty $TEMP_REPO_DIR"

rm -rf $TEMP_REPO_DIR
mkdir $TEMP_REPO_DIR

echo "Clone gh-pages branch"

git clone https://github.com/trinitrot0luene/Fractum.git -q --branch gh-pages $TEMP_REPO_DIR

echo "Create or clear version folder $APPVEYOR_REPO_TAG_NAME in gh-pages branch"

mkdir -p $TEMP_REPO_DIR/$APPVEYOR_REPO_TAG_NAME
cd $TEMP_REPO_DIR/$APPVEYOR_REPO_TAG_NAME

git rm ../*

echo "Copy generated docs for version $APPVEYOR_REPO_TAG_NAME to gh-pages"

cp -r $SOURCE_DIR/docfx_project/_site/* .

echo "Push updated documentation to github"

git add . -A
git commit -m "Updated docs for $APPVEYOR_REPO_TAG_NAME"
git push origin gh-pages
