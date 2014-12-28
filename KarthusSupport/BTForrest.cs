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
        private Dictionary<String, MethodInfo> reflectionCache = new Dictionary<String, MethodInfo>();
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

        public bool Process_Tree(String treeName, String stack)
        {
            Tree firstTree = JSONHelper.Deserialize<Tree>(LeagueSharp.Common.Config.LeagueSharpDirectory + "/bt/" + funcProcessor.GetType().Name + "/" + treeName + ".json");
            // expected to have one "Start" node
            List<Node> nodes = firstTree.nodes;
            Assert.True(nodes != null && nodes.Count == 1, "nodes != null && nodes.Count == 1");
            Node start = nodes[0];
            Assert.True(start != null, "start != null");

            if (start.children != null)
            {
                Assert.True(start.children.Count < 2, "start.children.Count must be 0 or 1");
                Node child = start.children[0];
                return ProcessGenericNode(child, stack + treeName);
            }
            else
            {
                return false;
            }
        }

        public bool Process_Sequence(Node node, String stack)
        {
            Assert.True(node.children != null, "node.children != null");
            if (node.children.Count > 0)
            {
                foreach (Node child in node.children)
                {
                    if (!ProcessGenericNode(child, stack + node.ToString()))
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

        public bool Process_Selector(Node node, String stack)
        {
            Assert.True(node.children != null, "node.children != null");
            if (node.children.Count > 0)
            {
                foreach (Node child in node.children)
                {
                    if (ProcessGenericNode(child, stack + node.ToString()))
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

        public bool Process_Decorator(Node node, String stack)
        {
            Assert.True(node.children != null && node.children.Count == 1, "node.children != null && node.children.Count == 1");
            return ProcessGenericNode(node, stack + node.ToString(), this, "Decorator_" + node.name);
        }

        public bool Decorator_Not(Node node, String stack)
        {
            return ! ProcessGenericNode(node, stack);
        }

        public bool Decorator_True(Node node, String stack)
        {
            ProcessGenericNode(node, stack);
            return true;
        }

        public bool Decorator_False(Node node, String stack)
        {
            ProcessGenericNode(node, stack);
            return false;
        }

        public bool Process_Action(Node node, String stack)
        {
            Assert.True(node.children == null || node.children.Count == 0, "node.children == null || node.children.Count == 0");
            return ProcessGenericNode(node, stack + node.ToString(), funcProcessor, "Action_" + node.name, node.func);
        }

        public bool Process_Condition(Node node, String stack)
        {
            Assert.True(node.children == null || node.children.Count == 0, "node.children == null || node.children.Count == 0");
            return ProcessGenericNode(node, stack + node.ToString(), funcProcessor, "Condition_" + node.name, node.func);
        }

        public bool Process_TreeLink(Node node, String stack)
        {
            Assert.True(node.children == null || node.children.Count == 0, "node.children == null || node.children.Count == 0");
            Assert.True(node.name != null && !"".Equals(node.name), "node.name != null && !\"\".Equals(node.name)");
            Process_Tree(node.name, stack);
            return true;
        }

        private bool ProcessGenericNode(Node node, String stack)
        {
            return ProcessGenericNode(node, stack, this, "Process_" + node.type);
        }

        private bool ProcessGenericNode(Node node, String stack, Object processor, String methodName, String func = null)
        {
            String simpleSignature = processor.GetHashCode() + methodName;
            MethodInfo method;
            if (!reflectionCache.TryGetValue(simpleSignature, out method))
            {
                Type type = processor.GetType();
                method = type.GetMethod(methodName);
                Assert.True(method != null, "GetMethod: null for : " + methodName + " in " + type.Name);
                reflectionCache.Add(simpleSignature, method);
            }

            object[] parameters = func == null ? new object[] { node, stack } : new object[] { node, func, stack };
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
