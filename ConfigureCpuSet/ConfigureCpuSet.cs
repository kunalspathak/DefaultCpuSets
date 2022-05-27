using Vanara.PInvoke;

namespace DefaultCpuSets
{
    public class ConfigureCpuSet
    {
        private static string cpusetFileName = "cpusetinfo.txt";
        /// <summary>
        /// Sets the CPU sets for current process.
        /// </summary>
        /// <returns>true, if it was successful in setting it.</returns>
        public static bool SetCpuSets()
        {
            try
            {
                string? appdataFolder = Environment.GetEnvironmentVariable("APPDATA");
                if (appdataFolder == null)
                {
                    Console.WriteLine("APPDATA environment variable not found.");
                    return false;
                }

                string cpusetInfoPath = Path.Combine(appdataFolder, cpusetFileName);

                if (!File.Exists(cpusetInfoPath))
                {
                    Console.WriteLine("{0} not found.", cpusetInfoPath);
                    return false;
                }

                string cpuSetInfo = File.ReadAllText(cpusetInfoPath);
                if (string.IsNullOrWhiteSpace(cpuSetInfo))
                {
                    Console.WriteLine("Empty contents in {0}.", cpusetInfoPath);
                    return false;
                }

                var cpuSet = new List<int>();
                var safeProcess = Kernel32.OpenProcess(ACCESS_MASK.MAXIMUM_ALLOWED, false, (uint)Environment.ProcessId);

                var ranges = cpuSetInfo.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var r in ranges)
                {
                    var bounds = r.Split('-', 2);

                    if (r.Length == 1)
                    {
                        cpuSet.Add(int.Parse(bounds[0]));
                    }
                    else
                    {
                        for (var i = int.Parse(bounds[0]); i <= int.Parse(bounds[1]); i++)
                        {
                            cpuSet.Add(i);
                        }
                    }
                }

                var ssi = Kernel32.GetSystemCpuSetInformation(safeProcess).ToArray();
                if (ssi.Length < cpuSet.Count)
                {
                    Console.WriteLine("Specified cores {0} is more than actual present {1}.", cpuSet.Count, ssi.Length);
                    return false;
                }

                var cpuSets = cpuSet.Select(i => ssi[i].CpuSet.Id).ToArray();
                bool result = Kernel32.SetProcessDefaultCpuSets(safeProcess, cpuSets, (uint)cpuSets.Length);

                if (result)
                {
                    Console.WriteLine("Configured CPU set to {0}", cpuSetInfo);
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}