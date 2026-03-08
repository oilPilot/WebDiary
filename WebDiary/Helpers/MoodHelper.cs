
namespace WebDiary.Helpers;

public static class MoodHelper
{
    private static readonly Dictionary<string, int> MoodPositivity = new()
    {
        {"😁", 2}, {"🙂", 1}, {"😌", 1}, // Positive
        {"🤔", 0}, {"😐", 0}, {"😴", -1}, // Neutral
        {"😥", -1}, {"😳", -1}, {"😔", -1}, // Negative
        {"😤", -2}, {"😱", -2}, {"🤬", -3}, // VERY Negative
        {"(^▽^)", 1}, {"(づ｡ ◕‿‿◕｡) づ", 2}, {"<&lt;3", 2} // Kaomoji's
    };

    public static int GetMoodValue(string mood)
        => MoodPositivity.TryGetValue(mood, out var value) ? value : 0;
}