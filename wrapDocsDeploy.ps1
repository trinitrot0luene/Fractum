if ($Env:APPVEYOR_REPO_TAG -eq "true") {
    git config --global credential.helper store
    Add-Content "$env:USERPROFILE\.git-credentials" "https://$($env:github_access_token):x-oauth-basic@github.com`n"
    git config --global user.email %github_email%
    git config --global user.name "trinitrot0luene"
    git config --global core.safecrlf false

    echo "Setup configuration for deploying to gh-pages"

    bash releaseDocs.sh

    exit 0
}