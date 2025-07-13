using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Ocelot.Modules;
using Ocelot.Windows;

namespace BOCCHI.Windows;

[OcelotConfigWindow]
public class ConfigWindow(Plugin primaryPlugin, Config config) : OcelotConfigWindow(primaryPlugin, config)
{
    private IModule? selectedConfigModule;

    public override void PostInitialize()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 0),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void Render()
    {
        var modules = plugin.Modules.GetModulesByConfigOrder().ToList();
        selectedConfigModule ??= modules.FirstOrDefault();

        using (ImRaii.Child("##LeftPanel", new Vector2(300, 0), true))
        {
            foreach (var module in modules)
            {
                var concreteModule = module as Module<Plugin, Config>;
                if (concreteModule == null || concreteModule.Config == null)
                {
                    continue;
                }

                var name = concreteModule.Config.GetType().Name;

                var title = concreteModule.Config.GetTitle();
                if (title != null)
                {
                    name = title;
                }

                var selected = module == selectedConfigModule;
                if (ImGui.Selectable(name, selected))
                {
                    selectedConfigModule = module;
                }
            }
        }

        ImGui.SameLine();


        using (ImRaii.Child("##RightPanel", new Vector2(0, 0), true))
        {
            selectedConfigModule!.RenderConfigUi();
        }
    }
}
