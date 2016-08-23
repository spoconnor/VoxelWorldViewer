using System.Xml;
using Sean.WorldClient.Hosts.World;
using Sean.Shared;

namespace Sean.WorldClient.GameObjects.Units
{
    internal abstract class Unit : GameObject
    {
        protected Unit(ref Coords coords) : base(ref coords)
        {
            
        }

        internal Unit(XmlNode xmlNode) : base(xmlNode)
        {
            
        }
    }
}