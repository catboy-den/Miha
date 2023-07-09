using System.Text;
using static ThisAssembly;

namespace Miha.Shared;

public static class Versioning
{
    public static string GetVersion()
    {
        var version = new StringBuilder();
        var customVersion = Environment.GetEnvironmentVariable("MIHA_CUSTOM_VERSION");

        if (!string.IsNullOrEmpty(customVersion))
        {
            version.Append(customVersion).Append(' ').Append(Git.Commit);
        }
        else
        {
            version.Append('v');
            version.Append(Git.BaseVersion.Major).Append('.').Append(Git.BaseVersion.Minor).Append('.').Append(Git.BaseVersion.Patch);
            version.Append('-').Append(Git.Commit);
        }

        return version.ToString();
    }
}
