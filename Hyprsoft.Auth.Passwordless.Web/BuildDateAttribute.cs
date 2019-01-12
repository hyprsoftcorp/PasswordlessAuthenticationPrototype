using System;
using System.Globalization;

namespace Hyprsoft.Auth.Passwordless.Web
{
    [AttributeUsage(AttributeTargets.Assembly)]
    internal class BuildDateAttribute : Attribute
    {
        public BuildDateAttribute(string value)
        {
            /* .csproj addition
             <ItemGroup>
                <AssemblyAttribute Include="[your namespace here].BuildDateAttribute">
                    <_Parameter1>$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</_Parameter1>
                </AssemblyAttribute>
            </ItemGroup>
            */
            DateTimeUtc = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        public DateTime DateTimeUtc { get; }
    }
}
