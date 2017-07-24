#tool "nuget:?package=GitVersion.CommandLine"

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
    GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = source + "/CommonAssamblyInfo.cs"
    });
    DotNetCoreBuild(source, new DotNetCoreBuildSettings {
        Configuration = "Release"
    });
});

Task("Test").Does(() => {
  var settings = new DotNetCoreTestSettings {
      Configuration = "Release"
  };
  var projectFiles = GetFiles(source + "/test/**/*.csproj");
  foreach(var file in projectFiles)
  {
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