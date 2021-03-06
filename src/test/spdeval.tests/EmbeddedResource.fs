﻿namespace sdpeval.tests

module EmbeddedResource =
    
    open System
    open sdpeval.tests.F
    
    let logger = Common.Logging.LogManager.GetLogger("EmbeddedResource")
    
    type ResourceName private (resourceName:string) =
        member x.Value = resourceName
        static member private validator value = 
            match value with
            |v when (String.IsNullOrWhiteSpace(v)) -> Result.Error (new Exception("Resource name cannot be empty."))
            |v -> Result.Ok v
        static member createWithContinuation success failure value =
            match (createWithContinuationGeneric success failure ResourceName.validator value) with
            |Ok v -> Result.Ok (ResourceName v)
            |Result.Error ex -> Result.Error ex
        static member create value =
            match (createGeneric ResourceName.validator value) with
            |Ok v -> Result.Ok (ResourceName v)
            |Result.Error ex -> Result.Error ex

    open System.Reflection
           
    let debugPrintResourceNames (assembly:Assembly) =
        nullGuard assembly "assembly" |> ignore
        if(logger.IsDebugEnabled) then
            let resourceNames = assembly.GetManifestResourceNames()
            logger.Debug( new Msg(fun m ->m.Invoke((sprintf "Assembly '%s' has %i embedded resources." assembly.FullName resourceNames.Length))|>ignore))
            resourceNames
                |> Array.toSeq
                |> Seq.map (fun rn -> logger.Debug(new Msg(fun m -> m.Invoke(sprintf "Embeded resource: %s"  rn)|>ignore)))
                |> Seq.toArray
                |> ignore
        ()

    let extractEmbeddedResourceInAssemblyToStream (resourceName:ResourceName, assembly:Assembly) =
        nullGuard assembly "assembly" |> ignore
        let resourceStream = assembly.GetManifestResourceStream(resourceName.Value)
        match resourceStream with
        |Null -> 
            debugPrintResourceNames assembly
            Result.Error (new Exception(sprintf "Failed to extract embedded resource '%s' from assembly '%s'." resourceName.Value assembly.FullName))
        |NotNull rs -> Result.Ok rs

    let extractEmbeddedResourceInAssemblyToFile (resourceName:ResourceName, assembly:Assembly, filePath) =
        nullGuard assembly "assembly" |> ignore
        try
            use fileStream = new System.IO.FileStream(filePath,System.IO.FileMode.OpenOrCreate,System.IO.FileAccess.ReadWrite,System.IO.FileShare.None)
            let stream = extractEmbeddedResourceInAssemblyToStream (resourceName, assembly)
            match stream with
            | Ok s -> 
                let bufferLength = Convert.ToInt32(s.Length)
                let mutable buffer : byte[]= Array.zeroCreate bufferLength
                s.Read(buffer,0, buffer.Length) |> ignore
                fileStream.Write(buffer,0,buffer.Length) |> ignore
                s.Dispose()
                Result.Ok filePath
            |Result.Error ex -> Result.Error ex    
        with
        | _ as ex -> Result.Error ex
        
    let resourceNameToPartialResourceNames (resourceName:string) =
        let split = resourceName.Split([|'.'|])
        let length = split.Length
        seq{
            for i in 0..(length-1) do
                let partialResourceName = System.String.Join<string>(".",split.[0..i])
                yield partialResourceName
        } 
        |> Seq.toArray
        |> Seq.rev
        
    let resourceNameToFileName (resourceName:string, dictionary: System.Collections.Generic.IDictionary<string,string>) =          
            let partialResourceNames = resourceNameToPartialResourceNames resourceName
            let directoryPartialName =
                partialResourceNames |> Seq.tryFind (fun x -> dictionary.ContainsKey(x))
            let fileName = 
                match directoryPartialName with
                | Some pn -> 
                    let fileN = System.IO.Path.Combine(dictionary.[pn],resourceName.Replace(pn,"").Trim('.'))
                    Some fileN
                | None -> None
            fileName
    
    let getAllEmbeddedResourceNamesBase (assembly:Assembly) =        
        assembly.GetManifestResourceNames()    
    
    let getAllEmbeddedResourceNames =
        let resourceAssembly = typeof<ThisAssembly>.Assembly
        getAllEmbeddedResourceNamesBase resourceAssembly

    let embeddedResourceExistsBase resourceName resourceAssembly =
        (getAllEmbeddedResourceNamesBase resourceAssembly)
        |> Seq.exists (fun rn -> rn = resourceName)

    let embeddedResourceExists resourceName =
        let resourceAssembly = typeof<ThisAssembly>.Assembly
        embeddedResourceExistsBase resourceName resourceAssembly

    let extractEmbededResouceToFile (resourceName:string , destinationFileName:string) = 
        try
            match(ResourceName.create resourceName) with
            |Result.Ok rn ->
                logger.Info(msg (sprintf "Extracting resource '%s' -> '%s'" resourceName destinationFileName))
                let fileResult = 
                    extractEmbeddedResourceInAssemblyToFile (rn,typeof<ThisAssembly>.Assembly, destinationFileName)
                fileResult                
            |Result.Error ex ->
                toErrorResult (sprintf "Failed to extract resource '%s'" resourceName) (Some ex)
        with
        |ex ->
            Result.Error ex

    let extractEmbeddedResourceBase (resourceName, destinationFolderPath, destinationFileName, assembly: Assembly) =
        result {                
                let! exeResourceName = 
                    ResourceName.create resourceName
                let exeFilePath = 
                    System.IO.Path.Combine(destinationFolderPath, destinationFileName)
                let! fileResult = 
                    extractEmbeddedResourceInAssemblyToFile (exeResourceName,assembly, exeFilePath)
                return fileResult
            }

    let extractEmbeddedResource (resourceName, destinationFolderPath, destinationFileName) =
        let assembly = typeof<ThisAssembly>.Assembly
        extractEmbeddedResourceBase (resourceName, destinationFolderPath, destinationFileName, assembly)
    
    let extractEmbeddedResouceByFileNameBase (fileName, destinationFolderPath, destinationFileName, assembly:Assembly) =
        let resourceNames = 
            getAllEmbeddedResourceNamesBase assembly
            |>Seq.filter (fun rn -> rn.EndsWith(fileName))
            |>Seq.sort
            |>Seq.toArray            
        match resourceNames.Length > 0 with
        | true -> 
            extractEmbeddedResourceBase (resourceNames.[0],destinationFolderPath,destinationFileName, assembly)
        | false -> raise (new Exception("File not found in embedded resource: " + fileName))

    let extractEmbeddedResouceByFileName (fileName, destinationFolderPath, destinationFileName) =
        let assembly = typeof<ThisAssembly>.Assembly
        extractEmbeddedResouceByFileNameBase (fileName, destinationFolderPath, destinationFileName, assembly)

    let mapResourceNamesToFileNames (destinationFolderPath, resourceNames:seq<string>,resourceNameToDirectoryDictionary)=
        let directoryLookDictionary = resourceNameToDirectoryDictionary destinationFolderPath
        resourceNames
        |> Seq.map (fun rn -> 
            let fileName = resourceNameToFileName (rn, directoryLookDictionary)
            match fileName with
            |Some fn -> Some (rn,fn)
            |None -> None
            )
        |> Seq.choose id

    [<AllowNullLiteral>]
    type ExtractedEmbeddedResource(fileName, logger:Common.Logging.ILog) =
        let tempFolderPath = 
            result {
                let nonExistingTempFolderPath = (System.IO.Path.Combine(System.IO.Path.GetTempPath(),(Guid.NewGuid().ToString())))
                let! existingTempFolderPath = ensureDirectoryExists true nonExistingTempFolderPath
                return existingTempFolderPath
            }

        let tempFilePath =
            result{
                let! folderPath = tempFolderPath                
                let! extractedFile = extractEmbeddedResouceByFileNameBase (fileName,folderPath,fileName,typeof<ThisAssembly>.Assembly)
                return extractedFile
            }

        member this.FilePath = tempFilePath
        interface IDisposable with
            member x.Dispose() = 
                logger.Debug(new Msg(fun m -> m.Invoke((sprintf "Disposing extracted embedded resource '%A'" tempFilePath))|>ignore))
                match (result{
                    let! folderPath = tempFolderPath
                    let! deleted = deleteDirectoryIfExists folderPath
                    return deleted                                                
                }) with
                |Result.Ok v -> ()
                |Result.Error ex -> raise ex                
    