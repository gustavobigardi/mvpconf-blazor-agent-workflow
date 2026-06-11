using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Identity;

namespace MultiAgentWorkflow.Core;

public interface IAgentChatClient
{
    Task<string> CompleteAsync(
        string agentName,
        string responsibility,
        WorkflowContext context,
        string outputContract,
        CancellationToken cancellationToken);
}

public sealed class FoundryChatClient : IAgentChatClient
{
    private static readonly string[] TokenScopes = ["https://ai.azure.com/.default"];
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private readonly HttpClient _httpClient;
    private readonly FoundryOptions _options;
    private readonly TokenCredential _credential;
    private readonly Uri _chatCompletionsUri;

    public FoundryChatClient(FoundryOptions options, HttpClient? httpClient = null, TokenCredential? credential = null)
    {
        _options = options;
        _httpClient = httpClient ?? new HttpClient();
        _credential = credential ?? new DefaultAzureCredential();
        _chatCompletionsUri = new Uri(_options.ResolveOpenAIEndpoint(), "chat/completions");
    }

    public async Task<string> CompleteAsync(
        string agentName,
        string responsibility,
        WorkflowContext context,
        string outputContract,
        CancellationToken cancellationToken)
    {
        var content = await CompleteOnceAsync(
            agentName,
            responsibility,
            context,
            outputContract,
            repairInstruction: null,
            cancellationToken);

        if (IsUsablePortugueseOutput(content))
        {
            return NormalizeModelText(content);
        }

        var repaired = await CompleteOnceAsync(
            agentName,
            responsibility,
            context,
            outputContract,
            repairInstruction: $"""
            A resposta anterior foi rejeitada porque continha texto corrompido, idiomas misturados ou caracteres aleatorios.
            Refaça do zero. Nao tente aproveitar a resposta anterior.
            Use apenas portugues do Brasil, letras latinas, numeros e pontuacao comum.
            """,
            cancellationToken);

        if (IsUsablePortugueseOutput(repaired))
        {
            return NormalizeModelText(repaired);
        }

        throw new InvalidOperationException(
            "Foundry retornou texto corrompido duas vezes. Verifique se o deployment e de um modelo chat/instruct e reduza temperatura/tokens.");
    }

    private async Task<string> CompleteOnceAsync(
        string agentName,
        string responsibility,
        WorkflowContext context,
        string outputContract,
        string? repairInstruction,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _chatCompletionsUri)
        {
            Content = JsonContent.Create(new
            {
                model = _options.ModelDeploymentName,
                temperature = _options.Temperature,
                max_tokens = _options.MaxOutputTokens,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = BuildSystemPrompt(agentName, responsibility, outputContract)
                    },
                    new
                    {
                        role = "user",
                        content = BuildUserPrompt(context, repairInstruction)
                    }
                }
            })
        };

        await AuthorizeAsync(request, cancellationToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Foundry retornou {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        return NormalizeModelText(ExtractContent(body));
    }

    private async Task AuthorizeAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Add("api-key", _options.ApiKey);
            return;
        }

        var token = await _credential.GetTokenAsync(new TokenRequestContext(TokenScopes), cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    private static string BuildSystemPrompt(string agentName, string responsibility, string outputContract)
    {
        return $"""
        Voce e o agente {agentName} em um workflow multi-agent para triagem de incidente.
        Responsabilidade: {responsibility}

        Regras:
        - Responda em portugues do Brasil.
        - Seja objetivo, operacional e util para uma demo tecnica.
        - Nao invente metricas exatas; use sinais qualitativos quando necessario.
        - Respeite o contrato de saida.
        - Use no maximo 6 linhas.
        - Use apenas letras latinas, numeros, acentos do portugues e pontuacao comum.
        - Nao use ideogramas, cirilico, arabe, devanagari, coreano, tailandes, emojis, HTML, XML, JSON ou tokens aleatorios.
        - Se estiver incerto, responda com uma lista curta de acoes seguras.

        Contrato de saida:
        {outputContract}
        """;
    }

    private static string BuildUserPrompt(WorkflowContext context, string? repairInstruction)
    {
        return $"""
        {repairInstruction}

        Objetivo original:
        {context.Request.Goal}

        Publico:
        {context.Request.Audience}

        Plano atual:
        {ValueOrEmpty(context.Plan)}

        Evidencias atuais:
        {ValueOrEmpty(context.Evidence)}

        Execucao atual:
        {ValueOrEmpty(context.ExecutionNotes)}

        Revisao atual:
        {ValueOrEmpty(context.Review)}
        """;
    }

    private static string ValueOrEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "(ainda nao produzido)" : value;
    }

    private static string ExtractContent(string body)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var first = choices[0];
            if (first.TryGetProperty("message", out var message)
                && message.TryGetProperty("content", out var content))
            {
                return content.GetString()?.Trim() ?? string.Empty;
            }
        }

        if (root.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString()?.Trim() ?? string.Empty;
        }

        throw new InvalidOperationException("Resposta Foundry nao contem choices[0].message.content.");
    }

    private static string NormalizeModelText(string text)
    {
        var normalizedLines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => WhitespaceRegex.Replace(line.Trim(), " "))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(8);

        return string.Join(Environment.NewLine, normalizedLines).Trim();
    }

    public static bool IsUsablePortugueseOutput(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = NormalizeModelText(text);
        if (normalized.Length < 20 || normalized.Length > 2400)
        {
            return false;
        }

        var letters = 0;
        var disallowed = 0;
        var suspiciousRuns = 0;
        var currentSuspiciousRun = 0;

        foreach (var rune in normalized.EnumerateRunes())
        {
            var value = rune.Value;
            if (char.IsLetter((char)Math.Min(value, char.MaxValue)))
            {
                letters++;
            }

            var allowed = IsAllowedRune(value);
            if (!allowed)
            {
                disallowed++;
                currentSuspiciousRun++;
                if (currentSuspiciousRun >= 3)
                {
                    suspiciousRuns++;
                }
            }
            else
            {
                currentSuspiciousRun = 0;
            }
        }

        if (letters < 12)
        {
            return false;
        }

        var ratio = (double)disallowed / Math.Max(1, normalized.Length);
        return ratio <= 0.03 && suspiciousRuns == 0;
    }

    private static bool IsAllowedRune(int value)
    {
        if (value is >= 0x20 and <= 0x7E)
        {
            return true;
        }

        // Latin-1 Supplement and Latin Extended-A cover Portuguese accents and common copied punctuation.
        if (value is >= 0x00A0 and <= 0x017F)
        {
            return true;
        }

        return value is 0x2013 or 0x2014 or 0x2018 or 0x2019 or 0x201C or 0x201D or 0x2026;
    }
}
