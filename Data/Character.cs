using System.Collections.Immutable;
using System.Linq;

namespace BoneBaker.Data;

public class Character
{
    public Guid Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int XP { get; set; }
    public int Scratch { get; set; }
    public int NIndex { get; set; }
    public HumanityStats HumanityStats { get; set; } = new();
    public HumanityCounters HumanityCounters { get; set; } = new();
    public Home Home { get; set; } = new();
    public Origin Origin { get; set; } = new();
    public Path Path { get; set; } = new();
    public Style Style { get; set; } = new();
    public Training Training { get; set; } = new();
    public Knowledge Knowledge { get; set; } = new(ImmutableArray<DieStat>.Empty);

    public Character()
    {
        Identifier = Guid.NewGuid();
    }

    public Character(
        Guid identifier,
        string name,
        string gender,
        int xp,
        int scratch,
        int nIndex,
        HumanityStats humanityStats,
        HumanityCounters humanityCounters,
        Home home,
        Origin origin,
        Path path,
        Style style,
        Training training,
        Knowledge knowledge)
    {
        Identifier = identifier;
        Name = name;
        Gender = gender;
        XP = xp;
        Scratch = scratch;
        NIndex = nIndex;
        HumanityStats = humanityStats;
        HumanityCounters = humanityCounters;
        Home = home;
        Origin = origin;
        Path = path;
        Style = style;
        Training = training;
        Knowledge = knowledge;
    }

}

public class HumanityStats
{
}

public class HumanityCounters
{
}

public class Origin
{
}

public class Path
{
}

public class Home
{
}

public class Style
{
    public Die Tough { get; set; } = Die.D4;
    public Die Quick { get; set; } = Die.D4;
    public Die Cool { get; set; } = Die.D4;
    public Die Techie { get; set; } = Die.D4;
    public Die Planner { get; set; } = Die.D4;

    public Style()
    {
    }

    public Style(
        Die tough = Die.D4,
        Die quick = Die.D4,
        Die cool = Die.D4,
        Die techie = Die.D4,
        Die planner = Die.D4)
    {
        Tough = tough;
        Quick = quick;
        Cool = cool;
        Techie = techie;
        Planner = planner;
    }

    public DieStat[] AsDieStats()
    {
        return [

            new DieStat("Tough", Tough),
            new DieStat("Quick", Quick),
            new DieStat("Cool", Cool),
            new DieStat("Techie", Techie),
            new DieStat("Planner", Planner)
        ];
    }
}

public class Training
{
    public Die Awareness { get; set; } = Die.D4;
    public Die Combat { get; set; } = Die.D4;
    public Die Education { get; set; } = Die.D4;
    public Die Fitness { get; set; } = Die.D4;
    public Die Hardware { get; set; } = Die.D4;
    public Die Medicine { get; set; } = Die.D4;
    public Die Social { get; set; } = Die.D4;
    public Die Software { get; set; } = Die.D4;
    public Die Subtlety { get; set; } = Die.D4;
    public Die Vehicles { get; set; } = Die.D4;

    public Training()
    {
    }

    public Training(
        Die awareness = Die.D4,
        Die combat = Die.D4,
        Die education = Die.D4,
        Die fitness = Die.D4,
        Die hardware = Die.D4,
        Die medicine = Die.D4,
        Die social = Die.D4,
        Die software = Die.D4,
        Die subtlety = Die.D4,
        Die vehicles = Die.D4)
    {
        Awareness = awareness;
        Combat = combat;
        Education = education;
        Fitness = fitness;
        Hardware = hardware;
        Medicine = medicine;
        Social = social;
        Software = software;
        Subtlety = subtlety;
        Vehicles = vehicles;
    }

    // Additional arbitrary training stats that can be added at runtime
    public ImmutableArray<DieStat> AdditionalStats { get; set; } = ImmutableArray<DieStat>.Empty;

    // Return all training stats including the fixed set and any additional ones
    public DieStat[] AsDieStats()
    {
        return new[]
        {
            new DieStat("Awareness", Awareness),
            new DieStat("Combat", Combat),
            new DieStat("Education", Education),
            new DieStat("Fitness", Fitness),
            new DieStat("Hardware", Hardware),
            new DieStat("Medicine", Medicine),
            new DieStat("Social", Social),
            new DieStat("Software", Software),
            new DieStat("Subtlety", Subtlety),
            new DieStat("Vehicles", Vehicles)
        }.Concat(AdditionalStats).ToArray();
    }

    // Try to add an additional training stat; returns false with error if invalid
    // Add an additional training stat (validation occurs on the form)
    public bool TryAddStat(DieStat stat, out string? error)
    {
        if (stat is null)
        {
            error = "Stat is required";
            return false;
        }

        AdditionalStats = AdditionalStats.Add(stat);
        error = null;
        return true;
    }
}

public class Knowledge
{
    public ImmutableArray<DieStat> Stats { get; set; } = ImmutableArray<DieStat>.Empty;

    public Knowledge()
    {
    }

    public Knowledge(ImmutableArray<DieStat> stats)
    {
        Stats = stats;
    }

    public bool TryAddStat(DieStat stat, out string? error)
    {
        if (stat is null)
        {
            error = "Stat is required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(stat.Name))
        {
            error = "Name is required";
            return false;
        }

        if (Stats.Any(s => s.Die == stat.Die))
        {
            error = "Die value must be unique";
            return false;
        }

        Stats = Stats.Add(stat);
        error = null;
        return true;
    }
}

public class DieStat
{
    public string Name { get; set; } = string.Empty;
    public Die Die { get; set; }

    public DieStat()
    {
    }

    public DieStat(string name, Die die)
    {
        Name = name;
        Die = die;
    }

    public override string ToString()
    {
        return $"{Name} {Die}";
    }
}

public enum Die
{
    D4 = 4,
    D6 = 6,
    D8 = 8,
    D10 = 10,
    D12 = 12
}