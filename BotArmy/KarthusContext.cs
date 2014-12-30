using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace najsvan
{
    public class KarthusContext : Context
    {
        public KarthusContext()
        {
            levelSpellsOrder = new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E };
        }
    }
}
