using DefaultCpuSets;
using System.Runtime.InteropServices;

public class SampleApp
{
    [DllImport("kernel32.dll")]
    static extern int GetCurrentProcessorNumber();

    public static void Main()
    {
        ConfigureCpuSet.SetCpuSets();

        int threadCount = 10;
        List<Action> tasks = new List<Action>();
        for (int i = 0; i <= threadCount; i++)
        {
            tasks.Add(() => Console.WriteLine(GetCurrentProcessorNumber()));
        }

        ParallelOptions parallelOption = new ParallelOptions
        {
            MaxDegreeOfParallelism = threadCount
        };

        Parallel.Invoke(parallelOption, tasks.ToArray());
    }
}