#load "../packages/FsLab.0.1.4/FsLab.fsx"
open FsLab
open Deedle
open Foogle
open FSharp.Data

// ----------------------------------------------------------------------------
// Working with WorldBank REST API
// ----------------------------------------------------------------------------

// TODO: Get population of a country in 2000 using WorldBank
// TODO: Save indicators 'world' and 'indCode'

// DEMO: Download page of data from WorldBank
  
// TODO: Generate type for parsing WorldBank responses ('WB')
// http://api.worldbank.org/countries/all/indicators/SP.POP.TOTL?format=json&date=2000:2000&per_page=100

// DEMO: Download information for all countries ('wb2000' and 'wb2010')
// TODO: Show population in 2010 (Series.observations & Chart.GeoChart)
// TODO: Calculate & show relative change (p10-p00)/p10*100%
















// ----------------------------------------------------------------------------
// Working with WorldBank data from Azure
// ----------------------------------------------------------------------------

// DEMO: Connecting to Azure storage

#load @"..\packages\FSharp.Azure.StorageTypeProvider.1.1.0\StorageTypeProvider.fsx"
open FSharp.Azure.StorageTypeProvider
type Azure = AzureTypeProvider< "UseDevelopmentStorage=true", "">

// TODO: Parse data from Azure containers

let azure2000json = 
  Azure.Containers.dotnetconf.``SP.POP.TOTL/``.``2000.json``.ReadAsString()
  |> WB.Parse

let azure2010json = 
  Azure.Containers.dotnetconf.``SP.POP.TOTL/``.``2010.json``.ReadAsString()
  |> WB.Parse

// TODO: Run the same analytics on Azure data

let azure2000 =
  [ for d in azure2000json.Array -> d.Country.Value => Option.map float d.Value ]
  |> Series.ofOptionalObservations
let azure2010 =
  [ for d in azure2010json.Array -> d.Country.Value => Option.map float d.Value ]
  |> Series.ofOptionalObservations

Chart.GeoChart(Series.observations azure2010)

let azureChange = (azure2010 - azure2000) / azure2010 * 100.0
Chart.GeoChart(Series.observations azureChange)
