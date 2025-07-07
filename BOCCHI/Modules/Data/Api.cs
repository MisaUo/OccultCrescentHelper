using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;

namespace BOCCHI.Modules.Data;

public class Api
{
    private readonly HttpClient client = new();

    private DataHelper data = new();

    public async Task SendEnemyData(IGameObject obj)
    {
        var enemy = new Enemy(obj);
        if (data.HasSharedEnemyData(enemy))
        {
            return;
        }

        var url = "https://api.oc.ohkannaduh.com/monster_spawn";
        var payload = MonsterPayload.Create(enemy);
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("x-api-key", "b1fba45b-6554-4dec-b53d-073deb8e3869");

        try
        {
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                Svc.Log.Info("Data sent successfully.");
                var responseBody = await response.Content.ReadAsStringAsync();
                data.MarkSharedEnemyData(enemy);
                Svc.Log.Info($"Response: {responseBody}");
            }
            else
            {
                Svc.Log.Error($"Failed to send data. Status code: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Svc.Log.Error($"Error response: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Failed to send request: {ex.Message}");
        }
    }
}
