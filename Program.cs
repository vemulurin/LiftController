
namespace LiftProgram
{
    enum Direction { Up, Down }

    record Summon(int Floor, Direction Direction);
    record Request(Summon Call, int Destination);

    class Elevator
    {
        public int CurrentFloor { get; private set; } = 0;
        public Direction Direction { get; private set; } = Direction.Up;
        private List<Request> _requests = new();

        public void AddRequest(Request req) => _requests.Add(req);

        public List<string> Run()
        {
            var log = new List<string>();

            while (_requests.Any())
            {
                // Valid pickups in current direction
                var pickups = _requests
                    .Where(r => r.Call.Direction == Direction &&
                                ((Direction == Direction.Up && r.Call.Floor > CurrentFloor) ||
                                 (Direction == Direction.Down && r.Call.Floor < CurrentFloor)))
                    .Select(r => r.Call.Floor);

                // Dropoffs in current direction
                var dropoffs = _requests
                    .Where(r => (Direction == Direction.Up && r.Destination > CurrentFloor) ||
                                (Direction == Direction.Down && r.Destination < CurrentFloor))
                    .Select(r => r.Destination);

                // Floors to visit in current direction (LOOK)
                var stops = pickups
                    .Concat(dropoffs)
                    .Distinct()
                    .OrderBy(f => f * (Direction == Direction.Up ? 1 : -1))
                    .ToList();

                if (!stops.Any())
                {
                    // Reverse direction if nothing to do
                    Direction = Direction == Direction.Up ? Direction.Down : Direction.Up;
                    continue;
                }

                foreach (var floor in stops)
                {
                    if (floor != CurrentFloor)
                    {
                        log.Add($"Moving from {CurrentFloor} to {floor}");
                        CurrentFloor = floor;
                    }

                    var pickupsHere = _requests
                        .Where(r => r.Call.Floor == floor && r.Call.Direction == Direction)
                        .ToList();

                    var dropoffsHere = _requests
                        .Where(r => r.Destination == floor)
                        .ToList();

                    if (pickupsHere.Any() || dropoffsHere.Any())
                    {
                        var msg = $"Doors open at {floor} - ";
                        if (pickupsHere.Any()) msg += $"Picked up {pickupsHere.Count}. ";
                        if (dropoffsHere.Any()) msg += $"Dropped off {dropoffsHere.Count}.";
                        log.Add(msg.Trim());

                        _requests.RemoveAll(r =>
                            (r.Call.Floor == floor && r.Call.Direction == Direction) ||
                            r.Destination == floor
                        );
                    }
                }
            }

            return log;
        }
    }
    class Program
    {
        static void Main()
        {
            var elevator = new Elevator();

            while (true)
            {
                Console.Write("Add request? (y/n): ");
                var input = Console.ReadLine()?.Trim().ToLower();
                if (input != "y") break;

                try
                {
                    //External Request
                    Console.Write("Summon floor (0-10): ");
                    int from = int.Parse(Console.ReadLine());

                    Console.Write("Direction (up/down): ");
                    var dirInput = Console.ReadLine()?.Trim().ToLower();
                    var dir = dirInput == "up" ? Direction.Up : Direction.Down;

                    // Internal Request/From Lift
                    Console.Write("Destination floor (0-10): ");
                    int to = int.Parse(Console.ReadLine());

                    var request = new Request(new Summon(from, dir), to);
                    elevator.AddRequest(request);
                }
                catch
                {
                    Console.WriteLine("Invalid input — try again.");
                }
            }

            Console.WriteLine("\nElevator starting...\n");

            foreach (var entry in elevator.Run())
            {
                Console.WriteLine(entry);
            }

            Console.WriteLine("\nAll requests served.");
        }
    }
}
