using System;
using Unica.TaskSimulator;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Unica.TemporalExpressionSimulator;

namespace Sockets
{
    class Program
    {
        public static TcpSensor sensor = new TcpSensor(2);
        public static int index = 0;

        static void Main(string[] args)
        {
            String dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace("Sockets\\bin\\Debug\\netcoreapp2.1", "") + "hmst\\";
            List<string> sequence = new List<string>
            {
                "releaseBrake",
                "Release THR Levers",
                "Directional control",
                "Gain altitude",
                "Gear up",
                "Reach 80 knots N.P.",
                "FLAPS 1",
                "Check 100 knots",
                "FLAPS 0"
            };

            //acquisizione file xml
            while (index < sequence.Count)
            {
                String file = sequence[index];
                String fullPath = String.Format(dir + file + ".hmst");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(fullPath));
                XmlNode xml_node = doc.SelectNodes("hamsters").Item(0).SelectNodes("nodes").Item(0).SelectSingleNode("task").SelectNodes("operator").Item(0);
                xml_node = doc.SelectNodes("hamsters").Item(0).SelectNodes("nodes").Item(0).SelectSingleNode("task").SelectSingleNode("operator");

                //creo l'albero
                TaskTerm root = new TaskTerm("-1;-1", "ROOT");
                Boolean releasePatternBlocker = true;
                Boolean cognitiveNeeded = false;

                if (file.Equals("Release THR Levers"))
                    releasePatternBlocker = false;
                if (file.Equals("Reach 80 knots N.P."))
                    cognitiveNeeded = true;

                makeTree(root, xml_node, releasePatternBlocker, cognitiveNeeded);

                //mostra albero
                Console.WriteLine("\n---------------------------------------------------------\nSubroutine : " + file);
                showTree(root);

                //sensore
                sensor.waitInput(root);

                index++;
            }
           
            Console.ReadLine();
        }

