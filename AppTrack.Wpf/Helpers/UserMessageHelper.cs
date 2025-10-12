namespace AppTrack.WpfUi.Helpers;

public static class UserMessageHelper
{
    public static string GenerateUserMessageforUnusedKeys(List<string> unusedKeys)
    {
        if (unusedKeys.Count == 0)
        {
            return string.Empty;
        }

        return "The following keys were not used: " + string.Join(",", unusedKeys);
    }
}
