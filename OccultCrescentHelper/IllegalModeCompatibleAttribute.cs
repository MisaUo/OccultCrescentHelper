using Dalamud.Interface;
using Ocelot.Config.Attributes;

namespace OccultCrescentHelper;

internal sealed class IllegalModeCompatibleAttribute : IconAttribute
{
    public IllegalModeCompatibleAttribute()
        : base(FontAwesomeIcon.Skull, "generic.tooltip.illegal_mode_compatible", 0.6f, 0.4f, 0.77f)
    {
    }
}
