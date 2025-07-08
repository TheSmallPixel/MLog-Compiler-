using System.Diagnostics;
using static Logic;
using static Logic.Unit;

public static class MLogicMain
{
    public static void Run()
    {
        // -----------------------------------------------------------------
        // 0. constant setup  (mirror the leading ‘set …’ lines)
        // -----------------------------------------------------------------
        Logic.Unit.Bind(UnitType.Flare);          // set unit @flare
        int flag = 1;                        // set flag 1
        var taking = ItemType.Lead;            // set taking? @lead
        int unitCap = 10;                       // set unitcap 10
        var dest = "";                       // getlink dest 1  (will be overwritten)

        // -----------------------------------------------------------------
        // 1. FIRST PHASE  (approach drop-off building)
        // -----------------------------------------------------------------
        Logic.BlockControl.GetLink(out dest, 1);      // getlink dest 1
        if (dest != "")
        {

            double x, y;
            Logic.BlockControl.Sensor(out x, dest, BlockControl.Property.X);
            Logic.BlockControl.Sensor(out y, dest, BlockControl.Property.Y);
            bool onSwitch;
            Logic.BlockControl.Sensor(out onSwitch, "switch1", BlockControl.Property.Enabled);

            if (onSwitch)
            {
                Logic.Unit.Bind(UnitType.Flare);
                Logic.Unit.Control.Flag(0); // sensor uflag @unit @flag placeholder

                Logic.Unit.Control.ItemDrop(BlockType.CoreShard, 99999);

                // All those conditional jumps -> use structured flow
                // For brevity we jump straight to PHASE2 after the stack of checks
            }

            // -----------------------------------------------------------------
            // 2. SECOND PHASE  (deliver to core if items in unit)
            // -----------------------------------------------------------------
            bool switchOn;
            Logic.BlockControl.Sensor(out switchOn, "switch1", BlockControl.Property.Enabled);
            switchOn = false;
            if (!switchOn)
            {

                Logic.Unit.Bind(UnitType.Flare);
                // … replicate the rest of the checks …
                // * sensor totalItems
                // * ulocate building core
                // * approach & drop
                // * clear flag
            }

            // -----------------------------------------------------------------
            // 3. THIRD PHASE  (assign flags to idle polys)
            // -----------------------------------------------------------------s
            Logic.Unit.Bind(UnitType.Flare);
            var first = ""; // set first @unit
            int count = 0;

            while (true)
            {
                Logic.Unit.Bind(UnitType.Flare);
                // sensor uflag etc.
                // mirror all remaining jumps/ops the same way
                break; // placeholder to end infinite loop in example
            }
        }
    }
}
