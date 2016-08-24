using System;
using Sean.WorldClient.Hosts.World;
using Sean.Shared;

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

