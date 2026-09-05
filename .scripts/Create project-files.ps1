# Run this from the root folder to create a complete file list of the project
Get-ChildItem -Recurse -File |
    Where-Object {
        $_.FullName -notmatch '\\(bin|obj|\.git|\.idea|\.vs)\\'
    } |
    ForEach-Object {
        $_.FullName.Substring((Get-Location).Path.Length + 1)
    } |
    Sort-Object |
    Out-File -Encoding utf8 project-files.txt