﻿namespace sdpeval.Tests

open NUnit.Framework

[<TestFixture>]
module SdpCatalogTests =
    open sdpeval.F
    open sdpeval.tests
    open sdpeval.fsharp.Sdp    
    open System
    type ThisAssembly = { Empty:string;}
    let logger = Common.Logging.Simple.ConsoleOutLogger("SdpCatalogTests",Common.Logging.LogLevel.All,true,true,true,"yyyy-MM-dd-HH-mm-ss-ms")
    

    [<Test>]
    [<Category(TestCategory.UnitTests)>]
    [<TestCase(null,"N/A",false,"ProductCode cannot be null.")>]
    [<TestCase("","N/A",false,"ProductCode cannot be empty.")>]
    [<TestCase("abc-bcd","N/A",false,"Invalid product code: 'abc-bcd'")>]
    [<TestCase("{FB2B7CC0-8307-47e6-A065-11015CC96B99}","{FB2B7CC0-8307-47e6-A065-11015CC96B99}",true,"N/A")>]
    let productCodeTests (guid:string,expectedGuid:string,isSuccess:bool,expectedErrorMessage:string) =
        match (result {
            let! actual = sdpeval.fsharp.Sdp.productCode guid
            return actual
        }) with        
        |Ok v -> 
            Assert.True(isSuccess, "Expected failed but succeeded instead.")
            Assert.AreEqual(expectedGuid,(sdpeval.fsharp.Sdp.productCodeValue v),"Product code value not expected")
        |Error ex -> 
            Assert.False(isSuccess,sprintf "Expected success but failed instead: %s" ex.Message)
            Assert.IsTrue(ex.Message.Contains(expectedErrorMessage),"Error message not as expected: " + ex.Message)

    type internal SdpTestData ={SdpTestFile:string;IsSuccess:bool;Expected:SoftwareDistributionPackage;ExpectedErrorMessage:string}

    let internal sdpTestData =
        [
            let installerData = InstallerData.CommandLineInstallerData{Program="Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE";Arguments="/s";RebootByDefault=false;DefaultResult=InstallationResult.Failed;ReturnCodes=[|{Code=0;Result=InstallationResult.Succeeded;Reboot=false};{Code=2;Result=InstallationResult.Succeeded;Reboot=true}|]}
            let installProperties = {CanRequestUserInput=false;Impact=InstallImpact.Normal;RequiresNetworkConnectivity=false;RebootBehavior=InstallationRebootBehavior.AlwaysRequiresReboot}
            let originFile = {Digest="WCGlVdICgjVY0T0q3DMQE6jy8LY=";FileName="Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE";Size=166945344L;Modified=new DateTime(2018,6,19,8,22,57);OriginUri="http://downloads.dell.com/FOLDER05035765M/5/Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE"}
            let installableItems = [|{Id="3787be40-ec14-47c7-a24d-23210ef448e9";ApplicabilityRules={IsInstallable=Some "N/A";IsInstalled=Some "N/A";IsSuperseded=Some "N/A"};InstallProperties=installProperties;UninstallProperties=None;InstallerData=installerData;OriginFile=originFile}|]
            yield {SdpTestFile="0e6cf4ac-2853-48aa-825b-8fe28206575f.sdp";IsSuccess=true;Expected={Title="Realtek High Definition Audio Driver,6.0.1.8454,A02";MoreInfoUrl="http://www.dell.com/support/home/us/en/19/Drivers/DriversDetails?driverId=99DDD";CreationDate=new DateTime(2018,10,03); Description="This package contains the driver for Realtek High-Definition audio codec and is supported on Dell products that run the Windows 10 operating system. Audio driver is the software that helps your operating system to communicate effectively with audio devices such as sound cards and speakers. The package supports Windows 10 Fall Creators Update.";ProductName="Drivers and Applications";PackageId="0e6cf4ac-2853-48aa-825b-8fe28206575f";UpdateSpecificData={MsrcSeverity= MsrcSeverity.Important;UpdateClassification=UpdateClassification.Updates;SecurityBulletinID=Some "99DDD";KBArticleID=Some "99DDD"};IsInstallable="";IsInstalled=None;InstallableItems=installableItems};ExpectedErrorMessage="N/A"}

            let installerData = InstallerData.MsiInstallerData{MsiFile="DSIAPC_1.6.6.5.msi";CommandLine="Reboot=ReallySuppress";ProductCode=(productCodeUnsafe "{FB2B7CC0-8307-47e6-A065-11015CC96B99}");UninstallCommandLine=None }
            let installProperties = {CanRequestUserInput=false;Impact=InstallImpact.Normal;RequiresNetworkConnectivity=false;RebootBehavior=InstallationRebootBehavior.NeverReboots}
            let originFile = {Digest="7sJyfjiOzAzMVjZ7FqJLS0hQsG0=";FileName="DSIAPC_1.6.6.5.msi";Size=26825728L;Modified=new DateTime(2019,1,24,8,52,15);OriginUri="http://downloads.dell.com/FOLDER05427436M/1/DSIAPC_1.6.6.5.msi"}
            let installableItems = [|{Id="5968b32a-dc30-4186-8652-ecc4a7cdc684";ApplicabilityRules={IsInstallable=Some "N/A";IsInstalled=Some "N/A";IsSuperseded=Some "N/A"};InstallProperties=installProperties;UninstallProperties=None;InstallerData=installerData;OriginFile=originFile}|]
            yield {SdpTestFile="dc718411-a5f1-4b15-966a-63a77cbb156e.sdp";IsSuccess=true;Expected={Title="Dell OpenManage Inventory Agent (for Dell Business Client Systems)";MoreInfoUrl="http://downloads.dell.com/cmsdk/DSIA_Readme.doc";CreationDate=new DateTime(2019,01,24);Description="Dell OpenManage Inventory Agent is a data provider for Microsoft WMI to inventory information for Dell supported devices on Dell Precision workstations, OptiPlex desktops, and Latitude laptops. This service is run at boot time and then stopped automatically.";ProductName="Drivers and Applications";PackageId="dc718411-a5f1-4b15-966a-63a77cbb156e";UpdateSpecificData={MsrcSeverity= MsrcSeverity.Important;UpdateClassification=UpdateClassification.Updates;SecurityBulletinID=Some "45YPT";KBArticleID=Some "45YPT"};IsInstallable="";IsInstalled=None;InstallableItems=installableItems};ExpectedErrorMessage="N/A"}
        ]

    [<Test>]
    [<Category(TestCategory.UnitTests)>]
    [<TestCaseSource("sdpTestData")>]    
    let loadSdpTests(sdpTestData:obj) =
        let sdpTestDataR = (sdpTestData:?>SdpTestData)
        match(result {            
            let tempDestinationFolderPath = System.IO.Path.GetTempPath()
            let! sdpFilePath = EmbeddedResource.extractEmbeddedResouceByFileNameBase (sdpTestDataR.SdpTestFile,tempDestinationFolderPath,sdpTestDataR.SdpTestFile,typeof<ThisAssembly>.Assembly)
            let! sdpXDocument = sdpeval.fsharp.Sdp.loadSdpXDocument sdpFilePath
            let! sdpXElement = sdpeval.fsharp.Sdp.loadSdpXElement sdpXDocument
            let! actual = sdpeval.fsharp.Sdp.loadSdpFromXElement sdpXElement
            return actual
        }) with
        |Ok v -> 
            Assert.True(sdpTestDataR.IsSuccess, "Expected failed but succeeded instead.")
            Assert.AreEqual(sdpTestDataR.Expected.Title, v.Title,"Title")
            Assert.AreEqual(sdpTestDataR.Expected.Description, v.Description,"Description")
            Assert.AreEqual(sdpTestDataR.Expected.ProductName, v.ProductName,"ProductName")
            Assert.AreEqual(sdpTestDataR.Expected.PackageId, v.PackageId,"PackageId")
            Assert.AreEqual(sdpTestDataR.Expected.UpdateSpecificData.KBArticleID, v.UpdateSpecificData.KBArticleID,"KBArticleID")
            Assert.AreEqual(sdpTestDataR.Expected.UpdateSpecificData.SecurityBulletinID, v.UpdateSpecificData.SecurityBulletinID,"SecurityBulletinID")
            Assert.AreEqual(sdpTestDataR.Expected.UpdateSpecificData.UpdateClassification, v.UpdateSpecificData.UpdateClassification,"UpdateClassification")
            Assert.AreEqual(sdpTestDataR.Expected.UpdateSpecificData.MsrcSeverity, v.UpdateSpecificData.MsrcSeverity,"MsrcSeverity")
            Assert.IsNotEmpty(v.IsInstallable,"IsInstallable")
            Assert.AreEqual(sdpTestDataR.Expected.IsInstalled, v.IsInstalled,"IsInstalled")
            Assert.IsFalse((v.InstallableItems.[0].ApplicabilityRules.IsInstallable) = None,"InstallableItems..IsInstallable")
            Assert.IsFalse((v.InstallableItems.[0].ApplicabilityRules.IsInstalled) = None,"InstallableItems..IsInstalled")
            Assert.IsTrue((v.InstallableItems.[0].ApplicabilityRules.IsSuperseded) = None,"InstallableItems..IsSuperseded")
            Assert.AreEqual(sdpTestDataR.Expected.InstallableItems.[0].InstallerData, v.InstallableItems.[0].InstallerData,"InstallerData")
            Assert.AreEqual(sdpTestDataR.Expected.InstallableItems.[0].OriginFile, v.InstallableItems.[0].OriginFile,"OriginFile")
            Assert.AreEqual(sdpTestDataR.Expected.InstallableItems.[0].InstallProperties, v.InstallableItems.[0].InstallProperties,"InstallProperties")
            Assert.AreEqual(sdpTestDataR.Expected.InstallableItems.[0].UninstallProperties, v.InstallableItems.[0].UninstallProperties,"UninstallProperties")
            Assert.AreEqual(sdpTestDataR.Expected.InstallableItems.[0].Id, v.InstallableItems.[0].Id,"Id")

        |Error ex -> 
            Assert.False(sdpTestDataR.IsSuccess,sprintf "Expected success but failed instead: %s" ex.Message)
            Assert.IsTrue(ex.Message.Contains(sdpTestDataR.ExpectedErrorMessage),"Error message not as expected: " + ex.Message)
    
    [<Test>]
    [<Category(TestCategory.UnitTests)>]
    [<TestCase("DellSDPCatalogPC.zip","DellSDPCatalogPC.xml",true,4450,"N/A")>]
    [<TestCase("HpCatalogForSms.zip","HpCatalogForSms.xml",true,1829,"N/A")>]
    [<Timeout(220000)>]
    let loadSystemManagementCatalogTests(sdpZipFileName:string,sdpFileName:string, isSuccess:bool,expectedNumberOfPackages:int,expectedErrorMessage:string) =        
        match(result {            
            
            let tempDestinationFolderPath = System.IO.Path.GetTempPath()
            let! sdpZipFilePath = EmbeddedResource.extractEmbeddedResouceByFileNameBase (sdpZipFileName,tempDestinationFolderPath,sdpZipFileName,typeof<ThisAssembly>.Assembly)
            use temporaryFolder = new F.TemporaryFolder(logger)
            let! temporaryFolderPath = temporaryFolder.FolderPath            
            let! nonExistingTempCatalogDestinationFolderPath = F.deleteDirectory true temporaryFolderPath
            let! existingTempCatalogDestinationFolderPath = F.ensureDirectoryExists true nonExistingTempCatalogDestinationFolderPath
            let! unZippedSdpFilePath = Compression.unzipFile (sdpZipFilePath,existingTempCatalogDestinationFolderPath, logger)
            let sdpCatalogFilePath = System.IO.Path.Combine (existingTempCatalogDestinationFolderPath,sdpFileName)           
            let! actual = sdpeval.fsharp.Sdp.loadSystemManagementCatalog sdpCatalogFilePath
            return actual
        }) with        
        |Ok v -> 
            Assert.AreEqual(expectedNumberOfPackages,(Array.length v.SoftwareDistributionPackages),sprintf "Number of SoftwareDistributionPackages in sdp xml '%s'" sdpFileName)
        |Error ex -> 
            Assert.False(isSuccess,sprintf "Expected success but failed instead: %s" ex.Message)
            Assert.IsTrue(ex.Message.Contains(expectedErrorMessage),"Error message not as expected: " + ex.Message)



    [<Test>]
    [<Category(TestCategory.UnitTests)>]
    [<TestCase("HpCatalogForSms.latest_V2.zip","HpCatalogForSms.latest_V2",true,1829,"N/A")>]
    [<TestCase("DellSDPCatalogPC_V2.zip","DellSDPCatalogPC_V2",true,4450,"N/A")>]
    [<Timeout(220000)>]
    let loadSdpsTests(sdpZipFileName:string,testFolder:string,isSuccess:bool,expectedNumberOfPackages:int,expectedErrorMessage:string) =        
        match(result {            
            
            let tempDestinationFolderPath = System.IO.Path.GetTempPath()
            let! sdpZipFilePath = EmbeddedResource.extractEmbeddedResouceByFileNameBase (sdpZipFileName,tempDestinationFolderPath,sdpZipFileName,typeof<ThisAssembly>.Assembly)            
            use temporaryFolder = new F.TemporaryFolder(logger)
            let! temporaryFolderPath = temporaryFolder.FolderPath
            let! unZippedSdpFilePath = Compression.unzipFile (sdpZipFilePath,temporaryFolderPath, logger)
            let! actual = sdpeval.fsharp.Sdp.loadSdps temporaryFolderPath
            return actual
        }) with        
        |Ok v -> 
            Assert.AreEqual(expectedNumberOfPackages,(Array.length v),sprintf "Number of SoftwareDistributionPackages in sdp xmls in folder '%s'" testFolder)
        |Error ex -> 
            Assert.False(isSuccess,sprintf "Expected success but failed instead: %s" ex.Message)
            Assert.IsTrue(ex.Message.Contains(expectedErrorMessage),"Error message not as expected: " + ex.Message)


    let internal sdpTestData2 =
        [
            let installerData = InstallerData.CommandLineInstallerData{Program="Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE";Arguments="/s";RebootByDefault=false;DefaultResult=InstallationResult.Failed;ReturnCodes=[|{Code=0;Result=InstallationResult.Succeeded;Reboot=false};{Code=2;Result=InstallationResult.Succeeded;Reboot=true}|]}
            let installProperties = {CanRequestUserInput=false;Impact=InstallImpact.Normal;RequiresNetworkConnectivity=false;RebootBehavior=InstallationRebootBehavior.AlwaysRequiresReboot}
            let originFile1 = {Digest="WCGlVdICgjVY0T0q3DMQE6jy8LY=";FileName="2_Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE";Size=166945344L;Modified=new DateTime(2017,6,19,8,22,57);OriginUri="http://downloads.dell.com/FOLDER05035765M/5/Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE"}
            let originFile2 = {Digest="WCGlVdICgjVY0T0q3DMQE6jy8LY=";FileName="1_Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE";Size=166945344L;Modified=new DateTime(2018,6,19,8,22,57);OriginUri="http://downloads.dell.com/FOLDER05035765M/5/Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE"}
            let originFile3 = {Digest="WCGlVdICgjVY0T0q3DMQE6jy8LY=";FileName="3_Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE";Size=166945344L;Modified=new DateTime(2016,6,19,8,22,57);OriginUri="http://downloads.dell.com/FOLDER05035765M/5/Realtek-High-Definition-Audio-Driver_99DDD_WIN_6.0.1.8454_A02_03.EXE"}
            let installableItems = [|
                                    {Id="1";ApplicabilityRules={IsInstallable=Some "N/A";IsInstalled=Some "N/A";IsSuperseded=Some "N/A"};InstallProperties=installProperties;UninstallProperties=None;InstallerData=installerData;OriginFile=originFile1};
                                    {Id="2";ApplicabilityRules={IsInstallable=Some "N/A";IsInstalled=Some "N/A";IsSuperseded=Some "N/A"};InstallProperties=installProperties;UninstallProperties=None;InstallerData=installerData;OriginFile=originFile2};
                                    {Id="3";ApplicabilityRules={IsInstallable=Some "N/A";IsInstalled=Some "N/A";IsSuperseded=Some "N/A"};InstallProperties=installProperties;UninstallProperties=None;InstallerData=installerData;OriginFile=originFile3};
                                    |]
            yield {SdpTestFile="0e6cf4ac-2853-48aa-825b-8fe28206575f.sdp";IsSuccess=true;Expected={Title="Realtek High Definition Audio Driver,6.0.1.8454,A02";MoreInfoUrl="http://www.dell.com/support/home/us/en/19/Drivers/DriversDetails?driverId=99DDD";CreationDate=new DateTime(2018,10,03); Description="This package contains the driver for Realtek High-Definition audio codec and is supported on Dell products that run the Windows 10 operating system. Audio driver is the software that helps your operating system to communicate effectively with audio devices such as sound cards and speakers. The package supports Windows 10 Fall Creators Update.";ProductName="Drivers and Applications";PackageId="0e6cf4ac-2853-48aa-825b-8fe28206575f";UpdateSpecificData={MsrcSeverity= MsrcSeverity.Important;UpdateClassification=UpdateClassification.Updates;SecurityBulletinID=Some "99DDD";KBArticleID=Some "99DDD"};IsInstallable="";IsInstalled=None;InstallableItems=installableItems};ExpectedErrorMessage="N/A"}
        ]

    [<Test>]
    [<Category(TestCategory.UnitTests)>]
    [<TestCaseSource("sdpTestData2")>]    
    let getReleaseDateTest (sdpTestData2:obj) =
       let sdpTestData2R = (sdpTestData2:?>SdpTestData)
       let actual = sdpeval.fsharp.Sdp.getSdpReleaseDate sdpTestData2R.Expected
       let expected = new DateTime(2018,6,19,8,22,57)
       Assert.AreEqual(expected, actual)
