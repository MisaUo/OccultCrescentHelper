using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BOCCHI.Modules.CriticalEncounters;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace BOCCHI.Modules.Data;

public class Api : IDisposable
{
    private readonly DataModule module;

    private readonly HttpClient client = new();

    private readonly EnemyDataHelper EnemyData = new();

    private readonly TrapDataHelper TrapData = new();

    private string towerHash = "";

    public Api(DataModule module)
    {
        this.module = module;
    }

    public void Initialize()
    {
        module.GetModule<CriticalEncountersModule>().Tracker.OnBattleState += OnCriticalEncounterBattle;

        GenerateHash();
    }

    public async Task SendEnemyData(IGameObject obj)
    {
        var enemy = new Enemy(obj);
        if (EnemyData.HasSharedData(enemy))
        {
            return;
        }

        const string url = "https://api.oc.ohkannaduh.com/monster_spawn";
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
                EnemyData.MarkSharedData(enemy);
                Svc.Log.Info($"Response: {responseBody}");
            }
            else
            {
                Svc.Log.Debug($"Failed to send data. Status code: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Svc.Log.Debug($"Error response: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Debug($"Failed to send request: {ex.Message}");
        }
    }


    public async Task SendTrapData(IGameObject obj)
    {
        var trap = new Trap(obj, towerHash);
        if (TrapData.HasSharedData(trap))
        {
            return;
        }

        const string url = "https://api.oc.ohkannaduh.com/trap_position";
        var payload = TrapPayload.Create(trap);
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
                TrapData.MarkSharedData(trap);
                Svc.Log.Info($"Response: {responseBody}");
            }
            else
            {
                Svc.Log.Debug($"Failed to send data. Status code: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Svc.Log.Debug($"Error response: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Debug($"Failed to send request: {ex.Message}");
        }
    }

    private void OnCriticalEncounterBattle(DynamicEvent ev)
    {
        if (ev.EventType < 4)
        {
            return;
        }

        GenerateHash();
    }

    private void GenerateHash()
    {
        using var sha256 = SHA256.Create();

        var timeBytes = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var contentIdBytes = BitConverter.GetBytes(Player.CID);

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
            Array.Reverse(contentIdBytes);
        }

        var combined = new byte[timeBytes.Length + contentIdBytes.Length];
        Buffer.BlockCopy(timeBytes, 0, combined, 0, timeBytes.Length);
        Buffer.BlockCopy(contentIdBytes, 0, combined, timeBytes.Length, contentIdBytes.Length);

        var hashBytes = sha256.ComputeHash(combined);

        towerHash = Convert.ToBase64String(hashBytes);
        Svc.Log.Debug($"New Tower Hash Generated {towerHash}");
    }

    public void Dispose()
    {
        module.GetModule<CriticalEncountersModule>().Tracker.OnBattleState -= OnCriticalEncounterBattle;
    }
}
