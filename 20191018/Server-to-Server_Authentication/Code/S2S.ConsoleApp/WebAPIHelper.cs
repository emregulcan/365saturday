using System;
using System.Text.RegularExpressions;

namespace S2S.ConsoleApp
{
    public static class WebAPIHelper
    {
        public static Guid GetRecordId(string uri)
        {
            Guid result = Guid.Empty;

            Regex regex = new Regex(@"\(([^)]+)\)");
            var id = regex.Match(uri).Value.Replace("(", "").Replace(")", "");

            Guid.TryParse(id, out result);

            return result;
        }
    }
}
