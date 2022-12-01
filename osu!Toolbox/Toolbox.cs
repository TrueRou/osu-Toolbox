using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using OsuParsers.Decoders;
using OsuParsers.Database;
using Collection = OsuParsers.Database.Objects.Collection;
using Coosu.Database;
using static osu_Toolbox.SampleOsuDbReaderExtensions;

namespace osu_Toolbox
{
    public class Toolbox
    {
        
        public static readonly string ClientPath = FindClient();
        public static readonly string WorkFolder = Environment.CurrentDirectory;
        public static readonly OsuData OsuData = new();

        private static string FindClient()
        {
            var value = Registry.ClassesRoot.OpenSubKey("osu!")?.OpenSubKey("DefaultIcon").GetValue("");
            if (value == null) return null;
            return Regex.Match(value.ToString(), "\"(.*?)\"").Value.Replace("\"", "").Replace("osu!.exe", "");
        }
    }

    public class Config
    {
        public void Save()
        {
            File.WriteAllText(Path.Combine(Toolbox.WorkFolder, "config.json"), JsonConvert.SerializeObject(JObject.FromObject(this), Formatting.Indented));
        }

        public static Config Load()
        {
            if (File.Exists(Path.Combine(Toolbox.WorkFolder, "config.json")))
            {
                return JObject.Parse(File.ReadAllText(Path.Combine(Toolbox.WorkFolder, "config.json"))).ToObject<Config>();
            }
            return new Config();
        }
    }

    public class OsuData
    {
        private CollectionDatabase collectionDb;
        public List<Collection> Collections { get { return collectionDb.Collections; } }
        public IEnumerable<SimpleBeatmap> GetBeatmapEnumerator()
        {
            return SampleOsuDbReaderExtensions.EnumerateTinyBeatmaps(new OsuDbReader(Path.Combine(Toolbox.ClientPath, "osu!.db")));
        }
        public void LoadCollectionData()
        {
            collectionDb = DatabaseDecoder.DecodeCollection(Path.Combine(Toolbox.ClientPath, "collection.db"));
        }

        public void SaveCollections(List<Collection> newCollection)
        {
            new CollectionDatabase
            {
                CollectionCount = newCollection.Count,
                OsuVersion = collectionDb.OsuVersion,
                Collections = newCollection
            }.Save(Path.Combine(Toolbox.ClientPath, "collections.db"));
            collectionDb = DatabaseDecoder.DecodeCollection(Path.Combine(Toolbox.ClientPath, "collections.db"));
        }
    }

    public static class SampleOsuDbReaderExtensions
    {
        public class SimpleBeatmap
        {
            internal string Creator;
            internal string Difficulty;
            internal int BeatmapId;
            internal int BeatmapSetId;
            internal string FolderName;
            internal string Md5Hash;
            internal string Artist;
            internal string Title;
        }
        public static IEnumerable<SimpleBeatmap> EnumerateTinyBeatmaps(this OsuDbReader reader)
        {
            SimpleBeatmap beatmap = null;

            while (!reader.IsEndOfStream && reader.Read())
            {
                if (reader.NodeType == NodeType.ObjectStart)
                {
                    beatmap = new SimpleBeatmap();
                    continue;
                }

                if (reader.NodeType == NodeType.ObjectEnd && beatmap != null)
                {
                    yield return beatmap;
                    beatmap = null;
                }

                if (reader.NodeType == NodeType.ArrayEnd && reader.NodeId == 7) yield break;
                if (beatmap == null) continue;
                if (reader.NodeType is not (NodeType.ArrayStart or NodeType.KeyValue)) continue;
                FillProperty(reader, beatmap);
            }
        }

        private static void FillProperty(OsuDbReader reader, SimpleBeatmap beatmap)
        {
            var nodeId = reader.NodeId;
            if (nodeId == 9) beatmap.Artist = reader.GetString();
            else if (nodeId == 11) beatmap.Title = reader.GetString();
            else if (nodeId == 13) beatmap.Creator = reader.GetString();
            else if (nodeId == 14) beatmap.Difficulty = reader.GetString();
            else if (nodeId == 16) beatmap.Md5Hash = reader.GetString();
            else if (nodeId == 46) beatmap.BeatmapId = reader.GetInt32();
            else if (nodeId == 47) beatmap.BeatmapSetId = reader.GetInt32();
            else if (nodeId == 63) beatmap.FolderName = reader.GetString();
        }
    }
}
