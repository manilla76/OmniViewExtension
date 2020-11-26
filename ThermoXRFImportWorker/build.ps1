dotnet restore
dotnet publish -o /publish/service
sc.exe create ThermoXrfImportService binpath= ./publish/service/ThermoXRFImportWorker.exe
