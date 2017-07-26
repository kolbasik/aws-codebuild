#tool "nuget:?package=GitVersion.CommandLine"
#addin "Cake.FileHelpers"

var src = "./src";
var dst = "./artifacts";

Task("Clean").Does(() => {
    CleanDirectories(dst);
    CleanDirectories(src + "/**/bin");
    CleanDirectories(src + "/**/obj");
    CleanDirectories(src + "/**/pkg");
    DotNetCoreClean(src);
});

Task("Restore").Does(() => {
    EnsureDirectoryExists(dst);
    DotNetCoreRestore(src);
});

Task("Build").Does(() => {
    var version = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json,
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = src + "/CommonAssamblyInfo.cs"
    });
    Information("GitVersion = {");
    Information("   FullSemVer: {0}", version.FullSemVer);
    Information("   LegacySemVer: {0}", version.LegacySemVer);
    Information("   MajorMinorPatch: {0}", version.MajorMinorPatch);
    Information("   InformationalVersion: {0}", version.InformationalVersion);
    Information("   Nuget v2 version: {0}", version.NuGetVersionV2);
    Information("}");
    FileWriteText(File(dst + "/VERSION"), version.FullSemVer);
    DotNetCoreBuild(src, new DotNetCoreBuildSettings {
        Configuration = "Release"
    });
});

Task("Test").Does(() => {
    var settings = new DotNetCoreTestSettings {
        Configuration = "Release"
    };
    foreach(var file in GetFiles(src + "/test/**/*.csproj")) {
        DotNetCoreTest(file.FullPath, settings);
    }
});

Task("Publish").Does(() => {
    var settings = new DotNetCorePublishSettings {
        Configuration = "Release"
    };
    DotNetCorePublish(src, settings);
    Zip(src + "/Lambda1/bin/Release/netcoreapp1.0/publish", dst + "/Lambda1.zip");
    Zip(src + "/Lambda2/bin/Release/netcoreapp1.0/publish", dst + "/Lambda2.zip");
    CopyFile("./buildspec-2.yml", dst + "/buildspec.yml");
    CopyFile("./serverless.yml", dst + "/serverless.yml");
    Information("Artifacts:");
    foreach(var file in GetFiles(dst + "/**/*.*")) {
        Information(file.FullPath);
    }
});

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Publish");

RunTarget(Argument("target", "Default"));