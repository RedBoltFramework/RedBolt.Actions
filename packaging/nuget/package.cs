// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

#:project ../../../RedBolt/eng/RedBolt.ModuleBuild/Packaging/RedBolt.ExternalModule.Packaging.csproj

using System.Runtime.CompilerServices;
using RedBolt.ExternalModule.Packaging;

return ExternalModulePackager.Run(args, new ExternalModulePackageManifest
{
    Name = "RedBolt.Actions",
    BuildSteps =
    [
        new("Actions/RedBolt.Actions.csproj", Arguments: "-p:BuildProjectReferences=false")
    ],
    ManagedPackages =
    [
        new(
            "RedBolt.Actions",
            "Composable time-based actions for RedBolt objects and scene nodes.",
            [new("build/bin/Release/linux-x64/core/RedBolt.Actions.dll", "lib/net10.0/RedBolt.Actions.dll")],
            [
                new("RedBolt.Core", PackageVersions.RedBolt)
            ])
    ],
    Consumer = new(
        "RedBolt.Actions",
        """
        using System;
        using RedBolt.Actions;
        Console.WriteLine(typeof(ActionManager).FullName);
        """)
}, SourcePath());

static string SourcePath([CallerFilePath] string path = "") => path;
