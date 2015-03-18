// Start Azure Storage emulator :)

#load @"..\packages\FSharp.Azure.StorageTypeProvider\StorageTypeProvider.fsx"
open FSharp.Azure.StorageTypeProvider
open FSharp.Azure.StorageTypeProvider.Blob
open FSharp.Azure.StorageTypeProvider.Table

// Connect to Azure
type Azure = AzureTypeProvider< "UseDevelopmentStorage=true", "">

// Explore the storage account...

// First we'll look at blobs

// Start reading a file remotely
let file = Azure.Containers.dotnetconf.``aliceInWonderland.txt``

// Now look at Tables
let footballResults = Azure.Tables.FootballResults.GetPartition "Tottenham"

// Querying is so easy
let populationTable = Azure.Tables.Population
let display = Array.map(fun (row:Azure.Domain.PopulationEntity) -> row.Region, row.Year, row.Population)

// First get everything in a partition
populationTable.GetPartition("1990") |> display

// Now a custom, strongly typed query