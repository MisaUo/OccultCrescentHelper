using OccultCrescentHelper.Enums;

namespace OccultCrescentHelper.Data;

public class Job
{
    public readonly JobId id;

    public readonly PlayerStatus status;

    public Job(JobId id, PlayerStatus status)
    {
        this.id = id;
        this.status = status;
    }

    public readonly static Job Freelancer = new(JobId.Freelancer, PlayerStatus.PhantomFreelancer);

    public readonly static Job Knight = new(JobId.Knight, PlayerStatus.PhantomKnight);

    public readonly static Job Berserker = new(JobId.Berserker, PlayerStatus.PhantomBerserker);

    public readonly static Job Monk = new(JobId.Monk, PlayerStatus.PhantomMonk);

    public readonly static Job Ranger = new(JobId.Ranger, PlayerStatus.PhantomRanger);

    public readonly static Job Samurai = new(JobId.Samurai, PlayerStatus.PhantomSamurai);

    public readonly static Job Bard = new(JobId.Bard, PlayerStatus.PhantomBard);

    public readonly static Job Geomancer = new(JobId.Geomancer, PlayerStatus.PhantomGeomancer);

    public readonly static Job TimeMage = new(JobId.TimeMage, PlayerStatus.PhantomTimeMage);

    public readonly static Job Cannoneer = new(JobId.Cannoneer, PlayerStatus.PhantomCannoneer);

    public readonly static Job Chemist = new(JobId.Chemist, PlayerStatus.PhantomChemist);

    public readonly static Job Oracle = new(JobId.Oracle, PlayerStatus.PhantomOracle);

    public readonly static Job Thief = new(JobId.Thief, PlayerStatus.PhantomThief);
}
