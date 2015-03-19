// TODO: REPL is nice for quick API exploration
// You can access all the standard .NET libraries

open System.Windows.Forms
MessageBox.Show("Hello world!")

// DEMO: Add FSharp.Data via NuGet and reference it in REPL

#r @"..\packages\FSharp.Data.2.1.1\lib\net40\fsharp.data.dll"
open FSharp.Data

// TODO: Weather in the capital city of CZ using type providers

let url = "http://api.openweathermap.org/data/2.5/weather?units=metric&q="
let wb = WorldBankData.GetDataContext()
let city = wb.Countries.``Czech Republic``.CapitalCity
Http.RequestString(url + city)

// TODO: Using JSON type provider to parse the JSON

type Weather = JsonProvider<"C:/data/Weather/london.json">
let w = Weather.Load(url + "Redmond")

w.Name
w.Sys.Country
w.Main.Temp

// TODO: Wrap the code as a reusable function (C# static method)

let getTemp city = 
  let w = Weather.Load(url + city)
  w.Main.Temp

getTemp "Redmond"
getTemp "London,UK"