using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeWars.SampleBot
{
    internal class SnakeEngine
    {
        private readonly string _mySnakeId;
        private readonly Random _random = new Random();

        public SnakeEngine(string mySnakeId)
        {
            _mySnakeId = mySnakeId;
        }

        public Move GetNextMove(GameBoardState gameBoardState)
        {
            //===========================
            // Your snake logic goes here
            //===========================

            var mySnake = gameBoardState.GetSnake(_mySnakeId);
            if (mySnake.IsAlive)
            {
                var occupiedCells = gameBoardState.GetOccupiedCells();

                // Check possible moves in random order.
                var moves = new List<Move>
                {
                    Move.Left,
                    Move.Right,
                    Move.Straight
                };

                var potentialSneakHeads = gameBoardState._gameState.Snakes.Where(s => s.IsAlive).Where(s => s.Id != _mySnakeId).SelectMany(x => moves.Select(m => m.GetSnakeNewHead(x, gameBoardState._gameState.BoardSize))).Distinct();

                var potentialConflicts = new List<Move>();

                occupiedCells = new HashSet<PointDTO>(occupiedCells.Concat(potentialSneakHeads));

                while (moves.Any())
                {

                    // Select random move.
                    var move = GetPrefferedDirection(gameBoardState._gameState.Food, moves, mySnake, gameBoardState._gameState.BoardSize);

                    if (move == Move.None)
                    {
                        Console.WriteLine("Fallback to Random!");
                        move = moves[_random.Next(moves.Count)];
                    }

                    moves.Remove(move);

                    var newHead = gameBoardState.GetSnakeNewHeadPosition(_mySnakeId, move);
                    if (!occupiedCells.Contains(newHead))
                    {
                        //if (IsPotentiallyClosed(2, newHead, gameBoardState.GetOccupiedCells()))
                        //{
                        //    potentialConflicts.Add(move);
                        //    continue;
                        //}

                        return move;
                    }

                    if (potentialSneakHeads.Contains(newHead))
                    {
                        potentialConflicts.Add(move);
                    }
                }

                if (potentialConflicts.Any())
                {
                    return potentialConflicts.First();
                }
            }
            return Move.None;
        }

        private Move GetPrefferedDirection(IEnumerable<PointDTO> food, List<Move> moves, SnakeDTO mySnake, SizeDTO boardSize)
        {
            var foodGroup = food.Select(f => new { Food = f, Metric = (Distance(mySnake.Head, f, boardSize) - NumOfNeigh(food, f) * 2) });

            var closestFoods = foodGroup.OrderBy(f => f.Metric).Select(f => f.Food);

            if (!closestFoods.Any())
            {
                return Move.None;
            }

            var closestFood = closestFoods.First();

            return moves.OrderBy(f => Distance(f.GetSnakeNewHead(mySnake, boardSize), closestFood)).First();
        }

        public int NumOfNeigh(IEnumerable<PointDTO> food, PointDTO f)
        {
            var neigh = new[] {
                new {X = -2, Y = -2},new {X = -1, Y = -2},new {X = 0, Y = -2},new {X = 1, Y = -2},new {X = 2, Y = -2},
            new {X = -2, Y = -1},new {X = -1, Y = -1},new {X = 0, Y = -1},new {X = 1, Y = -1},new {X = 2, Y = -1},
            new {X = -2, Y = 0},new {X = -1, Y = 0},new {X = 0, Y = 0},new {X = 1, Y = 0},new {X = 2, Y = 0},
            new {X = -2, Y = 1},new {X = -1, Y = 1},new {X = 0, Y = 1},new {X = 1, Y = 1},new {X = 2, Y = 1},
            new {X = -2, Y = 2},new {X = -1, Y = 2},new {X = 0, Y = 2},new {X = 1, Y = 2},new {X = 2, Y = 2}
        };

            return neigh.Where(n => food.Contains(new PointDTO { X = f.X + n.X, Y = f.Y + n.Y })).Count();
        }

        public bool IsPotentiallyClosed(int depth, PointDTO mySnakeHead, IEnumerable<PointDTO> taken)
        { 
            var neigh = new[] {
                new {X = -1, Y = -1},new {X = 0, Y = -1},new {X = 1, Y = -1},
                new {X = -1, Y = 0},new {X = 0, Y = 0},new {X = 1, Y = 0},
                new {X = -1, Y = 1},new {X = 0, Y = 1},new {X = 1, Y = 1}
            };

            var free = neigh.Select(n => new PointDTO { X = n.X + mySnakeHead.X, Y = n.Y + mySnakeHead.Y }).Except(taken);

            var newTaken = taken.Concat(free).Distinct();

            if (depth == 0) {
            
                return free.Any();
            }

            return free.Select(f => IsPotentiallyClosed(--depth, f, newTaken)).Any();
        }

        public int Distance(PointDTO head, PointDTO target, SizeDTO size)
        {
            var trans = new[] { 
                            new {X = 0, Y = 0},
                            new {X = 1, Y = 0},
                            new {X = -1, Y = 0},
                            new {X = 0, Y = 1},
                            new {X = 0, Y = -1}
            };

            var heads = trans.Select(t => new PointDTO { X = t.X * size.Width + head.X, Y = t.Y * size.Height + head.Y });

            return heads.Min(h => Distance(h, target));
        }

        public int Distance(PointDTO a, PointDTO b)
        {
            return (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));
        }
    }
}