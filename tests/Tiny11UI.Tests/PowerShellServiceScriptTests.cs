using tiny11_ui.Models;
using tiny11_ui.Services;
using Xunit;
using System.Diagnostics;

namespace Tiny11UI.Tests;

public class PowerShellServiceScriptTests
{
    private static ComponentRemovalOptions BareOptions() => new()
    {
        RemoveEdge = false,
        RemoveOneDrive = false,
        RemoveCortana = false,
        RemoveTeams = false,
        RemoveXbox = false,
        DisableTelemetry = false,
        DisableWindowsUpdate = false,
        DisableSponsoredApps = false,
        DisableReservedStorage = false,
        DisableBitLocker = false,
        BypassTPM = false,
        BypassCPU = false,
        BypassRAM = false,
        BypassSecureBoot = false,
        BypassMSAccount = false,
        SkipNetworkConnection = false,
        SkipPrivacyQuestions = false,
        CleanupComponentStore = false,
        CompressFinalImage = false,
        RemoveHyperV = false,
        RemoveRecall = false,
        RemoveWidgets = false,
        RemoveCopilot = false,
        RemoveInputComponents = false,
        CleanupDriverStore = false
    };

    private static string GenerateScript(
        string isoPath = @"C:\images\windows.iso",
        string scratchPath = @"C:\scratch",
        string outputPath = @"C:\output\tiny11.iso",
        int editionIndex = 3)
    {
        var service = new PowerShellService(new LocalizationService());
        return service.PreviewScript(isoPath, scratchPath, outputPath, editionIndex, new ComponentRemovalOptions());
    }

    [Fact]
    public void EsdExport_ValidatesResultAndUsesSingleImageIndex()
    {
        var script = GenerateScript();
        var export = script.IndexOf("Assert-NativeSuccess 'ESD to WIM export'", StringComparison.Ordinal);
        var resetIndex = script.IndexOf("$editionIndex = 1", StringComparison.Ordinal);
        var mount = script.IndexOf("/mount-wim /wimfile:$wimPath /index:$editionIndex", StringComparison.Ordinal);

        Assert.True(export >= 0);
        Assert.True(resetIndex > export);
        Assert.True(mount > resetIndex);
    }

