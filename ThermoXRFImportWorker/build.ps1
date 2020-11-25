dotnet restore
dotnet publish -o /publish/service
sc.exe create MyWorkerService binpath= ./publish/service/ThermoXRFImportWorker.exe