        //deve corrispondere al getProperBranch di JsonGenerator
        private static XmlNodeList getProperBranch(XmlNode root, Boolean releasePatternBlocker, Boolean cognitiveNeeded)
        {
            XmlNodeList firstBranch = root.SelectNodes("task");

            int i = 0;
            foreach (XmlNode n in firstBranch)
            {
                //flag
                if (cognitiveNeeded)
                {
                    if (n.Attributes.GetNamedItem("type").Value.Equals("cognitive", StringComparison.CurrentCultureIgnoreCase) && n.Attributes.GetNamedItem("name").Value.Contains("Wait"))
                        return firstBranch;
                }

                if (!cognitiveNeeded)
                {
                    if ((n.Attributes.GetNamedItem("type").Value.Equals("input", StringComparison.CurrentCultureIgnoreCase) && (!n.Attributes.GetNamedItem("name").Value.Contains("Release") || !releasePatternBlocker))
                       || n.Attributes.GetNamedItem("type").Value.Equals("user", StringComparison.CurrentCultureIgnoreCase))
                        return firstBranch;
                }

                if (n.Attributes.GetNamedItem("type").Value.Equals("abstract", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlNodeList tmp = getProperBranch(n, releasePatternBlocker, cognitiveNeeded);
                    if (tmp != null)
                        if (tmp.Count > 0)
                            return tmp;
                }

            }

            //XmlNode secondBranch = root.SelectSingleNode("operator");
            XmlNodeList secondBranch = root.SelectNodes("operator");
            if (secondBranch == null)
                return null;
            foreach (XmlNode branch in secondBranch)
            {
                XmlNodeList toRet = getProperBranch(branch, releasePatternBlocker, cognitiveNeeded);
                if (toRet != null)
                    return toRet;
            }
            return null;
        }

        //private static XmlNodeList getProperBranch(XmlNode root, Boolean releasePatternBlocker)
        //{
        //    XmlNodeList firstBranch = root.SelectNodes("task");

        //    int i = 0;
        //    foreach (XmlNode n in firstBranch)
        //    {
        //        if ((n.Attributes.GetNamedItem("type").Value.Equals("input", StringComparison.CurrentCultureIgnoreCase) && (!n.Attributes.GetNamedItem("name").Value.Contains("Release") || !releasePatternBlocker))
        //           || n.Attributes.GetNamedItem("type").Value.Equals("user", StringComparison.CurrentCultureIgnoreCase))
        //            return firstBranch;
        //        if (n.Attributes.GetNamedItem("type").Value.Equals("abstract", StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            XmlNodeList tmp = getProperBranch(n, releasePatternBlocker);
        //            if (tmp != null)
        //                if (tmp.Count > 0)
        //                    return tmp;
        //        }
        //    }

        //    XmlNode secondBranch = root.SelectSingleNode("operator");
        //    if (secondBranch == null)
        //        return null;
        //    return getProperBranch(secondBranch, releasePatternBlocker);
        //}

        public static void showTree(TaskTerm tree)
        {
            if (tree.Id == null)
                tree.Id = "root";

            if (tree.Children == null) return;

            Console.WriteLine("(" + tree.Id.ToString() + ") [" + tree.Name.ToString() + "] has " + tree.Children.Count + " children: ");

            foreach (TaskTerm child in tree.Children)
                Console.WriteLine("\t - (" + child.Id.ToString() + ") [" + child.Name.ToString() +  "]  << " + child.kind + " >>");

            foreach (TaskTerm child in tree.Children)
                showTree(child);
        }

        public static void makeTree(TaskTerm  treeNode, XmlNode xmlNode, Boolean releasePatternBlocker, Boolean cognitiveNeeded)
        {
            List<XmlNode> tmp = new List<XmlNode>();
            foreach (XmlNode node in getProperBranch(xmlNode, releasePatternBlocker, cognitiveNeeded))
                tmp.Add(node);

            treeNode.kind = "operator: " + tmp[0].ParentNode.Attributes.GetNamedItem("type").Value.ToString();
            treeNode.Id = tmp[0].ParentNode.Attributes.GetNamedItem("x").Value.ToString() + ";" + tmp[0].Attributes.GetNamedItem("y").Value.ToString();

            //riordino
            List<KeyValuePair<int, int>> sortingTool = new List<KeyValuePair<int, int>>();
            int counter = 0;
            foreach (XmlNode node in tmp)
            {
                KeyValuePair<int, int> pair = new KeyValuePair<int, int>(counter, Convert.ToInt32(node.Attributes.GetNamedItem("x").Value.ToString()));
                sortingTool.Add(pair);
                counter++;
            }
            sortingTool.Sort(delegate (KeyValuePair<int, int> pair1,
                                KeyValuePair<int, int> pair2)
            {
                return pair1.Value.CompareTo(pair2.Value);
            });

            List<XmlNode> toSave = new List<XmlNode>();
            foreach (KeyValuePair<int, int> p in sortingTool)
                toSave.Add(tmp[p.Key]);

            //accresco l'albero
            foreach (XmlNode node in toSave)
            {
                String n = node.OuterXml.Split(" ")[0];

                if (n.Equals("<operator"))
                {
                    if(node.Attributes.GetNamedItem("type").Value.Equals("enable"))
                    {
                        String ID = node.Attributes.GetNamedItem("x").Value.ToString() + ";" + node.Attributes.GetNamedItem("y").Value.ToString();
                        TaskTerm child = new TaskTerm(ID, node.Attributes.GetNamedItem("name").Value.ToString());
                        child.kind = "operator: " + node.Attributes.GetNamedItem("type").Value;
                        treeNode.Children.Add(child);

                        makeTree(child, node, releasePatternBlocker, cognitiveNeeded);
                    }
                }
                else if(n.Equals("<task"))
                {
                    String str = node.Attributes.GetNamedItem("type").Value.ToString();
                    if ((str.Equals("input") && !cognitiveNeeded) || str.Equals("output") || (str.Equals("user") && !cognitiveNeeded) || (str.Equals("cognitive") && cognitiveNeeded) || (str.Equals("sight") && node.Attributes.GetNamedItem("name").Value.Contains("Good")))
                    {
                        String ID = node.Attributes.GetNamedItem("x").Value.ToString() + ";" + node.Attributes.GetNamedItem("y").Value.ToString();
                        TaskTerm leaf = new TaskTerm(ID, node.Attributes.GetNamedItem("name").Value.ToString());
                        leaf.kind = "task";
                        treeNode.Children.Add(leaf);
                    }
                }
            }
        }
    }
}
