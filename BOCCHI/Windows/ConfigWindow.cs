using Ocelot.Windows;
using Ocelot.Modules;
using Ocelot.Config.Attributes;
using System.Numerics;
using System.Reflection;
using System.Linq;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using Ocelot;

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

    public override void Draw()
    {
        var modules = plugin.modules.GetModulesByConfigOrder().ToList();
        selectedConfigModule ??= modules.FirstOrDefault();

        using (ImRaii.Child("##LeftPanel", new Vector2(300, 0), true))
        {
            foreach (var module in modules)
            {
                var concreteModule = module as Module<Plugin, Config>;
                if (concreteModule == null || concreteModule.config == null)
                {
                    continue;
                }

                var name = concreteModule.config.GetType().Name;

                var title = concreteModule.config.GetType().GetCustomAttribute<TitleAttribute>();
                if (title != null)
                {
                    name = I18N.T(title.translation_key);
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
            selectedConfigModule!.DrawConfigUi();
        }
    }
}
