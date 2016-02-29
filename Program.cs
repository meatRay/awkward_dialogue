using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CA4
{
    class World
    {
        /* Tolerance vaguely imposes a limit on how off-topic a conversation can go.
         * 0 - Speaker must have a line about the specific subject 
         * 1 - Speaker can talk with a parent of the specific subject
         * 2 - No deep enough trees yet for this to matter */
        const int TOLERANCE = 0;
        static void Main(string[] args)
        {
            Stack<Interest> Dialogue = new Stack<Interest>();
            Queue<Voice> talkers = new Queue<Voice>();

            talkers.Enqueue(new Voice { Name = "Kreia", _root = ChatNode.FromString(File.ReadAllText("lines/lines_Kreia.txt")) });
            talkers.Enqueue(new Voice { Name = "Exile", _root = ChatNode.FromString(File.ReadAllText("lines/lines_Exile.txt")) });
            talkers.Enqueue(new Voice { Name = "Bao Dur", _root = ChatNode.FromString(File.ReadAllText("lines/lines_Bao-Dur.txt")) });
            Voice[] ppl = talkers.ToArray();

            Random rand = new Random();

            /* Add a point of Interest.  These could be raised by finding items, doing quests, ect. */
            Dialogue.Push(new Interest { Tags = new string[1] { "Greeting" } });


            Module.GetString = (path) =>
            {
                var arpath = path.ToLower().Split('/');
                if (arpath[0] == "last")
                {
                    var ln = Dialogue.Peek() as Line;
                    if (ln != null)
                    {
                        return ln.Author.GetString(path);
                    }
                }
                return "";
            };
            string[] current_topic = null;
            string response_name = null;
            Line response = null;
            while (
                //TODO -- Find the Person with the highest level of interest.
                talkers.Any(talker =>
                   talker.HasInterest(current_topic = Dialogue.Peek().Tags, TOLERANCE)
                   && !Dialogue.Contains(response = talker._root.VoiceInterest(current_topic))
                   && (response_name = talker.Name) != null /*OH MY GOD YOU LAZY GIT*/
                   && (response.Author = talker) != null
                )
            )
            {
                Console.WriteLine(String.Format("{0}: {1}", response_name, response.Output));
                Dialogue.Push(response);

                // Oh god, Stop me, please.
                // Just re-queues the talkers in random order
                // I really don't need a Queue anymore, I should just access an enumerable in a random fashion
                talkers.Clear();
                var numbs = Enumerable.Range(0, ppl.Length).ToList();
                while (numbs.Any())
                {
                    int i = rand.Next(numbs.Count());
                    int ind = numbs.ElementAt(i);
                    numbs.RemoveAt(i);
                    talkers.Enqueue(ppl[ind]);
                }
            }
            Console.ReadLine();
        }
    }

    class Line : Interest
    {
        public static Line FromString(string line)
        {
            Line n_line = new Line();
            int i = line.IndexOf("\"") + 1;
            int k = line.LastIndexOf("\"") - i;
            var n_txt = line.Substring(i, k);
            n_line.Output = n_txt;
            line = line.Remove(i - 1, k + 2);
            n_line.Tags = line.Split(',').Select(l => l.Trim()).ToArray();
            return n_line;
        }
        public string Output;
        public Voice Author;
    }
    class Module
    {
        public static Func<string, string> GetString;
        public enum Do { NONE, TXT, GET };
        public Do Activity;
        public string Path;
        public string Resolve()
        {
            switch (Activity)
            {
                case Do.GET:
                    return GetString(Path);
                case Do.TXT:
                    return Path;
                default:
                    return "";
            }
        }
    }
    class Interest
    {
        //Define a common 'Library' of tags to keep standard?  Enums?
        public string[] Tags;
    }
    class ChatNode
    {
        public static ChatNode FromString(string input)
        { string ded; return FromString(input, out ded); }
        public static ChatNode FromString(string input, out string remaining_input)
        {
            var node = new ChatNode();
            int i = input.IndexOf(':');
            string tag = input.Substring(0, i++).Trim();
            input = input.Substring(i, input.Length - i);

            node.Tag = tag;

            i = input.IndexOf('(') + 1;
            int k = input.IndexOf(')') - i;
            string line = input.Substring(i, k);
            input = input.Remove(i - 1, k + 2);

            node.Comment = Line.FromString(line);
            List<ChatNode> children = new List<ChatNode>();
            while ((input = input.Trim())[0] != ';')
            {
                string rem_input;
                children.Add(FromString(input, out rem_input));
                input = rem_input;
            }
            node.Children = children.ToArray();
            remaining_input = new String(input.Skip(1).ToArray());
            return node;
        }

        /* TODO -- 'Modifiers'.
         * Modules that site in a Node, and alter the Comment based upon input */

        public string Tag;
        public Line Comment;
        public ChatNode[] Children;
        public int InterestLevel(IEnumerable<string> tags)
        {
            if (tags.Any())
            {
                string tag = tags.First();
                var fnd = Children.Where(n => tag.Contains(n.Tag));
                if (fnd.Any())
                { return fnd.Max(c => c.InterestLevel(tags.Skip(1)) + 1); }
            }
            return 0;
        }

        /*Find multiple max values*/
        static IEnumerable<ChatNode> OfMostInterest(IEnumerable<ChatNode> nodes, IEnumerable<string> tags)
        {
            if (!nodes.Any()) { return nodes; }
            if (nodes.Count() == 1) { return nodes; }
            List<ChatNode> maxums = new List<ChatNode> { nodes.First() };
            int at_level = nodes.First().InterestLevel(tags);
            foreach (var node in nodes.Skip(1))
            {
                int int_level = node.InterestLevel(tags);
                if (int_level > at_level)
                {
                    at_level = int_level;
                    maxums.Clear();
                    maxums.Add(node);
                }
                else if (int_level == at_level)
                { maxums.Add(node); }
            }
            return maxums;
        }
        static Random Make_Something_Cheaper = new Random();
        public Line VoiceInterest(IEnumerable<string> tags)
        {
            if (tags.Any())
            {
                string tag = tags.First();
                /*Make a Tag class ASAP!!*/

                //TODO -- Find most fitting line that isn't already mentioned!  Right now we're 'missing out' on dialogue.
                var fnd = OfMostInterest(Children.Where(n => tag.Contains(n.Tag)), tags);
                if (fnd.Any())
                { return fnd.ElementAt(Make_Something_Cheaper.Next(fnd.Count())).VoiceInterest(tags.Skip(1)); }
            }
            return Comment;
        }
    }
    class Voice
    {
        public string GetString(string path)
        {
            switch (path)
            {
                case "name":
                    return Name;
                default:
                    return "";
            }
        }
        public string Name;
        public ChatNode _root;
        //Discussion tree?
        public bool HasInterest(string[] tags, int tolerance)
        {
            return tags.Length - _root.InterestLevel(tags) <= tolerance;
        }
        //HasInterest must be "got attention" -- Voices can have comments for things they wouldn't immediately open to.
        public Line VoiceInterest(Interest interest)
        {
            var ln = this._root.VoiceInterest(interest.Tags);
            return ln;
            //ln.Output.
        }
        //Hear Interest
        //Have Interest?
        //Voice Interest
    }
}
