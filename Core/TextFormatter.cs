namespace ProjectSMP.Core
{
    public static class TextFormatter
    {
        public static string Build(params string[] lines)
        {
            return string.Concat(lines);
        }

        public static string BuildWithNewLines(params string[] lines)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line);
                if (!line.EndsWith("\n"))
                    sb.Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }
    }

    public static class Msg
    {
        public const string Command = "{C6E2FF}COMMAND:{888888}";
        public const string Error = "{C6E2FF}ERROR:{FFFFFF}";
        public const string AdmCmd = "{FF6347}[AdmCmd]{FFFFFF}";
        public const string AdmCmd_G = "{FF6347}[AdmCmd]{888888}";
        public const string Bank = "{C6E2FF}BANK:{FFFFFF}";
        public const string Death = "{C6E2FF}DEATH:{FFFFFF}";
        public const string Hospital = "{F4C2C2}HOSPITAL:{FFFFFF}";
        public const string Settings = "{C6E2FF}SETTINGS:{FFFFFF}";
        public const string AB = "{ebe6ae}[AB]{FFFFFF}";
        public const string ADO = "{ebe6ae}[ADO]{FFFFFF}";
        public const string AME = "{ebe6ae}[AME]{D0AEEB}";
        public const string Sick = "{ffa500}SICK:{FFFFFF}";
        public const string Report = "{fff000}[REPORT]{FFFFFF}";
        public const string Report_G = "{fff000}[REPORT]{888888}";
        public const string Report_A = "{fff000}[REPORT][ANSWER]{FFFFFF}";
        public const string Report_R = "{fff000}[RESPOND]{fffccc}";
    }
}