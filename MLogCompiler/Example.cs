using Logic;
using Logic.Unit;

Logic.Unit.Bind(Logic.Unit.UnitType.Dagger); 
int targetX = 100, targetY = 100;
while (true)
{
	// Move the unit in a square patrol pattern
    Logic.Unit.Control.Move(targetX, targetY);
    if (Logic.Unit.Control.Within(targetX, targetY, 1))
    {
        // toggle target between (100,100) and (200,100) for patrol
        targetX = (targetX == 100 ? 200 : 100);
    }
   //ogic.(2.0);  // wait 2 seconds at each point
}
