using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BOCCHI.Data;
using ECommons.DalamudServices;

namespace BOCCHI.Modules.Data;

using Schema = List<uint>;

public class DataHelper
{
    private readonly Dictionary<uint, string> Paths = new()
    {
        { ZoneData.SOUTHHORN, Path.Join(Svc.PluginInterface.ConfigDirectory.FullName, "southhorn_enemies.json") },
    };

    private Schema LoadSchema()
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
        var schema = JsonSerializer.Deserialize<Schema>(json);
        return schema ?? [];
    }

    private void SaveSchema(Schema schema)
    {
        if (!Paths.TryGetValue(Svc.ClientState.TerritoryType, out var path))
        {
            throw new KeyNotFoundException($"No JSON path configured for territory {Svc.ClientState.TerritoryType}");
        }

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public bool HasSharedEnemyData(Enemy enemy)
    {
        if (!Paths.ContainsKey(Svc.ClientState.TerritoryType))
        {
            return true;
        }

        return LoadSchema().Contains(enemy.LayoutId);
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
