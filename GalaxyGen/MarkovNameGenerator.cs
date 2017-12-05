using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GalaxyGen
{
    public class MarkovNameGenerator : NameGenerator
    {
        Node rootNode = new Node("");

        const int MaxLinkSize = 3;
        const int MinLength = 4;
        const int MaxLength = 12;
        const bool excludeBaseNames = false;

        List<string> baseNames = new List<string>();

        public MarkovNameGenerator()
        {
        }

        public MarkovNameGenerator(string inputFilePath)
        {
            Train(inputFilePath);
        }

        public class Node
        {
            public Node(string key) { this.key = key; }
            public string key;
            public Dictionary<string, Node> nodes = new Dictionary<string, Node>();
            public int frequency = 1;

            public Node PickRandomChild(Rand64 random)
            {
                int childFrequency = 0;
                foreach (Node child in nodes.Values)
                {
                    childFrequency += child.frequency;
                }

                int pick = random.Range(0, childFrequency);

                foreach (Node child in nodes.Values)
                {
                    pick -= child.frequency;
                    if (pick <= 0)
                        return child;
                }
                return null;
            }
        }

        void AddLink(string key, string next)
        {
            Node keyNode = null;

            if (rootNode.nodes.ContainsKey(key))
            {
                keyNode = rootNode.nodes[key];
            }
            else
            {
                keyNode = new Node(key);
                rootNode.nodes.Add(key, keyNode);
            }

            if (keyNode.nodes.ContainsKey(next))
            {
                keyNode.nodes[next].frequency++;
            }
            else
            {
                keyNode.nodes.Add(next, new Node(next));
            }
        }

        public void AddName(string name)
        {
            //name = "[" + name.ToLower() + "]";
            name = "[" + name + "]";

            /*for(int n = 0; n < name.Length - 1; n++)
            {
                AddLink(name[n].ToString(), name[n + 1].ToString());
                if(n > 0)
                {
                    AddLink(name.Substring(n - 1, 2), name[n + 1].ToString());
                }
            }*/

            for (int n = 0; n < name.Length; n++)
            {
                for (int l = 1; l <= MaxLinkSize; l++)
                {
                    if (n >= l)
                    {
                        AddLink(name.Substring(n - l, l), name[n].ToString());
                    }
                }
            }

            if(excludeBaseNames)
            {
                baseNames.Add(name);
            }
        }

        public override string Generate(Rand64 random)
        {
            int attempts = 100;

            while (attempts > 0)
            {
                attempts--;
                string name;

                if (AttemptGenerate(random, out name))
                {
                    return name;
                }
            }
            return "";
        }

        bool AttemptGenerate(Rand64 random, out string outName)
        {
            string result = "[";
            Node node = null;

            for (int n = 0; n < 100; n++)
            {
                bool found = false;

                float linkSizeRoll = random.Range(0.0f, 1.0f);
                int linkSize = linkSizeRoll < 0.05f ? 1 : linkSizeRoll < 0.15f ? 3 : 2;

                for (int l = linkSize; l > 0 && !found; l--)
                {
                    if (result.Length >= l)
                    {
                        string sub = result.Substring(result.Length - l, l);

                        if (rootNode.nodes.TryGetValue(sub, out node))
                        {
                            node = node.PickRandomChild(random);

                            if (node != null)
                            {
                                if (result.Length + node.key.Length >= (2 + MinLength) || !node.key.Contains("]"))
                                {
                                    if (l > 1 || node.key != "]")
                                    {
                                        result += node.key;
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if(!found)
                {
                    outName = null;
                    return false;
                }

                if (result.EndsWith("]"))
                    break;
            }

            if (excludeBaseNames && baseNames.Contains(result))
            {
                outName = null;
                return false;
            }

            result = result.Trim('[', ']');

            outName = result;

            if (outName.Length > MaxLength || outName.Length < MinLength)
                return false;

            outName = char.ToUpper(outName[0]).ToString() + outName.Substring(1);

            return true;
        }

        public void Train(string pathName)
        {
            string[] sampleNames = File.ReadAllLines(pathName);

            foreach (string name in sampleNames)
            {
                AddName(name);
            }
        }
    }
}
