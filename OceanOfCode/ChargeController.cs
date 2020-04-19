using System.Collections.Generic;
using System.Linq;
using OceanOfCode.Surveillance;

namespace OceanOfCode
{
    public class ChargeController
    {
        private readonly EnemyTracker _enemyTracker;
        private int _moveCounter;
        List<(int, string)> _charges = new  List<(int, string)>();

        public ChargeController(EnemyTracker enemyTracker)
        {
            _enemyTracker = enemyTracker;
        }

        class Charge
        {
            public static string Torpedo = "TORPEDO";
            public static string Silence = "SILENCE";
            public static string Mine = "MINE";
            public static string Sonar = "SONAR";
            
        }
        public string NextPowerToCharge(MoveProps move)
        {
            _moveCounter++;
            string chosenCharge;
            
            if (_enemyTracker.DoWeHaveExactEnemyLocation())
            {
                if (move.TorpedoCooldown > 0)
                {
                    chosenCharge = Charge.Torpedo;
                }else if (move.SilenceCooldown > 0 && _charges.Take(6).Select(x => x.Item2).Any(c => !c.Equals(Charge.Silence)))
                {
                    chosenCharge = Charge.Silence;
                }else if (move.MineCooldown > 0)
                {
                    chosenCharge = Charge.Mine;
                }
                else
                {
                    chosenCharge = Charge.Silence;
                }

            }else if (move.SilenceCooldown > 0 && _charges.TakeLast(6).Select(x => x.Item2).Count(c => !c.Equals(Charge.Silence)) >= 2)
            {
                chosenCharge = Charge.Silence;
            }else if (move.MineCooldown > 0 && !_charges.TakeLast(7).Select(x => x.Item2).Any(c => c.Equals(Charge.Mine)))
            {
                chosenCharge = Charge.Mine;
            }
            else
            {
                chosenCharge = Charge.Torpedo;
            }

            _charges.Add((_moveCounter, chosenCharge));
            return chosenCharge;
        }
    }
}