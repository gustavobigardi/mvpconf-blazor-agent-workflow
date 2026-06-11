namespace MultiAgentWorkflow.Core;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public string Mode { get; set; } = "Simulated";
    public string? ProjectEndpoint { get; set; }
    public string? OpenAIEndpoint { get; set; }
    public string? ModelDeploymentName { get; set; }
    public string? AgentName { get; set; }
    public string? ApiKey { get; set; }
    public int MaxOutputTokens { get; set; } = 420;
    public float Temperature { get; set; } = 0f;

    public bool IsConfigured =>
        string.Equals(Mode, "Foundry", StringComparison.OrdinalIgnoreCase)
        && (!string.IsNullOrWhiteSpace(ProjectEndpoint) || !string.IsNullOrWhiteSpace(OpenAIEndpoint))
        && !string.IsNullOrWhiteSpace(ModelDeploymentName);

    public IReadOnlyList<string> MissingValues()
    {
        var missing = new List<string>();
        if (!string.Equals(Mode, "Foundry", StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Foundry:Mode=Foundry");
        }

        if (string.IsNullOrWhiteSpace(ProjectEndpoint) && string.IsNullOrWhiteSpace(OpenAIEndpoint))
        {
            missing.Add("Foundry:ProjectEndpoint ou Foundry:OpenAIEndpoint");
        }

        if (string.IsNullOrWhiteSpace(ModelDeploymentName))
        {
            missing.Add("Foundry:ModelDeploymentName");
        }

        return missing;
    }

    public Uri ResolveOpenAIEndpoint()
    {
        var configured = !string.IsNullOrWhiteSpace(OpenAIEndpoint)
            ? OpenAIEndpoint
            : ProjectEndpoint;

        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("Configure Foundry:ProjectEndpoint ou Foundry:OpenAIEndpoint.");
        }

        var uri = new Uri(configured, UriKind.Absolute);
        var builder = new UriBuilder(uri)
        {
            Path = NormalizeOpenAIPath(uri.AbsolutePath),
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri;
    }

    private static string NormalizeOpenAIPath(string path)
    {
        var normalized = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if (normalized.Contains("/openai/v1", StringComparison.OrdinalIgnoreCase))
        {
            return $"{normalized.TrimEnd('/')}/";
        }

        if (normalized.Contains("/api/projects/", StringComparison.OrdinalIgnoreCase))
        {
            return "/openai/v1/";
        }

        return $"{normalized.TrimEnd('/')}/openai/v1/";
    }
}