    [Fact]
    public void Script_HasFailFastAndOwnedResourceCleanup()
    {
        var script = GenerateScript();

        Assert.Contains("$ErrorActionPreference = 'Stop'", script);
        Assert.Contains("} catch {", script);
        Assert.Contains("} finally {", script);
        Assert.Contains("if ($wimMounted -and (Test-Path $mountDir))", script);
        Assert.Contains("if ($isoMounted)", script);
        Assert.DoesNotContain("/cleanup-wim", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GetProcessesByName", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsoOutput_IsBuiltTemporarilyAndAtomicallyPublished()
    {
        var script = GenerateScript();

        Assert.Contains("$temporaryOutputPath", script);
        Assert.Contains("Assert-NativeSuccess 'ISO creation'", script);
        Assert.Contains("[System.IO.File]::Replace($temporaryOutputPath, $outputPath", script);
        Assert.Contains("[System.IO.File]::Move($temporaryOutputPath, $outputPath)", script);
        Assert.DoesNotContain("$isoDir $outputPath", script);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public async Task InitialRegistryCleanup_SkipsAbsentHivesAndChecksNativeFailures(bool hiveExists, int exitCode)
    {
        var script = GenerateScript();
        Assert.Contains("Clear-InitialRegistryHive 'HKLM\\OFFLINE_SOFTWARE'", script);
        Assert.Contains("Clear-InitialRegistryHive 'HKLM\\OFFLINE_SYSTEM'", script);
        Assert.Contains("Clear-InitialRegistryHive 'HKU\\OFFLINE_NTUSER'", script);
        Assert.Contains("[Console]::Error.WriteLine($_.InvocationInfo.PositionMessage)", script);
        var start = script.IndexOf("function Clear-InitialRegistryHive", StringComparison.Ordinal);
        var end = script.IndexOf("function Set-OfflineRegistryValue", start, StringComparison.Ordinal);
        var helper = script[start..end];
        // Exercise the generated helper in the same Windows PowerShell used by the app.
        // Mock registry access; only cmd.exe runs, so this never unloads real hives.
        var harness = """
            $ErrorActionPreference = 'Stop'
            $script:unloadCalls = 0
            function Test-Path { param($LiteralPath, $ErrorAction); return HIVE_EXISTS }
            function reg.exe {
                param($operation, $key)
                $script:unloadCalls++
                if ($operation -ne 'unload' -or $key -ne 'HKLM\OFFLINE_SOFTWARE') { throw 'Unexpected registry command' }
                & cmd.exe /d /c 'echo Simulated native stderr 1>&2 & exit /b NATIVE_EXIT'
                $global:LASTEXITCODE = $LASTEXITCODE
            }
            HELPER
            $caught = $null
            try { Clear-InitialRegistryHive 'HKLM\OFFLINE_SOFTWARE' } catch { $caught = $_ }
            if ($ErrorActionPreference -ne 'Stop') { throw 'Caller error preference changed' }
            if ($script:unloadCalls -ne EXPECTED_CALLS) { throw 'Unexpected unload count' }
            if (EXPECT_FAILURE) {
                if (!$caught -or $caught.Exception.Message -notlike '*Initial registry cleanup failed*exit code 1*Simulated native stderr*') {
                    throw "Missing actionable native failure: $caught"
                }
            } elseif ($caught) { throw $caught }
            """
            .Replace("HIVE_EXISTS", hiveExists ? "$true" : "$false")
            .Replace("NATIVE_EXIT", exitCode.ToString())
            .Replace("EXPECTED_CALLS", hiveExists ? "1" : "0")
            .Replace("EXPECT_FAILURE", hiveExists && exitCode != 0 ? "$true" : "$false")
            .Replace("HELPER", helper);
        var startInfo = new ProcessStartInfo("powershell.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-EncodedCommand");
        startInfo.ArgumentList.Add(Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(harness)));
        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }
        Assert.True(process.ExitCode == 0, $"{await stdout}\n{await stderr}");
    }

    [Fact]
    public void UserPaths_EscapePowerShellSingleQuotes()
    {
        var script = GenerateScript(
            @"C:\input\user's.iso",
            @"C:\scratch\builder's",
            @"C:\output\user's tiny.iso");

        Assert.Contains("$isoPath = 'C:\\input\\user''s.iso'", script);
        Assert.Contains("$scratchPath = 'C:\\scratch\\builder''s'", script);
        Assert.Contains("$outputPath = 'C:\\output\\user''s tiny.iso'", script);
    }

    [Fact]
    public void OptionalNativeOperations_AreCheckedAndNotSilentlyIgnored()
    {
        var service = new PowerShellService(new LocalizationService());
        var options = new ComponentRemovalOptions
        {
            RemoveHyperV = true,
            RemoveRecall = true,
            RemoveInputComponents = true,
            CleanupDriverStore = true,
            CleanupComponentStore = true,
            CompressFinalImage = true
        };
        var script = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, options);

        Assert.Contains("Assert-NativeSuccess 'Hyper-V removal'", script);
        Assert.Contains("Assert-NativeSuccess \"Capability removal: $name\"", script);
        Assert.Contains("Assert-NativeSuccess \"Driver removal: $pubName\"", script);
        Assert.Contains("Assert-NativeSuccess 'Component store cleanup'", script);
        Assert.Contains("Assert-NativeSuccess 'Final WIM compression'", script);
        Assert.DoesNotContain("try { Remove-Matching", script);
    }

