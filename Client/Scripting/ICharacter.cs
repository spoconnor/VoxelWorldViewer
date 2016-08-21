using System;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient.Scripting
{
  public interface ICharacter
  {
    IScriptHost Parent { set; }
    string Execute(int characterId);

    string Name { get; }
    Coords Coords { get; }
  }
}

