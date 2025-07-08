using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Logic.Unit;

public class Logic
{
    public static class IO
    {
        public static void Print(string message) { }

        public static class Draw
        {
            public static void Clear() { }
            public static void Line(double x1, double y1, double x2, double y2) { }
            public static void LineRect(double x, double y, double w, double h) { }
            public static void LinePoly(double x, double y, int sides, double radius) { }
            public static void Image(string image, double x, double y, double w, double h, double rotation = 0) { }

            public static void Col(double r, double g, double b, double a = 1) { }
            public static void Color(double r, double g, double b, double a = 1) { }
            public static void Stroke(double thickness) { }
            public static void Rect(double x, double y, double w, double h) { }
            public static void Poly(double x, double y, int sides, double radius) { }
            public static void Triangle(double x1, double y1, double x2, double y2, double x3, double y3) { }
        }

        public static void Write(object value, string memoryName, int index) { }
        public static void Read(string destVar, string memoryName, int index) { }
    }

    public static class BlockControl
    {
        public enum Property { Enabled, Health, Heat, TotalItems, Items, Progress, Efficiency, Power, PowerNetStored, PowerNetCapacity, Liquid, LiquidCapacity, Ammo, Team, Type, X, Y, Rotation }

        public static class Control
        {
            public static void Enable(BlockType block, bool on) { }
            public static void Color(BlockType block, double r, double g, double b, double a = 1) { }
            public static void Shoot(BlockType turret, double x, double y, bool shoot) { }
            public static void ShootId(BlockType turret, int unitId, bool shoot) { }
            public static void Config(BlockType block, params object[] conf) { }
        }
        // generic sensor (basic block property)
        public static void Sensor(string destVar, BlockType block, Property prop) { }
        /// <summary>Read a specific <c>item</c> quantity from a container / core.</summary>
        public static void SensorItem(string destVar, BlockType block, ItemType item) { }
        /// <summary>Read a custom numeric cell (e.g., <c>@counter</c> or <c>memory[index]</c>).
        /// The translator will decide correct low‑level syntax.</summary>
        public static void SensorValue(string destVar, string what) { }


        public static void GetLink(string destVar, int linkIndex) { }

        public static void PrintFlush(BlockType messageBlock) { }
        public static void DrawFlush(BlockType displayBlock) { }
    }

    public static class Unit
    {
        public static void Bind(UnitType unitType) { }

        public static void UnBind() { }

        public static class Locate
        {
            public static bool Ore(
                ItemType ore,
                SortBy sortBy,
                SortOrder order,
                double x, double y, double radius) => false;
            public static bool Building(
                BlockType block,
                SortBy sortBy,
                SortOrder order,
                double x, double y, double radius) => false;
            public static bool Spawn() => false;
            public static bool Damaged(
                SortBy sortBy = SortBy.Health,
                SortOrder order = SortOrder.Lowest,
                double x = 0, double y = 0, double radius = 0) => false;
        }


        public static class Radar
        {
            public static bool Target(
                TargetType who,
                SortBy sortBy,
                SortOrder order,
                double x, double y, double radius) => false;
        }

        public static class Control
        {
            public static void Idle() { }
            public static void Stop() { }
            public static void Move(double x, double y) { }
            public static void Approach(double x, double y, double radius) { }
            public static void Pathfind(double x, double y) { }
            public static void AutoPathfind() { }
            public static void Boost(bool enable) { }
            public static void Target(double x, double y, int shoot) { }
            public static void TargetTP(int unit, int shoot) { }//??
            public static void ItemDrop(BlockType to, int amount) { }
            public static void ItemTake(BlockType from, ItemType item, int amount) { }
            public static void PayloadDrop() { }
            public static void PayloadTake(bool takeUnits) { }
            public static void PayloadEnter() { }
            public static void Mine(double x, double y) { }
            public static void Flag(int value) { }
            public static void Build(double x, double y, BlockType block, int rotation, string config) { }
            public static void GetBlock(double x, double y, string type, string building, string floor) { }
            public static void Within(double x, double y, double radius, string resultVar) { }
        }

        public enum UnitType
        {
            Dagger,
            Mace,
            Fortress,
            Scepter,
            Reign,
            Nova,
            Pulsar,
            Quasar,
            Vela,
            Corvus,
            Crawler,
            Atrax,
            Spiroct,
            Arkyid,
            Toxopid,
            Flare,
            Horizon,
            Zenith,
            Antumbra,
            Eclipse,
            Mono,
            Poly,
            Mega,
            Quad,
            Oct,
            Risso,
            Minke,
            Bryde,
            Sei,
            Omura
        }

        public enum TargetType
        {
            Any,
            Allies,
            Enemies
        }

        public enum SortBy
        {
            Distance,
            Health,
            Shield
        }

        public enum SortOrder
        {
            Lowest,
            Highest
        }

        // alphabetical slice – extend as needed
        public enum BlockType
        {
            CoreNucleus,
            CoreFoundation,
            CoreShard,
            CopperWall,
            TitaniumWall,
            PlastaniumWall,
            Duo,
            Scatter,
            Hail,
            Lancer,
            Salvo,
            Cyclone,
            Foreshadow,
            MechanicalDrill,
            PneumaticDrill,
            LaserDrill,
            BlastDrill,
            AirFactory,
            GroundFactory,
            NavalFactory,

            AdditiveReconstructor
            // … + every other vanilla block
        }

        public enum ItemType
        {
            Copper,
            Lead,
            Graphite,
            Titanium,
            Silicon,
            Thorium,
            Sand,
            Coal,
            Metaglass,
            Plastanium,
            PhaseFabric,
            SurgeAlloy,
            Scrap,
            Pyratite,
            BlastCompound,

            SporePods
            // … extend if mods add more
        }
    }


}
