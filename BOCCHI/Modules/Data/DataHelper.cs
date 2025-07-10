using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BOCCHI.Data;
using ECommons.DalamudServices;

namespace BOCCHI.Modules.Data;

using Data = List<uint>;

public class DataHelper
{
    private readonly Dictionary<uint, string> Paths = new()
    {
        { ZoneData.SOUTHHORN, Path.Join(Svc.PluginInterface.ConfigDirectory.FullName, "southhorn_enemies.json") },
    };

    private readonly JsonSerializerOptions options = new() { WriteIndented = true };

    private Data LoadSchema()
    {
        if (!Paths.TryGetValue(Svc.ClientState.TerritoryType, out var path))
        {
            throw new KeyNotFoundException($"No JSON path configured for territory {Svc.ClientState.TerritoryType}");
        }

        if (!File.Exists(path))
        {
            return [];
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Data>(json) ?? [];
    }

    private void SaveSchema(Data data)
    {
        if (!Paths.TryGetValue(Svc.ClientState.TerritoryType, out var path))
        {
            throw new KeyNotFoundException($"No JSON path configured for territory {Svc.ClientState.TerritoryType}");
        }

        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(path, json);
    }

    public bool HasSharedEnemyData(Enemy enemy)
    {
        return !Paths.ContainsKey(Svc.ClientState.TerritoryType) || LoadSchema().Contains(enemy.LayoutId);
    }

    public void MarkSharedEnemyData(Enemy enemy)
    {
        if (!Paths.ContainsKey(Svc.ClientState.TerritoryType))
        {
            return;
        }

        var schema = LoadSchema();
        schema.Add(enemy.LayoutId);
        SaveSchema(schema);
    }
}
