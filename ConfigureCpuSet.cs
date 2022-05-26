using Vanara.PInvoke;

namespace DefaultCpuSets
{
    public class ConfigureCpuSet
    {
        /// <summary>
        /// Sets the CPU sets for current process.
        /// </summary>
        /// <param name="arg">CPU numbers range like 0-27. Can be multiple with comma-separated like 0-4, 8-18</param>
        /// <returns></returns>
        public static bool SetCpuSets(string arg)
        {
            var cpuSet = new List<int>();
            var safeProcess = Kernel32.OpenProcess(ACCESS_MASK.MAXIMUM_ALLOWED, false, (uint)Environment.ProcessId);

            var ranges = arg.Split(',', StringSplitOptions.RemoveEmptyEntries);
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
                return false;
            }

            var cpuSets = cpuSet.Select(i => ssi[i].CpuSet.Id).ToArray();
            return Kernel32.SetProcessDefaultCpuSets(safeProcess, cpuSets, (uint)cpuSets.Length);
        }
    }
}