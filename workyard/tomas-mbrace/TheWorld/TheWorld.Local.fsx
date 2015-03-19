#load "../packages/FsLab/FsLab.fsx"
open FsLab
open Deedle
open Foogle
open FSharp.Data


/// Function that downloads one page of WorldBank data
let getPage indCode year page = 
  Http.RequestString
    ( "http://api.worldbank.org/countries/all/indicators/" + indCode, 
      query=[ "format", "json"; "date", sprintf "%d:%d" year year;
              "per_page", "100"; "page", string page] )

// Generate type for parsing WorldBank responses
type WB = JsonProvider<"http://api.worldbank.org/countries/all/indicators/SP.POP.TOTL?format=json&date=2000:2000&per_page=100">

/// Download information for all countries in cloud 
let getIndicator year indCode = 
  let info = WB.Parse(getPage indCode year 1)
  let series =
    [ for p in 1 .. info.Record.Pages -> 
        let fn = sprintf @"C:\Tomas\Materials\Workyard\Talks.DotNetConf\workyard\data\%s_%d_%d.json" indCode year p
        System.IO.File.WriteAllText(fn, getPage indCode year p)
        let page = WB.Parse(getPage indCode year p)
        [ for d in page.Array -> 
            d.Country.Value => Option.map float d.Value ]
        |> Series.ofOptionalObservations ]
  Series.mergeAll series 

let wb = WorldBankData.GetDataContext()
let world = wb.Countries.World.Indicators

let pop2000 = getIndicator 2000 world.``Population, total``.IndicatorCode
let pop2010 = getIndicator 2010 world.``Population, total``.IndicatorCode

// ---------------------------------------

// HACKING - saving downloaded data
let pages = System.Collections.Generic.Dictionary<int*string, string>()

/// Download information for all countries in cloud 
let getIndicatorH year indCode = 
  let info = WB.Parse(getPage indCode year 1)
  let array =
   [| for p in 1 .. info.Record.Pages do
        let page = WB.Parse(getPage indCode year p)
        for a in  page.Array do yield a.JsonValue |] |> JsonValue.Array
  let merged = JsonValue.Array [| info.Record.JsonValue; array |]
  pages.[ (year, indCode) ] <- merged.ToString()

// HACKING - download "big data"
for y in 1990 .. 2010 do
  printfn "year = %d" y
  getIndicatorH y world.``School enrollment, tertiary (% gross)``.IndicatorCode
  |> ignore

// HACKING - save big data to files
for (KeyValue((y,code),v)) in pages do
  let dn = sprintf @"C:\Tomas\Materials\Workyard\Talks.DotNetConf\workyard\data\%s" code
  try System.IO.Directory.CreateDirectory(dn) |> ignore with _ -> ()
  let fn = sprintf @"C:\Tomas\Materials\Workyard\Talks.DotNetConf\workyard\data\%s\%d.json" code y
  System.IO.File.WriteAllText(fn, v)

// ---------------------------------------

let fromCloud = WB.Load(@"C:\Tomas\Materials\Workyard\Talks.DotNetConf\workyard\data\SP.POP.TOTL\1995.json")
fromCloud.Record.Total
fromCloud.Array |> Seq.length

// ---------------------------------------

Chart.GeoChart(Series.observations pop2000)

let change = (pop2010 - pop2000) / pop2010 * 100.0

Chart.GeoChart(Series.observations change)

// TODO: Isaac - Change this to WB.Parse(Azure.something.ReadFile())
// TODO: Tomas - see if we can find another interestin indicator with data

#load @"..\packages\FSharp.Azure.StorageTypeProvider\StorageTypeProvider.fsx"
open FSharp.Azure.StorageTypeProvider
type Azure = AzureTypeProvider< "UseDevelopmentStorage=true", "">


let gdp2000data = WB.Load(@"C:\Tomas\Materials\Workyard\Talks.DotNetConf\workyard\data\SP.POP.TOTL\2000.json")
let gdp2010data = WB.Load(@"C:\Tomas\Materials\Workyard\Talks.DotNetConf\workyard\data\SP.POP.TOTL\2010.json")

let gdp2000data = WB.Parse <| Azure.Containers.dotnetconf.``SP.POP.TOTL/``.``2000.json``.ReadAsString()
let gdp2010data = WB.Parse <| Azure.Containers.dotnetconf.``SP.POP.TOTL/``.``2010.json``.ReadAsString()

let gdp2000 =
  [ for d in gdp2000data.Array -> d.Country.Value => Option.map float d.Value ]
  |> Series.ofOptionalObservations
let gdp2010 =
  [ for d in gdp2010data.Array -> d.Country.Value => Option.map float d.Value ]
  |> Series.ofOptionalObservations

Chart.GeoChart(Series.observations gdp2010)

let gdpChange = (gdp2010 - gdp2000) / gdp2010 * 100.0
Chart.GeoChart(Series.observations gdpChange)
