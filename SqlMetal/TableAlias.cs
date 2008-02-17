using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SqlMetal
{
    public class TableAlias
    {
        public class Renamings
        {
            [XmlElement("Renaming")]
            public readonly List<Renaming> Arr = new List<Renaming>();
        }

        public class Renaming
        {
            [XmlAttribute]
            public string old;
            [XmlAttribute]
            public string @new;
        }

        public static IDictionary<string, string> Load(string fileName, mmConfig mmConfig)
        {
            if (!System.IO.File.Exists(fileName))
                throw new ArgumentException("Renames file missing:" + mmConfig.renamesFile);

            Console.WriteLine("Loading renames file: " + fileName);

            XmlSerializer renamingsXmlSerializer = new XmlSerializer(typeof(Renamings));
            Renamings renamings = (Renamings)renamingsXmlSerializer.Deserialize(System.IO.File.OpenText(mmConfig.renamesFile));

            Dictionary<string, string> aliases = new Dictionary<string, string>();
            foreach (Renaming renaming in renamings.Arr)
            {
                aliases[renaming.old] = renaming.@new;
            }
            return aliases;
        }

        public static IDictionary<string, string> Load(mmConfig mmConfig)
        {
            if (mmConfig.renamesFile == null)
                return new Dictionary<string, string>();
            return Load(mmConfig.renamesFile, mmConfig);
        }
    }
}
