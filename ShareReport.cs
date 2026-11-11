static class ShareReport
{
    public static void Run(string[] args)
    {
        if (args.Length != 2)
            throw new FormatException("Usage: share study.tsv");
        var areas = new Dictionary<(string Area, string Service), (long Demand, long Seats)>();
        var links = new Dictionary<(string From, string To, string Service), long>();
        foreach (var r in Rows(args[1]))
        {
            if (r is ["area", var site, var service, var demand, var seats])
                Require(areas.TryAdd((site, service), (Integer(demand), Integer(seats))), "Duplicate area/service.");
            else if (r is ["link", var from, var to, var svc, var cap])
                Require(from != to && links.TryAdd((from, to, svc), Integer(cap)), "Invalid or duplicate link.");
            else
                throw new FormatException("Expected area or link.");
        }

        Require(areas.Count > 0 && links.Keys.All(l => areas.ContainsKey((l.From, l.Service)) && areas.ContainsKey((l.To, l.Service))), "Every link needs both areas in the same service.");
        foreach (var svc in areas.Keys.Select(k => k.Service).Distinct())
        {
            var names = areas.Keys.Where(k => k.Service == svc).Select(k => k.Area).Order().ToArray();
            int n = names.Length, src = 2 * n, sink = src + 1;
            var capacities = new long[sink + 1, sink + 1];
            long local = 0;
            for (int i = 0; i < n; i++)
            {
                var a = areas[(names[i], svc)];
                var covered = Math.Min(a.Demand, a.Seats);
                local = checked(local + covered);
                capacities[src, i] = a.Demand - covered;
                capacities[n + i, sink] = a.Seats - covered;
            }

            foreach (var(key, cap)in links.Where(l => l.Key.Service == svc))
                capacities[Array.IndexOf(names, key.From), n + Array.IndexOf(names, key.To)] = cap;
            var residual = (long[, ])capacities.Clone();
            long extra = 0;
            while (true)
            {
                var prev = Enumerable.Repeat(-1, sink + 1).ToArray();
                prev[src] = src;
                var q = new Queue<int>();
                q.Enqueue(src);
                while (q.TryDequeue(out int u) && prev[sink] < 0)
                    for (int v = 0; v <= sink; v++)
                        if (prev[v] < 0 && residual[u, v] > 0)
                        {
                            prev[v] = u;
                            q.Enqueue(v);
                        }

                if (prev[sink] < 0)
                    break;
                long flow = long.MaxValue;
                for (int v = sink; v != src; v = prev[v])
                    flow = Math.Min(flow, residual[prev[v], v]);
                for (int v = sink; v != src; v = prev[v])
                {
                    residual[prev[v], v] -= flow;
                    residual[v, prev[v]] += flow;
                }

                extra = checked(extra + flow);
            }

            Console.WriteLine($"{svc}: {local} local potential coverage, {extra} additional with sharing.");
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($"  {names[i]}: {residual[src, i]} requests still uncovered");
                for (int j = 0; j < n; j++)
                    if (capacities[i, n + j] > residual[i, n + j])
                        Console.WriteLine($"  {names[i]} -> {names[j]}: {capacities[i, n + j] - residual[i, n + j]} slots");
            }
        }
    }

    static List<string[]> Rows(string file, int width = 0, bool allowEmpty = false)
    {
        using var reader = file == "-" ? Console.In : File.OpenText(file);
        var rows = new List<string[]>();
        int line = 0;
        while (reader.ReadLine()is string s)
        {
            line++;
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#"))
                continue;
            var cells = s.Split('\t', StringSplitOptions.TrimEntries);
            if ((width > 0 && cells.Length != width) || (!allowEmpty && cells.Any(string.IsNullOrEmpty)))
                throw new FormatException($"Invalid columns on line {line}.");
            rows.Add(cells);
        }

        if (rows.Count == 0)
            throw new FormatException("Empty input.");
        return rows;
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new FormatException(message);
    }

    static long Integer(string text)
    {
        var value = long.Parse(text);
        Require(value >= 0, "Count must be nonnegative.");
        return value;
    }
}
