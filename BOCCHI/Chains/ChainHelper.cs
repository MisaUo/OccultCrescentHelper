using System;
using System.Numerics;
using BOCCHI.Enums;
using BOCCHI.Modules.Mount;
using BOCCHI.Modules.Mount.Chains;
using BOCCHI.Modules.Teleporter;
using BOCCHI.Modules.Treasure;
using ECommons.GameHelpers;
using Ocelot.Chain;
using Ocelot.Chain.ChainEx;
using Ocelot.IPC;
using Ocelot.Modules;

namespace BOCCHI.Chains;

public class ChainHelper
{
    private static ChainHelper? _instance = null;

    private static ChainHelper instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("ChainHelper has not been initialized. Call Initialize(plugin) first.");
            }

            return _instance;
        }
    }

    private Plugin plugin;

    private static ModuleManager modules
    {
        get => instance.plugin.modules;
    }

    private static IPCManager ipc
    {
        get => instance.plugin.ipc;
    }

    private ChainHelper(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public static void Initialize(Plugin plugin)
    {
        _instance ??= new ChainHelper(plugin);
    }

    public static ReturnChain ReturnChain()
    {
        var config = new ReturnChainConfig
        {
            ApproachAetheryte = instance.plugin.config.TeleporterConfig.ApproachAetheryte,
        };

        return ReturnChain(config);
    }

    public static ReturnChain ReturnChain(ReturnChainConfig config)
    {
        return new ReturnChain(modules.GetModule<TeleporterModule>(), config);
    }

    public static TeleportChain TeleportChain(Aethernet aethernet)
    {
        return new TeleportChain(
            aethernet,
            ipc.GetProvider<Lifestream>(),
            modules.GetModule<TeleporterModule>()
        );
    }

    public static MountChain MountChain()
    {
        return new MountChain(modules.GetModule<MountModule>().config);
    }

    public static Func<Chain> PathfindToAndWait(Vector3 destination, float distance)
    {
        var vnav = ipc.GetProvider<VNavmesh>();
        return () => Chain.Create()
            .ConditionalThen(_ => Player.DistanceTo(destination) > distance, _ =>
                Chain.Create()
                    .Then(new PathfindAndMoveToChain(vnav, destination))
                    .WaitUntilNear(vnav, destination, distance)
                    .Then(_ => vnav.Stop())
            );
    }

    public static Func<Chain> MoveToAndWait(Vector3 destination, float distance)
    {
        var vnav = ipc.GetProvider<VNavmesh>();
        return () => Chain.Create()
            .ConditionalThen(_ => Player.DistanceTo(destination) > distance, _ =>
                Chain.Create()
                    .Then(_ => vnav.MoveToPath([destination], false))
                    .WaitUntilNear(vnav, destination, distance)
                    .Then(_ => vnav.Stop())
            );
    }

    public static TreasureSightChain TreasureSightChain()
    {
        return new TreasureSightChain(modules.GetModule<TreasureModule>());
    }
}
