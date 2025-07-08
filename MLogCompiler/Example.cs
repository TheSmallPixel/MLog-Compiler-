using Logic;
using Logic.Unit;
using Microsoft.CodeAnalysis.Diagnostics;

public static class Test
{
	public void Method()
	{
		Logic.Unit.Bind(Logic.Unit.UnitType.Dagger);
		int targetX = 100, targetY = 100;
		while (true)
		{
			var currentLink = Logic.BlockControl.GetLink(0);
			// Move the unit in a square patrol pattern
			Logic.Unit.Control.Move(targetX, targetY);
			if (Logic.Unit.Control.Within(targetX, targetY, 1))
			{
				// toggle target between (100,100) and (200,100) for patrol
				targetX = (targetX == 100 ? 200 : 100);
			}
			//ogic.(2.0);  // wait 2 seconds at each poin

            Logic.BlockControl.SensorItem(currentLink, Logic.Unit.BlockType.CopperWall, Logic.Unit.ItemType.Copper);
        }
	}

}
