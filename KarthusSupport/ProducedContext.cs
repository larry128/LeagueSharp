using System;
using System.Collections.Generic;

namespace najsvan
{
    public class ProducedContext
    {
        private readonly Dictionary<ProducedContextKey, KeyValuePair<Producer, Object[]>> context = new Dictionary<ProducedContextKey, KeyValuePair<Producer, Object[]>>();

        public delegate Object Producer();

        public Object Get(ProducedContextKey key)
        {
            KeyValuePair<Producer, Object[]> pair;
            if (context.TryGetValue(key, out pair))
            {
                if (pair.Value[0] == null)
                {
                    pair.Value[0] = pair.Key();
                }

                return pair.Value[0];
            }
            else
            {
                Assert.Fail(false, "Don't know how to produce " + key);
                return null;
            }
            
        }

        public void Clear()
        {
            foreach (KeyValuePair<Producer, Object[]> value in context.Values)
            {
                value.Value[0] = null;
            }
        }

        public void Set(ProducedContextKey key, Producer prod)
        {
            Assert.False(context.ContainsKey(key), "Trying to set producer for key " + key + " multiple times");
            context.Add(key, new KeyValuePair<Producer, Object[]>(prod, new Object[] {null}));
        }
    }
}
