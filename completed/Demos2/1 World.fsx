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

let wb = WorldBankData.GetDataContext()
let world = wb.Countries.World.Indicators
let indCode = world.``Population, total``.IndicatorCode

// DEMO: Download page of data from WorldBank

let getPage indCode year page = 
  Http.RequestString
    ( "http://api.worldbank.org/countries/all/indicators/" + indCode, 
      query=[ "format", "json"; "date", sprintf "%d:%d" year year;
              "per_page", "100"; "page", string page] )
  
// TODO: Generate type for parsing WorldBank responses ('WB')
// http://api.worldbank.org/countries/all/indicators/SP.POP.TOTL?format=json&date=2000:2000&per_page=100

type WB = JsonProvider<"http://api.worldbank.org/countries/all/indicators/SP.POP.TOTL?format=json&date=2000:2000&per_page=100">
WB.Parse(getPage indCode 2000 1)

// DEMO: Download information for all countries

let getIndicator year indCode = seq {
  let info = WB.Parse(getPage indCode year 1)
  for p in 1 .. info.Record.Pages do 
    let page = WB.Parse(getPage indCode year p)
    for d in page.Array do 
      match d.Value with
      | Some v when v < 1500000000L -> 
          yield d.Country.Value => float v
      | _ -> () }

// TODO: Show population in 2010 (Series.observations & Chart.GeoChart)

let wb2000 = series(getIndicator 2000 indCode)
let wb2010 = series(getIndicator 2010 indCode)
Chart.GeoChart(Series.observations wb2000)

// TODO: Calculate & show relative change (p10-p00)/p10*100%

let wbChange = (wb2010 - wb2000) / wb2010 * 100.0
Chart.GeoChart(Series.observations wbChange)


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
