using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace najsvan
{
    public abstract class TreeProcessor
    {
        private String currentMethod;
        private readonly Dictionary<String, int[]> methodCallTimestamps = new Dictionary<String, int[]>();
        private readonly Dictionary<String, MethodInfo> reflectionCache = new Dictionary<String, MethodInfo>();

        public Dictionary<String, int[]> GetMethodCallTimestamps()
        {
            return methodCallTimestamps;
        }

        public Dictionary<String, MethodInfo> GetReflectionCache()
        {
            return reflectionCache;
        }

        public void SetCurrentMethod(String currentMethod)
        {
            this.currentMethod = currentMethod;
        }

        public String GetCurrentMethod()
        {
            return currentMethod;
        }
    }
}
