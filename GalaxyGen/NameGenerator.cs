using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public abstract class NameGenerator
    {
        public abstract string Generate(Rand64 random);
    }
}