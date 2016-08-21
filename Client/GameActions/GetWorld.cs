using System;
using System.IO;
using System.IO.Compression;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient.GameActions
{
    internal class GetWorld : GameAction
    {
        internal override ActionType ActionType { get { return ActionType.GetWorld; } }
        public int UncompressedLength { get; set; }
        public override string ToString()
        {
            return "GetWorld";
        }

        internal override void Receive()
        {
            lock (TcpClient)
            {
                base.Receive();
                if (DataLength == 0) return;

                var response = new byte[DataLength];
                var bytesRead = 0;
                do
                {
                    Settings.Launcher.UpdateProgressInvokable(string.Format("Downloading: {0}kb / {1}kb", bytesRead / 1024, DataLength / 1024), bytesRead / 1024, DataLength / 1024);
                    bytesRead += TcpClient.GetStream().Read(response, bytesRead, Math.Min(DataLength - bytesRead, 4096));
                    //System.Threading.Thread.Sleep(50); //simulate remote connect
                } while (bytesRead < DataLength);
                
                var memstream = new MemoryStream(response);
                var gzstream = new DeflateStream(memstream, CompressionMode.Decompress);

                var worldSettingsSizeBytes = new byte[sizeof(int)];
                bytesRead = 0;
                while (bytesRead < sizeof(int))
                {
                    bytesRead += gzstream.Read(worldSettingsSizeBytes, bytesRead, sizeof(int) - bytesRead); //read the size of the world config xml
                }
                UncompressedLength = bytesRead;
                var worldSettingsBytes = new byte[BitConverter.ToInt32(worldSettingsSizeBytes, 0)];

                bytesRead = 0;
                while (bytesRead < worldSettingsBytes.Length)
                {
                    bytesRead += gzstream.Read(worldSettingsBytes, bytesRead, worldSettingsBytes.Length - bytesRead);
                }
                UncompressedLength += bytesRead;
                WorldSettings.LoadSettings(worldSettingsBytes);

                var chunkTotal = WorldData.SizeInChunksX * WorldData.SizeInChunksZ;
                var chunkCount = 1;
                for (var x = 0; x < WorldData.SizeInChunksX; x++) //loop through each chunk and load it
                {
                    for (var z = 0; z < WorldData.SizeInChunksZ; z++)
                    {
                        Settings.Launcher.UpdateProgressInvokable(string.Format("Loading Chunks: {0} / {1}", chunkCount, chunkTotal), chunkCount, chunkTotal);
                        
                        bytesRead = 0;
                        var chunkBytes = new byte[Chunk.SIZE_IN_BYTES];
                        while (bytesRead < chunkBytes.Length)
                        {
                            var byteCount = gzstream.Read(chunkBytes, bytesRead, chunkBytes.Length - bytesRead);
                            if (byteCount <= 0) throw new Exception("Received incomplete world."); //gm: was sometimes getting zero here because the gzipstream wasnt getting flushed properly, might as well leave the check here
                            bytesRead += byteCount;
                        }
                        UncompressedLength += bytesRead;
                        
                        chunkCount++;
                    }
                }

                gzstream.Dispose();
                memstream.Dispose();
            }

            WorldData.InitializeAllLightMaps();

            Settings.Launcher.UpdateProgressInvokable("World Ready", 0, 0);
            WorldData.IsLoaded = true;
        }
    }
}