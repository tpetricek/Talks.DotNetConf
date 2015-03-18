#load "credentials.fsx"
open MBrace
open MBrace.Azure.Client
open MBrace.Azure.Runtime
open MBrace.Streams

// Connect to the cluster
let cluster = Runtime.GetHandle(config)
cluster.AttachClientLogger(new Runtime.ConsoleLogger())

// Show me the workers on the cluster
cluster.ShowWorkers()


// It's easy to lift work into the cloud.
let helloWorld = 2 + 2

let helloWorldAsync = async { return 2 + 2 }
helloWorldAsync |> Async.RunSynchronously

let helloWorldCloud = cloud { return 2 + 2 }
helloWorldCloud |> cluster.Run



// Parallelise multiple jobs at once!
let lotsOfWorkflows =
    [ 1 .. 50 ]
    |> List.map(fun number -> cloud { return sprintf "i'm job %d on machine %s" number System.Environment.MachineName })
    |> Cloud.Parallel
    |> cluster.Run

// LINQ-style
let numbers =
    [| 1..100 |]
    |> CloudStream.ofArray
    |> CloudStream.map (fun num -> num * num)
    |> CloudStream.filter (fun num -> num < 2500)
    |> CloudStream.countBy (fun num -> if num % 2 = 0 then "Even" else "Odd")
    |> CloudStream.toArray
    |> cluster.Run









