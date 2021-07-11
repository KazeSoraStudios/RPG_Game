using System;
using System.Collections.Generic;
using RPG_Character;

namespace RPG_GameData
{
    public class GameDataPartyHandler : GameDataHandler
    {
        public static Dictionary<string, PartyMemeberDefintion> ProcessParty(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Items.");
            LogManager.LogDebug($"Processing items for data {data}");
            var items = new Dictionary<string, PartyMemeberDefintion>();
            for (int i = 0; i < count; i++)
            {
                var partyMember = new PartyMemeberDefintion()
                {
                    Id = data[index++],
                    StatsId = data[index++],
                    StatGrowth = GetStatGrowth(data[index++]),
                    ActionGrowth = GetActionGrowth(data[index++]),
                    Portrait = data[index++],
                    Name = data[index++],
                    Level = int.Parse(data[index++])
                };
                index += columnAdvance;
                items.Add(partyMember.Id, partyMember);
            }
            LogManager.LogDebug("Processing Gamedata Items finished.");
            return items;
        }

        private static ActionGrowth GetActionGrowth(string data)
        {
            var actionGrowth = new ActionGrowth();
            if (data.IsEmpty())
            {
                LogManager.LogError($"Action Growth is empty skipping.");
                return actionGrowth;
            }
            var growths = data.Split('/');
            foreach (var growth in growths)
            {
                var growthData = growth.Split(':');
                if (growthData.Length > 3)
                {
                    LogManager.LogError($"Invalid Action data for {growth}");
                    continue;
                }
                if (!int.TryParse(growthData[0], out var level))
                {
                    LogManager.LogError($"Invalid format for growth {growth}. Level is expected first.");
                    continue;
                }
                if (growth[1].Equals("magic"))
                {
                    if (!actionGrowth.Spells.ContainsKey(level))
                        actionGrowth.Spells.Add(level, new List<string>());
                    actionGrowth.Spells[level].Add(growthData[2]);
                }
                else
                {
                    if (!actionGrowth.Special.ContainsKey(level))
                        actionGrowth.Special.Add(level, new List<string>());
                    actionGrowth.Special[level].Add(growthData[2]);
                }
            }
            return actionGrowth;
        }

        private static StatGrowth GetStatGrowth(string data)
        {
            var statGrowth = new StatGrowth();
            if (data.IsEmpty())
            {
                LogManager.LogError($"Stat Growth is empty skipping.");
                return statGrowth;
            }
            var growths = data.Split('/');
            foreach (var growth in growths)
            {
                var growthData = growth.Split(':');
                if (growthData.Length > 2)
                {
                    LogManager.LogError($"Invalid growth data for {growth}");
                    continue;
                }
                var stat = GetEnum(Stat.HP, growthData[0]);
                Dice dice;
                if (char.IsDigit(growthData[1][0]))
                {
                    dice = BuildDice(growthData[1]);
                }
                else
                {
                    dice = GetDice(growthData[1]);
                }
                statGrowth.Growths.Add(stat, dice);
            }
            return statGrowth;
        }

        private static Dice BuildDice(string diceString)
        {
            int numberOfDice = diceString[0] - '0';
            int plusLocation = diceString.IndexOf('+');
            int sides = int.Parse(diceString.Substring(2, plusLocation - 2));
            int baseValue = int.Parse(diceString.Substring(plusLocation));

            var dice = new List<Die>();
            for (int i = 0; i < numberOfDice; i++)
                dice.Add(new Die
                {
                    Rolls = 1,
                    Faces = sides,
                    Bonus = baseValue
                });
            return new Dice
            {
                _Dice = dice
            };
        }

        public static Dice GetDice(string dice)
        {
            return dice.Equals("fast", StringComparison.OrdinalIgnoreCase) ? Fast :
                dice.Equals("med", StringComparison.OrdinalIgnoreCase) ? Medium :
                Slow;

        }

        private static Dice Slow = new Dice
        {
            _Dice = new List<Die>
        {
            new Die
            {
                Rolls = 1,
                Faces = 2,
                Bonus = 0
            }
        }
        };

        private static Dice Medium = new Dice
        {
            _Dice = new List<Die>
        {
            new Die
            {
                Rolls = 1,
                Faces = 3,
                Bonus = 0
            }
        }
        };

        private static Dice Fast = new Dice
        {
            _Dice = new List<Die>
        {
            new Die
            {
                Rolls = 3,
                Faces = 2,
                Bonus = 0
            }
        }
        };
    }
}