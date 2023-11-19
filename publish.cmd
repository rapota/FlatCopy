dotnet publish FlatCopy --configuration Release --output FlatCopy-app --self-contained
dotnet publish FlatCopy --configuration Release --output FlatCopy-single --self-contained /p:PublishSingleFile=true
dotnet publish FlatCopy --configuration Release --output FlatCopy-trimmed --self-contained /p:PublishTrimmed=true
dotnet publish FlatCopy --configuration Release --output FlatCopy-single-trimmed --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true
dotnet publish FlatCopy --configuration Release --output FlatCopy-aot-x64 --runtime win-x64 /p:PublishAot=true

PAUSE