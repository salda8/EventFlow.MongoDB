// The MIT License (MIT)
// 
// Copyright (c) 2015-2017 Rasmus Mikkelsen
// Copyright (c) 2015-2017 eBay Software Foundation
// https://github.com/eventflow/EventFlow.MongoDb
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

#r "System.IO.Compression.FileSystem"
#r "System.Xml"

#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=OpenCover"

using System.IO.Compression;
using System.Net;
using System.Xml;

var VERSION = GetArgumentVersion();
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath;
var CONFIGURATION = "Release";
var REGEX_NUGETPARSER = new System.Text.RegularExpressions.Regex(
    @"(?<group>[a-z]+)\s+(?<package>[a-z\.0-9]+)\s+\-\s+(?<version>[0-9\.]+)",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

// IMPORTANT DIRECTORIES
var DIR_OUTPUT_PACKAGES = System.IO.Path.Combine(PROJECT_DIR, "Build", "Packages");
var DIR_OUTPUT_REPORTS = System.IO.Path.Combine(PROJECT_DIR, "Build", "Reports");

// IMPORTANT FILES
var FILE_SOLUTION = System.IO.Path.Combine(PROJECT_DIR, "EventFlow.MongoDB.sln");

var RELEASE_NOTES = ParseReleaseNotes(System.IO.Path.Combine(PROJECT_DIR, "RELEASE_NOTES.md"));

// =====================================================================================================
Task("Default")
    .IsDependentOn("Package");

// =====================================================================================================
Task("Clean")
    .Does(() =>
        {
            CleanDirectories(new []
                {
                    DIR_OUTPUT_PACKAGES,
                    DIR_OUTPUT_REPORTS
                });
				
			DeleteDirectories(GetDirectories("**/bin"), true);
			DeleteDirectories(GetDirectories("**/obj"), true);
        });
	
// =====================================================================================================
Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
        {
			DotNetCoreRestore(
				".", 
				new DotNetCoreRestoreSettings()
				{
					ArgumentCustomization = aggs => aggs.Append(GetDotNetCoreArgsVersions())
				});
        });
		
// =====================================================================================================
Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
        {
            DotNetCoreBuild(
				".", 
				new DotNetCoreBuildSettings()
				{
					Configuration = CONFIGURATION,
					ArgumentCustomization = aggs => aggs
                        .Append(GetDotNetCoreArgsVersions())
                        .Append("/p:ci=true")
                        .Append("/p:SourceLinkEnabled=true")
				});
        });

// =====================================================================================================
Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
        {
            Information("Version: {0}", RELEASE_NOTES.Version);
            Information(string.Join(Environment.NewLine, RELEASE_NOTES.Notes));

			foreach (var project in GetFiles("./Source/**/*.csproj"))
			{
				var name = project.GetDirectory().FullPath;
				var version = VERSION.ToString();
				
				if ((name.Contains("Test") && !name.Contains("TestHelpers")) || name.Contains("Example"))
				{
					continue;
				}

                SetReleaseNotes(project.ToString());
							
				DotNetCorePack(
					name,
					new DotNetCorePackSettings()
					{
						Configuration = CONFIGURATION,
						OutputDirectory = DIR_OUTPUT_PACKAGES,
						NoBuild = true,
						ArgumentCustomization = aggs => aggs.Append(GetDotNetCoreArgsVersions())
					});
			}
        });

// =====================================================================================================
Task("All")
    .IsDependentOn("Package")
    .Does(() =>
        {

        });

// =====================================================================================================

Version GetArgumentVersion()
{
    return Version.Parse(EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "0.0.1");
}

string GetDotNetCoreArgsVersions()
{
	var version = GetArgumentVersion().ToString();
	
	return string.Format(
		@"/p:Version={0} /p:AssemblyVersion={0} /p:FileVersion={0} /p:ProductVersion={0}",
		version);
}

void SetReleaseNotes(string filePath)
{
    var releaseNotes = string.Join(Environment.NewLine, RELEASE_NOTES.Notes);

    var xmlDocument = new XmlDocument();
    xmlDocument.Load(filePath);

    var node = xmlDocument.SelectSingleNode("Project/PropertyGroup/PackageReleaseNotes") as XmlElement;
    if (node == null)
    {
        throw new Exception(string.Format(
            "Project {0} does not have a `<PackageReleaseNotes>UPDATED BY BUILD</PackageReleaseNotes>` property",
            filePath));
    }

    if (!AppVeyor.IsRunningOnAppVeyor)
    {
        Information("Skipping update of release notes");
        return;
    } 
    else
    {
        Information(string.Format("Setting release notes in '{0}'", filePath));
        
        node.InnerText = releaseNotes;

        xmlDocument.Save(filePath);
    }
}


RunTarget(Argument<string>("target", "Package"));