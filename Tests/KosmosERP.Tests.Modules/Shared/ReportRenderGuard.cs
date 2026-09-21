namespace KosmosERP.Tests.Modules.Shared;

/// <summary>
/// FastReport rendering needs libgdiplus (System.Drawing.Common) at runtime. This probes once so
/// render tests can skip cleanly on dev machines that lack it, while CI — which installs libgdiplus
/// and sets REQUIRE_REPORT_RENDER=1 — enforces them and catches GDI regressions.
/// </summary>
public static class ReportRenderGuard
{
    public static readonly bool GdiAvailable = Probe();

    private static bool Probe()
    {
        try
        {
            using var _ = new FastReport.Report();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Skips the calling test when libgdiplus is unavailable — unless CI requires it, in which case it fails.</summary>
    public static void Require()
    {
        if (GdiAvailable)
            return;

        if (Environment.GetEnvironmentVariable("REQUIRE_REPORT_RENDER") == "1")
            Assert.Fail("libgdiplus is unavailable but REQUIRE_REPORT_RENDER=1 (CI). Install libgdiplus so FastReport can render headlessly.");

        Assert.Ignore("Skipping report render test: libgdiplus is not installed. Install it (e.g. 'sudo apt-get install -y libgdiplus') to run FastReport rendering locally.");
    }
}
