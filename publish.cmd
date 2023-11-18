dotnet publish FlatCopy --self-contained --configuration Release --output FlatCopy-app
dotnet publish FlatCopy --self-contained --configuration Release --output FlatCopy-single /p:PublishSingleFile=true

PAUSE