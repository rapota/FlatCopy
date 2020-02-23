dotnet publish FlatCopy --configuration Release --framework netcoreapp3.1 /p:PublishSingleFile=true --runtime win-x64 --output FlatCopy64
dotnet publish FlatCopy --configuration Release --framework netcoreapp3.1 /p:PublishSingleFile=true --runtime win-x86 --output FlatCopy86

dotnet publish FlatCopy --configuration Release --framework net48 --output FlatCopyNet48

PAUSE