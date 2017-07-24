#tool "nuget:?package=GitVersion.CommandLine"
#addin "Cake.FileHelpers"

var source = "./src";
var target = "./artifacts";

Task("Clean").Does(() => {
    CleanDirectories(target);
    CleanDirectories(source + "/**/bin");
    CleanDirectories(source + "/**/obj");
    DotNetCoreClean(source);
});

Task("Restore").Does(() => {
    DotNetCoreRestore(source);
});

Task("Build").Does(() => {
    var version = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json,
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = source + "/CommonAssamblyInfo.cs"
    });
    Information("MajorMinorPatch: {0}", version.MajorMinorPatch);
    Information("FullSemVer: {0}", version.FullSemVer);
    Information("LegacySemVer: {0}", version.LegacySemVer);
    Information("InformationalVersion: {0}", version.InformationalVersion);
    Information("Nuget v2 version: {0}", version.NuGetVersionV2);
    FileWriteText(File("VERSION"), version.FullSemVer);
    DotNetCoreBuild(source, new DotNetCoreBuildSettings {
        Configuration = "Release"
    });
});

Task("Test").Does(() => {
  var settings = new DotNetCoreTestSettings {
      Configuration = "Release"
  };
  var projectFiles = GetFiles(source + "/test/**/*.csproj");
  foreach(var file in projectFiles) {
      DotNetCoreTest(file.FullPath, settings);
  }
});

Task("Publish").Does(() => {
    var settings = new DotNetCorePublishSettings {
        Configuration = "Release"
    };
    DotNetCorePublish(source, settings);
    CreateDirectory(target);
    Zip(source + "/Lambda1/bin/Release/netcoreapp1.0/publish", target + "/Lambda1.zip");
    Zip(source + "/Lambda2/bin/Release/netcoreapp1.0/publish", target + "/Lambda2.zip");
    CopyFile("./buildspec-2.yml", target + "/buildspec.yml");
    CopyFile("./serverless.yml", target + "/serverless.yml");
});

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Publish");

RunTarget(Argument("target", "Default"));