using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoActMod.Actions;

public class TaskPick : TaskPoint
{
    public Thing refThing;
    public bool installed;
    public bool IsTarget(Card t) => t == refThing || t.CanStackTo(refThing);

    public TaskPick() { }
    public TaskPick(Point pos, Thing refThing, bool installed = false)
    {
        this.pos = pos;
        this.refThing = refThing;
        this.installed = installed;
    }

    public override IEnumerable<Status> Run()
    {
        yield return DoGoto(pos, 1, true, null);
        bool success = false;
        if (installed)
        {
            var t = pos.Installed;
            if ((t.IsNull() || !IsTarget(t)) && pos.HasThing)
            {
                t = pos.Things.Find(t => t.placeState == PlaceState.installed && IsTarget(t));
            }
            if (t.HasValue() && IsTarget(t))
            {
                if (!pc.CanLift(t))
                {
                    pc.Say("tooHeavy", t, null, null);
                }
                if (t.HasEditorTag(EditorTag.TreasureMelilith))
                {
                    if (player.flags.pickedMelilithTreasure)
                    {
                        pc.PlaySound("curse3", 1f, true);
                        pc.PlayEffect("curse", true, 0f);
                        pc.SetFeat(1206, 1, true);
                        player.flags.gotMelilithCurse = true;
                    }
                    else
                    {
                        Msg.Say("pickedMelilithTreasure");
                        player.flags.pickedMelilithTreasure = true;
                        QuestCursedManor questCursedManor = game.quests.Get<QuestCursedManor>();
                        questCursedManor?.NextPhase();
                    }
                    t.c_editorTags = null;
                }
                pc.HoldCard(t, -1);
                if (pc.held.HasValue())
                {
                    t.PlaySoundHold(false);
                    player.RefreshCurrentHotItem();
                    ActionMode.Adv.planRight.Update(ActionMode.Adv.mouseTarget);
                    pc.renderer.Refresh();
                    success = true;
                }
            }
        }
        else
        {
            pos.Things.Where(t => IsTarget(t)).ToArray().ForeachReverse(t =>
            {
                pc.Pick(t, true, true);
                success = true;
            });
        }
        if (success)
        {
            yield break;
        }
        else
        {
            yield return Cancel();
        }
    }
}