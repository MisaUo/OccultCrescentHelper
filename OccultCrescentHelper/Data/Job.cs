using BOCCHI.Enums;

namespace BOCCHI.Data;

public class Job
{
    public static readonly Job Freelancer = new(JobId.Freelancer, PlayerStatus.PhantomFreelancer);
    public static readonly Job Knight = new(JobId.Knight, PlayerStatus.PhantomKnight);
    public static readonly Job Berserker = new(JobId.Berserker, PlayerStatus.PhantomBerserker);
    public static readonly Job Monk = new(JobId.Monk, PlayerStatus.PhantomMonk);
    public static readonly Job Ranger = new(JobId.Ranger, PlayerStatus.PhantomRanger);
    public static readonly Job Samurai = new(JobId.Samurai, PlayerStatus.PhantomSamurai);
    public static readonly Job Bard = new(JobId.Bard, PlayerStatus.PhantomBard);
    public static readonly Job Geomancer = new(JobId.Geomancer, PlayerStatus.PhantomGeomancer);
    public static readonly Job TimeMage = new(JobId.TimeMage, PlayerStatus.PhantomTimeMage);
    public static readonly Job Cannoneer = new(JobId.Cannoneer, PlayerStatus.PhantomCannoneer);
    public static readonly Job Chemist = new(JobId.Chemist, PlayerStatus.PhantomChemist);
    public static readonly Job Oracle = new(JobId.Oracle, PlayerStatus.PhantomOracle);
    public static readonly Job Thief = new(JobId.Thief, PlayerStatus.PhantomThief);
    public readonly JobId id;

    public readonly PlayerStatus status;

    public Job(JobId id, PlayerStatus status)
    {
        this.id = id;
        this.status = status;
    }
}
