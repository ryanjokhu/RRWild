using Newtonsoft.Json;
using RRWild.Models;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RRWild.Services
{
    public sealed class WildPointStore
    {
        private readonly string _path;
        private readonly string _tmpPath;

        private readonly Dictionary<int, WildPoint> _points = new Dictionary<int, WildPoint>();

        private readonly System.Random _rng = new System.Random();

        public WildPointStore(string directoryPath)
        {
            _path = Path.Combine(directoryPath, "WildPoints.json");
            _tmpPath = _path + ".tmp";
        }

        public int Count => _points.Count;

        public bool TryGetRandom(out WildPoint point)
        {
            point = null;
            if (_points.Count == 0) return false;

            int target = _rng.Next(0, _points.Count);
            int i = 0;

            foreach (var kv in _points)
            {
                if (i == target)
                {
                    point = kv.Value;
                    return true;
                }
                i++;
            }

            return false;
        }

        public bool TrySet(int index, Vector3 pos, float yaw)
        {
            if (index <= 0) return false;
            _points[index] = new WildPoint(pos.x, pos.y, pos.z, yaw);
            return true;
        }

        public bool Remove(int index)
        {
            if (index <= 0) return false;
            return _points.Remove(index);
        }

        public void Clear()
        {
            _points.Clear();
        }

        public void Load()
        {
            _points.Clear();

            if (!File.Exists(_path))
                return;

            try
            {
                string json = File.ReadAllText(_path);
                var dict = JsonConvert.DeserializeObject<Dictionary<int, WildPoint>>(json);
                if (dict == null) return;

                foreach (var kv in dict)
                {
                    if (kv.Key <= 0 || kv.Value == null) continue;
                    _points[kv.Key] = kv.Value;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RRWild] Failed to load WildPoints.json: {ex}");
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_points, Formatting.Indented);

                using (var fs = new FileStream(_tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                    sw.Flush();
                    fs.Flush(true);
                }

                if (File.Exists(_path))
                    File.Replace(_tmpPath, _path, null);
                else
                    File.Move(_tmpPath, _path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RRWild] Failed to save WildPoints.json: {ex}");
                try { if (File.Exists(_tmpPath)) File.Delete(_tmpPath); } catch { }
            }
        }
    }
}
