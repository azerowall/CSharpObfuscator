using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSObfuscator
{
    class IdentifierGenerator
    {
        uint counter = 0;
        Random rand = new Random(DateTime.Now.Millisecond);
        
        public string NewId()
        {
            string[] formats = { "_{0}", "O{0}", "I{0}",
                "Ox{0:X}", "Ox{0:x}" };

            counter++;

            return String.Format(formats[rand.Next(formats.Length)],
                rand.Next(2) == 0 ? counter.ToString().PadLeft(10, '0') : counter.ToString());
        }
    }
}
