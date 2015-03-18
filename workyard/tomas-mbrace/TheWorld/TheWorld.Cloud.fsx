// Load credentials, reference FsLab and all relevant MBrace packages
#load "credentials.fsx"
#load "../packages/FsLab/FsLab.fsx"
#r "System.Xml.Linq.dll"
open MBrace
open MBrace.Azure.Client
open FsLab
open Deedle
open Foogle
open FSharp.Data

// Connect to the cluster
let cluster = Runtime.GetHandle(config)
cluster.ShowProcesses()
cluster.ShowWorkers()

/// Function that downloads one page of WorldBank data
let getPage indCode year page = 
  Http.RequestString
    ( "http://api.worldbank.org/countries/indicators/" + indCode, 
      query=[ "api_key", "hq8byg8k7t2fxc6hp7jmbx26"; 
              "date", sprintf "%d:%d" year year;
              "per_page", "100"; "page", string page] )

// Generate type for parsing WorldBank responses
type WB = XmlProvider<"http://api.worldbank.org/countries/indicators/SP.POP.TOTL?date=2000:2000&per_page=100">

/// Download information for all countries in cloud 
let getIndicator year indCode = cloud {
  let info = WB.Parse(getPage indCode year 1)
  let! series =
    [ for p in 1 .. info.Pages -> cloud {
        let page = WB.Parse(getPage indCode year p)
        return
          [ for d in page.Datas -> d.Country.Value => Option.map float d.Value ]
          |> Series.ofOptionalObservations }]
    |> Cloud.Parallel
  return Series.mergeAll series 
}

/// Get population in 2000 and 2010 and calculate growth
let growthCloud = cloud {
  let wb = WorldBankData.GetDataContext()
  let world = wb.Countries.World.Indicators
  let! pop2000work = 
      world.``Population, total``.IndicatorCode 
      |> getIndicator 2000 |> Cloud.StartChild
  let! pop2010 = 
      world.``Population, total``.IndicatorCode 
      |>  getIndicator 2010 
  let! pop2000 = pop2000work
  return (pop2010 - pop2000) / pop2010 * 100.0
}

// Get the series with country growths and show chart
let work1 = growthCloud |> cluster.CreateProcess
let growth = work1.AwaitResult()
Chart.GeoChart(Series.observations growth)

/// Get 5 top countries with the largest & smallest growth
let top5Cloud = cloud {
  let! growth = growthCloud
  let byGrowth = 
      growth
      |> Series.dropMissing
      |> Series.sort
      |> Series.mapValues int
  return Series.take 5 byGrowth,
         Series.take 5 (Series.rev byGrowth)
}

// Get the series with results from the cloud
let work2 = top5Cloud |> cluster.CreateProcess
work2.ShowInfo()
let small5, large5 = work2.AwaitResult()

