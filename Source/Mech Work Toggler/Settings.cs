using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mech_Work_Toggler
{
    [StaticConstructorOnStartup]
    public static class Init
    {
        static Init() => MechWorkTogglerMod.settings.toggledWorkGivers.ToList().ForEach(name => DefDatabase<WorkGiverDef>.GetNamed(name).canBeDoneByMechs ^= true);
    }

    public class Settings : ModSettings
    {
        public HashSet<string> toggledWorkGivers = new HashSet<string>();
        private WorkTypeDef workType;
        private Vector2 scrollPosition = Vector2.zero;

        private void Toggle(string name)
        {
            var workGiver = DefDatabase<WorkGiverDef>.GetNamed(name);
            workGiver.canBeDoneByMechs = !workGiver.canBeDoneByMechs;
            if (toggledWorkGivers.Contains(name))
                toggledWorkGivers.Remove(name);
            else
                toggledWorkGivers.Add(name);
        }

        public void DoWindowContents(Rect inRect)
        {
            var curY = 40f;
            var height = 32f;
            var width = inRect.width;
            var workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            if (workType == null)
                workType = workTypes.First();
            if (Widgets.ButtonText(new Rect(0f, curY, width / 3, height), workType.labelShort))
                Find.WindowStack.Add(new FloatMenu(workTypes.Select(workType => new FloatMenuOption(workType.labelShort, () => this.workType = workType)).ToList()));
            if (Widgets.ButtonText(new Rect(width / 3, curY, width / 3, height), "Reset".Translate()))
                workType.workGiversByPriority.Where(workGiver => toggledWorkGivers.Contains(workGiver.defName)).ToList().ForEach(workGiver => Toggle(workGiver.defName));
            if (Widgets.ButtonText(new Rect(width * 2 / 3, curY, width / 3, height), "ResetAll".Translate()))
                toggledWorkGivers.ToList().ForEach(workGiver => Toggle(workGiver));
            curY += height + 12f;
            var scrollviewHeight = workType.workGiversByPriority.Count() * height;
            Widgets.BeginScrollView(new Rect(0f, curY, width, inRect.height - curY - 40f), ref scrollPosition, new Rect(0f, curY, width - 20f, scrollviewHeight));
            workType.workGiversByPriority.ForEach(workGiver =>
            {
                var toggled = toggledWorkGivers.Contains(workGiver.defName);
                var rowRect = new Rect(0f, curY, width - 20f, height);
                new WidgetRow(rowRect.xMin, rowRect.yMin, UIDirection.RightThenDown).Label(workGiver.LabelCap.Colorize(toggled ? Color.white : Color.gray));
                if (new WidgetRow(rowRect.xMax, rowRect.yMin, UIDirection.LeftThenDown).ButtonIcon(workGiver.canBeDoneByMechs ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    Toggle(workGiver.defName);
                curY += height;
            });
            Widgets.EndScrollView();
        }

        public override void ExposeData() => Scribe_Collections.Look(ref toggledWorkGivers, "toggledWorkGivers");
    }

    public class MechWorkTogglerMod : Mod
    {
        public static Settings settings;

        public MechWorkTogglerMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);

        public override string SettingsCategory() => "MechWorkToggler.MechWorkTogglerMod".Translate();
    }
}
