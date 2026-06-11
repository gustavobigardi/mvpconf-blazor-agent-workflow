namespace MultiAgentWorkflow.Core;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public string Mode { get; set; } = "Simulated";
    public string? ProjectEndpoint { get; set; }
    public string? ModelDeploymentName { get; set; }
    public string? AgentName { get; set; }

    public bool IsConfigured =>
        string.Equals(Mode, "Foundry", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(ProjectEndpoint)
        && !string.IsNullOrWhiteSpace(ModelDeploymentName);

    public IReadOnlyList<string> MissingValues()
    {
        var missing = new List<string>();
        if (!string.Equals(Mode, "Foundry", StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Foundry:Mode=Foundry");
        }

        if (string.IsNullOrWhiteSpace(ProjectEndpoint))
        {
            missing.Add("Foundry:ProjectEndpoint");
        }

        if (string.IsNullOrWhiteSpace(ModelDeploymentName))
        {
            missing.Add("Foundry:ModelDeploymentName");
        }

        return missing;
    }
}
