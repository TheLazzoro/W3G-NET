using W3GNET;

[assembly: Parallelize(Workers = 8, Scope = ExecutionScope.ClassLevel)]
namespace Tests
{
    [TestClass]
    public class ReplayTests
    {
        static string replaysFolder = Path.Combine(Directory.GetCurrentDirectory(), "Replays");

        public static IEnumerable<object[]> GetReplaysFromTestFolder
        {
            get
            {
                string[] replays = Directory.GetFileSystemEntries("Replays\\", "*.w3g", SearchOption.AllDirectories);
                var objects = new List<object[]>();
                for (int i = 0; i < replays.Length; i++)
                {
                    yield return new[] { Path.Combine(Directory.GetCurrentDirectory(), replays[i]) };
                }
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(GetReplaysFromTestFolder), typeof(ReplayTests), DynamicDataSourceType.Property)]
        public async Task ReplayTestsAll(string replayPath)
        {
            using (var s = new FileStream(replayPath, FileMode.Open))
            {
                W3GReplay replay = new W3GReplay(true);
                await replay.Parse(s);
            }
        }
    }
}