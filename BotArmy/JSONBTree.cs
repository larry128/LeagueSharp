using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using LeagueSharp.Common;

namespace najsvan
{
    public class JSONBTree
    {
        private static readonly Logger LOG = Logger.GetLogger("JSONBTree");
        private static readonly Statistics STAT = Statistics.GetStatistics("JSONBTree");
        private readonly Object funcProcessor;
        private readonly Dictionary<String, MethodInfo> reflectionCache = new Dictionary<String, MethodInfo>();
        private readonly Tree tree;
        private readonly String treeName;

        public JSONBTree(Object funcProcessor, String treeName)
        {
            Assert.True(funcProcessor != null, "funcProcessor != null");
            this.funcProcessor = funcProcessor;
            this.treeName = treeName;
            tree = JSONHelper.Deserialize<Tree>(Config.LeagueSharpDirectory + "/bt/" + treeName + ".json");
            Assert.True(tree != null, "JSONHelper.Deserialize<Tree>: null for : " + treeName);
        }

        public bool Tick(String stack = "")
        {
            LOG.Debug(treeName + " Tick()");

            // expected to have one "Start" node
            var nodes = tree.nodes;
            Assert.True(nodes != null && nodes.Count == 1, "nodes != null && nodes.Count == 1");
            var start = nodes[0];
            Assert.True(start != null, "start != null");

            if (start.children != null)
            {
                Assert.True(start.children.Count < 2, "start.children.Count must be 0 or 1");
                var child = start.children[0];
                return ProcessGenericNode(child, stack + treeName);
            }
            return false;
        }

        public bool Process_Sequence(Node node, String stack)
        {
            Assert.True(node.children != null, "node.children != null");
            if (node.children.Count > 0)
            {
                foreach (var child in node.children)
                {
                    if (!ProcessGenericNode(child, stack + node))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool Process_Selector(Node node, String stack)
        {
            Assert.True(node.children != null, "node.children != null");
            if (node.children.Count > 0)
            {
                foreach (var child in node.children)
                {
                    if (ProcessGenericNode(child, stack + node))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public bool Process_Decorator(Node node, String stack)
        {
            Assert.True(node.children != null && node.children.Count == 1,
                "node.children != null && node.children.Count == 1");
            return ProcessGenericNode(node.children[0], stack + node, this, "Decorator_" + node.name);
        }

        public bool Decorator_Not(Node node, String stack)
        {
            return !ProcessGenericNode(node, stack);
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
            return ProcessFunc(node, stack, "Action_");
        }

        public bool Process_Condition(Node node, String stack)
        {
            return ProcessFunc(node, stack, "Condition_");
        }

        private bool ProcessFunc(Node node, string stack, String prefix)
        {
            Assert.True(node.children == null || node.children.Count == 0,
                "node.children == null || node.children.Count == 0");
            var methodName = prefix + node.name;
            var result = ProcessGenericNode(node, stack + node, funcProcessor, methodName);
            STAT.Increment(treeName + "." + methodName);
            LOG.Debug(stack + node + " : " + result);
            return result;
        }

        private bool ProcessGenericNode(Node node, String stack)
        {
            return ProcessGenericNode(node, stack, this, "Process_" + node.type);
        }

        private bool ProcessGenericNode(Node node, String stack, Object processor, String methodName)
        {
            var simpleSignature = processor.GetHashCode() + methodName;
            MethodInfo method;
            if (!reflectionCache.TryGetValue(simpleSignature, out method))
            {
                var type = processor.GetType();
                method = type.GetRuntimeMethod(methodName, new[] {node.GetType(), stack.GetType()});
                Assert.True(method != null, "GetMethod: null for : " + methodName + " in " + type.Name);
                reflectionCache.Add(simpleSignature, method);
            }

            var invokeResult = method.Invoke(processor, new object[] {node, stack});
            bool methodResult;

            if (invokeResult != null && Boolean.TryParse(invokeResult.ToString(), out methodResult))
            {
                return methodResult;
            }
            return true;
        }
    }

    [DataContract]
    internal class Tree
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

        [DataMember]
        public String sim { get; protected set; }

        public override String ToString()
        {
            return "/" + name + "[" + type + "]";
        }
    }

    internal class JSONHelper
    {
        public static T Deserialize<T>(String jsonPath)
        {
            var json = File.ReadAllText(jsonPath);
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T) serializer.ReadObject(ms);
                ms.Close();
                ms.Dispose();
                return obj;
            }
        }
    }
}