using ItemChanger;
using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.Logging;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using System.IO;

namespace NotSpikesmas
{
    internal static class RandoInterop
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(1f, DefineTermsAndItems);
            RequestBuilder.OnUpdate.Subscribe(-499f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(0.5f, AddItem);
            SettingsLog.AfterLogSettings += LogRandoSettings;
            CondensedSpoilerLogger.AddCategorySafe("NotSpikesmas", () => true, new() { NotSpikesmas.NotSpikesmasItemName });
            RandomizerMenuAPI.AddMenuPage(_ => { }, BuildConnectionMenuButton);
            
            if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            {
                HookRSM();
            }
        }

        private static void HookRSM()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(
                new SimpleSettingsProxy<Settings>(
                    NotSpikesmas.Instance,
                    settings => NotSpikesmas.GS = settings ?? new(),
                    () => NotSpikesmas.GS.NotSpikesmasEnabled ? NotSpikesmas.GS : null
                )
            );
        }

        private static bool BuildConnectionMenuButton(MenuPage landingPage, out SmallButton button)
        {
            SmallButton settingsButton = new(landingPage, "Not Spikesmas");

            void UpdateButtonColor()
            {
                settingsButton.Text.color = NotSpikesmas.GS.NotSpikesmasEnabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }

            UpdateButtonColor();
            settingsButton.OnClick += () =>
            {
                NotSpikesmas.GS.NotSpikesmasEnabled = !NotSpikesmas.GS.NotSpikesmasEnabled;
                UpdateButtonColor();
            };
            button = settingsButton;
            return true;
        }

        private static void LogRandoSettings(LogArguments args, TextWriter tw)
        {
            string enabled = NotSpikesmas.GS.NotSpikesmasEnabled ? "Enabled" : "Disabled";
            tw.WriteLine($"{nameof(NotSpikesmas)} settings: {enabled}");
        }

        private static void AddItem(RequestBuilder rb)
        {
            if (!NotSpikesmas.GS.NotSpikesmasEnabled) return;

            rb.AddItemByName(NotSpikesmas.NotSpikesmasItemName);
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!NotSpikesmas.GS.NotSpikesmasEnabled) return;

            rb.EditItemRequest(NotSpikesmas.NotSpikesmasItemName, info =>
            {
                info.getItemDef = () => new ItemDef()
                {
                    Name = NotSpikesmas.NotSpikesmasItemName,
                    Pool = "NotSpikesmas",
                    MajorItem = false,
                    PriceCap = 500,
                };
            });

            rb.OnGetGroupFor.Subscribe(-999f, MatchGroup);

            static bool MatchGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
            {
                if (item == NotSpikesmas.NotSpikesmasItemName && (type == RequestBuilder.ElementType.Item || type == RequestBuilder.ElementType.Unknown))
                {
                    gb = rb.GetGroupFor(ItemNames.Ismas_Tear, type);
                    return true;
                }

                gb = default;
                return false;
            }
        }

        private static void DefineTermsAndItems(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!NotSpikesmas.GS.NotSpikesmasEnabled) return;

            Term notSpikesmasTerm = lmb.GetOrAddTerm("NOTSPIKESMAS");
            Term spikeTunnelsTerm = lmb.GetTerm("SPIKETUNNELS");
            TermValue[] effect;

            if (gs.SkipSettings.SpikeTunnels)
            {
                effect = new TermValue[] { new(notSpikesmasTerm, 1) };
            }
            else
            {
                effect = new TermValue[] { new(notSpikesmasTerm, 1), new(spikeTunnelsTerm, 1) };
            }

            lmb.AddItem(new CappedItem(
                NotSpikesmas.NotSpikesmasItemName, 
                effect,
                new(notSpikesmasTerm, 1)
                ));
        }
    }
}
