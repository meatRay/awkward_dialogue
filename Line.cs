using System.Linq;

namespace CA4
{
    class Line : Interest
    {
        public static Line FromString(string line)
        {
            Line n_line = new Line();
            int i = line.IndexOf("\"") + 1;
            int k = line.LastIndexOf("\"") - i;
            var n_txt = line.Substring(i, k);

            var ar_txt = n_txt.Split('[', ']');
            var mods = new Module[ar_txt.Length];
            for (int l = 0; l < ar_txt.Length; ++l)
            {
                if (ar_txt[l].Contains(':'))
                {
                    var parts = ar_txt[l].Split(':');
                    switch (parts[0].ToLower())
                    {
                        case "get":
                            mods[l] = new Module(Module.Do.GET, parts[1]);
                            break;
                        case "say":
                            mods[l] = new Module(Module.Do.SAY, parts[l]);
                            break;
                        default:
                            mods[l] = new Module(Module.Do.SAY, ar_txt[l]);
                            break;
                    }
                }
                else
                { mods[l] = new Module(Module.Do.SAY, ar_txt[l]); }
            }
            n_line.Output = mods;

            line = line.Remove(i - 1, k + 2);
            n_line.Tags = line.Split(',').Select(l => l.Trim()).ToArray();
            return n_line;
        }
        public Module[] Output;
        public Voice Author;
    }
}