    [Fact]
    public void BaseTinyPackageRemoval_RemainsEnabledWithoutOptionalApplicationChoices()
    {
        var packages = BareOptions().GetPackagesToRemove();

        Assert.Contains("Microsoft.BingNews", packages);
        Assert.Contains("Microsoft.WindowsFeedbackHub", packages);
        Assert.DoesNotContain(packages, package => package.Contains("Teams", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TeamsAndChat_IsOneFunctionalPackageFamilyChoice()
    {
        var options = BareOptions();
        options.RemoveTeams = true;

        var packages = options.GetPackagesToRemove();

        Assert.Contains("MicrosoftTeams", packages);
        Assert.Contains("MSTeams", packages);
        Assert.Contains("Microsoft.Windows.Teams", packages);
    }

    [Fact]
    public void CoreBuild_ProducesAdditionalDestructiveOperations()
    {
        var service = new PowerShellService(new LocalizationService());
        var standard = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, BareOptions());
        var core = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, BareOptions(), isCoreBuild: true);

        Assert.Contains("# Tiny11 Builder - Standard Script", standard);
        Assert.DoesNotContain("CORE package inventory", standard);
        Assert.Contains("# Tiny11 Builder - CORE Script", core);
        Assert.Contains("CORE package inventory", core);
        Assert.Contains("Windows\\System32\\Recovery\\winre.wim", core);
        Assert.Contains("ControlSet001\\Services\\wuauserv", core);
        Assert.Contains("WinDefend", core);
    }

    [Fact]
    public void OobeCheckboxes_EmitSupportedUnattendSettingsIndependently()
    {
        var service = new PowerShellService(new LocalizationService());
        var options = BareOptions();
        options.SkipPrivacyQuestions = true;
        var privacy = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, options);
        options.SkipPrivacyQuestions = false;
        options.SkipNetworkConnection = true;
        var network = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, options);
        options.SkipNetworkConnection = false;
        options.BypassMSAccount = true;
        var account = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, options);

        Assert.Contains("Set-OobeValue 'ProtectYourPC' '3'", privacy);
        Assert.DoesNotContain("HideWirelessSetupInOOBE", privacy);
        Assert.Contains("Set-OobeValue 'HideWirelessSetupInOOBE' 'true'", network);
        Assert.DoesNotContain("HideOnlineAccountScreens", network);
        Assert.Contains("Set-OobeValue 'HideOnlineAccountScreens' 'true'", account);
        Assert.DoesNotContain("bypassnro.cmd", account, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LocalizedDismInventoriesAndRegistryWrites_AreChecked()
    {
        var options = BareOptions();
        options.RemoveRecall = true;
        options.CleanupDriverStore = true;
        options.BypassTPM = true;
        var service = new PowerShellService(new LocalizationService());
        var script = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, options);

        Assert.Contains("/English /Image:$mountDir /Get-Capabilities", script);
        Assert.Contains("/English /Image:$mountDir /Get-Drivers", script);
        Assert.Contains("Assert-NativeSuccess \"Registry write: $key\\$name\"", script);
        Assert.Contains("Assert-NativeSuccess \"AppX removal: $match\"", script);
        Assert.DoesNotContain("DisableAntiSpyware", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Remove-AppxProvisionedPackage", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EveryBooleanOption_ChangesTheGeneratedBuildScript()
    {
        var service = new PowerShellService(new LocalizationService());
        var baseline = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, BareOptions());
        var booleanOptions = typeof(ComponentRemovalOptions).GetProperties().Where(property => property.PropertyType == typeof(bool));

        foreach (var property in booleanOptions)
        {
            var options = BareOptions();
            property.SetValue(options, true);
            var script = service.PreviewScript(@"C:\images\windows.iso", @"C:\scratch", @"C:\output\tiny11.iso", 3, options);

            Assert.NotEqual(baseline, script);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void GeneratedScripts_HaveValidPowerShellSyntax(bool isCoreBuild, bool useCustomUnattend)
    {
        var options = new ComponentRemovalOptions
        {
            RemoveXbox = true,
            DisableWindowsUpdate = true,
            RemoveHyperV = true,
            RemoveInputComponents = true,
            CleanupDriverStore = true,
            CustomAutounattendPath = useCustomUnattend ? @"C:\answers\custom user's autounattend.xml" : null
        };
        var service = new PowerShellService(new LocalizationService());
        var script = service.PreviewScript(
            @"C:\images\windows.iso",
            @"C:\scratch",
            @"C:\output\tiny11.iso",
            3,
            options,
            isCoreBuild);
        var scriptPath = Path.Combine(Path.GetTempPath(), $"tiny11-script-test-{Guid.NewGuid():N}.ps1");

        try
        {
            File.WriteAllText(scriptPath, script);
            var startInfo = new ProcessStartInfo("powershell.exe")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add("$tokens=$null; $errors=$null; [System.Management.Automation.Language.Parser]::ParseFile($env:TINY11_SCRIPT_TEST_PATH, [ref]$tokens, [ref]$errors) | Out-Null; if ($errors.Count) { $errors | ForEach-Object { [Console]::Error.WriteLine($_.Message) }; exit 1 }");
            startInfo.Environment["TINY11_SCRIPT_TEST_PATH"] = scriptPath;

            using var process = Process.Start(startInfo)!;
            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.True(process.ExitCode == 0, $"PowerShell parser rejected the generated script.\n{standardOutput}\n{standardError}");
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }
}
