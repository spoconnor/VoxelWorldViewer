using System;
using System.Xml;
using Sean.WorldClient.Hosts.World;
using OpenTK;
using Sean.Shared;

namespace Sean.WorldClient.GameObjects.Units
{
    internal enum MobType
    {
        MobType1,
        MobType2
    }

    internal class Mob : Unit
    {
        internal Mob(Coords coords, MobType type) : base(ref coords)
        {
            Type = type;
            WorldData.Chunks[coords].Mobs.Add(this);
        }

        internal Mob(XmlNode xmlNode) : base(xmlNode)
        {
            if (xmlNode.Attributes == null) throw new Exception("Node attributes is null.");
            Type = (MobType)int.Parse(xmlNode.Attributes["T"].Value);
            WorldData.Chunks[Coords].Mobs.Add(this);
        }

        internal virtual MobType Type { get; private set; }

        internal virtual void Spawn()
        {
            
        }

        internal override string XmlElementName
        {
            get { return "M"; }
        }

        internal override XmlNode GetXml(XmlDocument xmlDocument)
        {
            var xmlNode = base.GetXml(xmlDocument);
            if (xmlNode.Attributes == null) throw new Exception("Node attributes is null.");
            xmlNode.Attributes.Append(xmlDocument.CreateAttribute("T")).Value = ((int)Type).ToString();
            return xmlNode;
        }

     internal override void Render(FrameEventArgs e)
     {
         base.Render(e); //sets light color, the color assigned will be the block the lower body is on
     }

    }
}
