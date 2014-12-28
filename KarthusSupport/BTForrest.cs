using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Reflection;


namespace najsvan
{
    public class BTForrest
    {
        private Object funcProcessor;
        private String firstTreeName;

        public BTForrest(String firstTreeName, Object funcProcessor)
        {
            Assert.True(funcProcessor != null, "funcProcessor != null");
            this.funcProcessor = funcProcessor;
            this.firstTreeName = firstTreeName;
        }

        public void Tick()
        {
            Process_Tree(firstTreeName, "/");
        }

        private bool Process_Tree(String treeName, String stack)
        {
            Tree firstTree = JSONHelper.Deserialize<Tree>(LeagueSharp.Common.Config.LeagueSharpDirectory + treeName + ".json");
            // expected to have one "Start" node
            List<Node> nodes = firstTree.nodes;
            Assert.True(nodes != null && nodes.Count == 1, "nodes != null && nodes.Count == 1");
            Node start = nodes[0];
            Assert.True(start != null, "start != null");

            if (start.children.Count == 1)
            {
                Node child = start.children[0];
                return ProcessAbstractNode(child, stack + treeName);
            }
            else
            {
                return false;
            }
        }

        private bool Process_Sequence(Node node, String stack)
        {
            Assert.True(node.children != null, "node.children != null");
            if (node.children.Count > 0)
            {
                foreach (Node child in node.children)
                {
                    if (!ProcessAbstractNode(child, stack + node.ToString()))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Process_Selector(Node node, String stack)
        {
            Assert.True(node.children != null, "node.children != null");
            if (node.children.Count > 0)
            {
                foreach (Node child in node.children)
                {
                    if (ProcessAbstractNode(child, stack + node.ToString()))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        private bool Process_Action(Node node, String stack)
        {
            Assert.True(node.children == null || node.children.Count == 0, "node.children == null || node.children.Count == 0");
            return ProcessAbstractNode(node, stack + node.ToString(), funcProcessor, "Action_", node.func);
        }

        private bool Process_Condition(Node node, String stack)
        {
            Assert.True(node.children == null || node.children.Count == 0, "node.children == null || node.children.Count == 0");
            return ProcessAbstractNode(node, stack + node.ToString(), funcProcessor, "Condition_", node.func);
        }

        private bool Process_TreeLink(Node node, String stack)
        {
            Assert.True(node.children == null || node.children.Count == 0, "node.children == null || node.children.Count == 0");
            Assert.True(node.name == null && !"".Equals(node.name), "node.name == null && !\"\".Equals(node.name)");
            return Process_Tree(node.name, stack);
        }

        private bool ProcessAbstractNode(Node node, String stack)
        {
            return ProcessAbstractNode(node, stack, this, "Process_");
        }

        private bool ProcessAbstractNode(Node node, String stack, Object processor, String prefix, String func = null)
        {
            Type type = processor.GetType();
            MethodInfo method = type.GetMethod(prefix + node.type);
            object[] parameters = func == null ? new object[] {node, stack} : new object[] {node, func, stack};
            return (bool)method.Invoke(processor, parameters);
        }
    }

    [DataContract]
    class Tree
    {
        [DataMember]
        public List<Node> nodes { get; protected set; }
    }

    [DataContract]
    public class Node
    {
        [DataMember]
        public List<Node> children { get; protected set; }

        [DataMember]
        public String func { get; protected set; }

        [DataMember]
        public String name { get; protected set; }

        [DataMember]
        public String type { get; protected set; }

        public override String ToString()
        {
            return "/" + name + "[" + type + "]";
        }
    }

    class JSONHelper
    {
        public static T Deserialize<T>(String jsonPath)
        {
            String json = File.ReadAllText(jsonPath);
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                ms.Close();
                ms.Dispose();
                return obj;
            }
        }
    }
}
