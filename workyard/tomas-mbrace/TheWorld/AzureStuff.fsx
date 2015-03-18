// Start Azure Storage emulator :)

#load @"..\packages\FSharp.Azure.StorageTypeProvider\StorageTypeProvider.fsx"

// Connect to Azure
open FSharp.Azure.StorageTypeProvider
type Azure = AzureTypeProvider< "UseDevelopmentStorage=true", "">

// Explore the storage account...

// First we'll look at blobs

// Start reading a file remotely
let file = Azure.Containers.messages.richard.OpenStreamAsText()

// Now look at Tables
let footballResults = Azure.Tables.FootballResults.GetPartition "Tottenham"

// Querying is so easy
let results =
    Azure.Tables.People.Query()
            .``Where Age Is``.``Greater Than``(30)
            .``Where Partition Key Is``.``Equal To``("UK")
            .Execute()



