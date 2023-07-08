using System.Text;
using static ThisAssembly;

namespace Miha.Shared;

public static class Versioning
{
    public static string GetVersion()
    {
        var version = new StringBuilder();
        var versionPrefix = Environment.GetEnvironmentVariable("MIHA_VERSION_PREFIX");

        if (!string.IsNullOrEmpty(versionPrefix))
        {
            version.Append(versionPrefix).Append(' ').Append(Git.Commit);
        }
        else
        {
            version.Append(Git.BaseVersion.Major).Append('.').Append(Git.BaseVersion.Minor).Append('.').Append(Git.BaseVersion.Patch);
            version.Append(' ').Append(Git.Commit);
        }

        return version.ToString();
    }
}
