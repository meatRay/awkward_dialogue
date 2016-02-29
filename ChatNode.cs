using System;
using System.Collections.Generic;
using System.Linq;

namespace CA4
{
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
}
