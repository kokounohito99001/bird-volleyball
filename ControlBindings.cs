using System.Text.Json;

namespace BirdVolleyball;

public enum ControlAction
{
    MoveLeft,
    MoveRight,
    Bump,
    Smash,
    ResetRound
}

public sealed class ControlBindings
{
    private readonly Dictionary<ControlAction, HashSet<Keys>> bindings;

    private ControlBindings(Dictionary<ControlAction, HashSet<Keys>> bindings)
    {
        this.bindings = bindings;
    }

    public static ControlBindings Load(string path)
    {
        if (!File.Exists(path))
        {
            return CreateDefault();
        }

        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<ControlsFile>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return FromConfig(config) ?? CreateDefault();
        }
        catch
        {
            return CreateDefault();
        }
    }

    public bool IsPressed(ControlAction action, HashSet<Keys> pressedKeys)
    {
        return bindings.TryGetValue(action, out var keys) && keys.Overlaps(pressedKeys);
    }

    private static ControlBindings? FromConfig(ControlsFile? config)
    {
        if (config is null)
        {
            return null;
        }

        var result = new Dictionary<ControlAction, HashSet<Keys>>();
        foreach (var pair in new Dictionary<ControlAction, string[]?>
        {
            [ControlAction.MoveLeft] = config.MoveLeft,
            [ControlAction.MoveRight] = config.MoveRight,
            [ControlAction.Bump] = config.Bump,
            [ControlAction.Smash] = config.Smash,
            [ControlAction.ResetRound] = config.ResetRound
        })
        {
            var mapped = new HashSet<Keys>();
            if (pair.Value is not null)
            {
                foreach (var key in pair.Value)
                {
                    if (Enum.TryParse<Keys>(key, ignoreCase: true, out var parsed))
                    {
                        mapped.Add(parsed);
                    }
                }
            }

            result[pair.Key] = mapped;
        }

        return new ControlBindings(result);
    }

    private static ControlBindings CreateDefault()
    {
        return new ControlBindings(new Dictionary<ControlAction, HashSet<Keys>>
        {
            [ControlAction.MoveLeft] = [Keys.Left, Keys.A],
            [ControlAction.MoveRight] = [Keys.Right, Keys.D],
            [ControlAction.Bump] = [Keys.Space],
            [ControlAction.Smash] = [Keys.ShiftKey],
            [ControlAction.ResetRound] = [Keys.R]
        });
    }

    private sealed class ControlsFile
    {
        public string[]? MoveLeft { get; set; }

        public string[]? MoveRight { get; set; }

        public string[]? Bump { get; set; }

        public string[]? Smash { get; set; }

        public string[]? ResetRound { get; set; }
    }
}
