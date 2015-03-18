#load @"..\packages\FSharp.Azure.StorageTypeProvider\StorageTypeProvider.fsx"

// Connect to Azure
open FSharp.Azure.StorageTypeProvider
open FSharp.Azure.StorageTypeProvider.Table

type Population =
    { Region : string
      Year : int
      Population : int64 }

#load "../packages/FsLab/FsLab.fsx"
open FSharp.Data

type WBData = JsonProvider<"""[
  {
    "page": 1,
    "pages": 3,
    "per_page": "100",
    "total": 248
  },
  [
    {
      "indicator": {
        "id": "SP.POP.TOTL",
        "value": "Population, total"
      },
      "country": {
        "id": "1A",
        "value": "Arab World"
      },
      "value": "4139014872",
      "decimal": "1",
      "date": "1990"
    },
    {
      "indicator": {
        "id": "SP.POP.TOTL",
        "value": "Population, total"
      },
      "country": {
        "id": "S3",
        "value": "Caribbean small states"
      },
      "decimal": "1",
      "date": "1990"
    } ] ]""">

let records =
    System.IO.Directory.GetFiles(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..\..\data\SP.POP.TOTL"))
    |> Seq.map WBData.Load
    |> Seq.collect(fun d -> d.Array |> Array.toSeq)
    |> Seq.map(fun r ->
        { Region = r.Country.Value
          Year = r.Date
          Population = r.Value |> defaultArg <| 0L })
    |> Seq.filter(fun r -> r.Population > 0L)
    |> Seq.toList

type Azure = AzureTypeProvider< "UseDevelopmentStorage=true", "">

records
|> List.map(fun c ->
    Table.Partition (c.Year.ToString()),
    Table.Row (c.Region),
    c)
|> Azure.Tables.Population.Insert
