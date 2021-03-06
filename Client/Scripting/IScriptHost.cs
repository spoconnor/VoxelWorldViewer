using System;
using Sean.WorldClient.Hosts.World;
using OpenTK;
using Sean.WorldClient.GameObjects.GameItems;
using Sean.WorldClient.GameActions;
using Sean.Shared;

namespace Sean.WorldClient.Scripting
{
  public interface IScriptHost
  {
     void AddBlock(int characterId, Position position, Block.BlockType blockType);
     void AddBlockItem(int characterId, Coords coords, Vector3 velocity, Block.BlockType blockType, int gameObjectId);
     void AddProjectile(int characterId, Coords coords, Vector3 velocity, Block.BlockType blockType, bool allowBounce, int gameObjectId);
     void AddStaticItem(int characterId, Coords coords, StaticItemType staticItemType, ushort subType, Face attachedToFace, int gameObjectId);
     //void AddStructure(int characterId, Position position, StructureType structureType, Facing frontFace);
     void ChatMsg(int characterId, string message);
     void PickupBlockItem(int characterId, int gameObjectId);
     void CharacterMove(int characterId, Coords coords);
     void RemoveBlock(int characterId, Position position);
     void RemoveBlockItem(int characterId, int gameObjectId, bool isDecayed);
  }
}

