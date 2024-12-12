using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElinAutoAct.Actions;

public class AutoActClean : AutoAct
{
    public int detRangeSq;
    public Point pos;
    public override Point Pos => pos;

    public AutoActClean()
    {
        detRangeSq = Settings.DetRangeSq;
    }

    public static AutoActClean TryCreate(string id, Point pos)
    {
        if (id != "actClean") { return null; }
        return new AutoActClean { pos = pos };
    }

    public override IEnumerable<Status> Run()
    {
        Status Process()
        {
            var held = owner.held;
            if (held?.trait is not TraitBroom)
            {
                return Status.Fail;
            }

            _map.SetDecal(pos.x, pos.z, 0, 1, true);
            _map.SetLiquid(pos.x, pos.z, 0, 0);
            pos.PlayEffect("vanish");
            owner.Say("clean", held, null, null);
            owner.PlaySound("clean_floor", 1f, true);
            owner.stamina.Mod(-1);
            owner.ModExp(293, 40);
            return KeepRunning();
        };

        yield return Process();
        while (CanProgress())
        {
            var targetPos = FindNextPos(cell => !cell.HasBlock && (cell.decal > 0 || (cell.effect.HasValue() && cell.effect.IsLiquid)), detRangeSq);
            if (targetPos.IsNull())
            {
                SayNoTarget();
                yield break;
            }

            pos = targetPos;
            yield return Process();
        }
        yield break;
    }
}