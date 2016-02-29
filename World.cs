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

            talkers.Enqueue(new Voice { Name = "Bao Dur", _root = ChatNode.FromString(File.ReadAllText("lines/lines_Bao-Dur.txt")) });
            talkers.Enqueue(new Voice { Name = "WHO", _root = ChatNode.FromString(File.ReadAllText("lines/lines_Who.txt")) });
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
                        return ln.Author.GetString(arpath[1]);
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
                Console.WriteLine(String.Format("{0}: {1}", response_name, response.Output.Select( l => l.Resolve() ).Aggregate( (a, b) => a + b ) ) );
                Dialogue.Push(response);

                // Oh god, Stop me, please.
                // Just re-queues the talkers in random order
                // I really don't need a Queue anymore, I should just access an enumerable in a random fashion
                talkers.Enqueue(talkers.Dequeue());
                /*talkers.Clear();
                var numbs = Enumerable.Range(0, ppl.Length).ToList();
                while (numbs.Any())
                {
                    int i = rand.Next(numbs.Count());
                    int ind = numbs.ElementAt(i);
                    numbs.RemoveAt(i);
                    talkers.Enqueue(ppl[ind]);
                }*/
            }
            Console.ReadLine();
        }
    }

    class Module
    {
        public static Func<string, string> GetString;
        public enum Do { NONE, SAY, GET };
        public Do Activity;
        public string Path;

        public Module(Do activity, string path)
        {
            Activity = activity;
            Path = path;
        }
        public string Resolve()
        {
            switch (Activity)
            {
                case Do.GET:
                    return GetString(Path);
                case Do.SAY:
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
