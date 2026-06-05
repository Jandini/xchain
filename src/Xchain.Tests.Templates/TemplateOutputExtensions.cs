namespace Xchain.Tests.Templates;

public static class TemplateOutputExtensions
{
    public static TestOutput<T, Guid> ClientId<T>(this TestChainOutput output) =>
        new(output, "ClientId");

    public static TestOutput<T, string> ProjectId<T>(this TestChainOutput output) =>
        new(output, "ProjectId");

    public static TestOutput<T, string> ImportId<T>(this TestChainOutput output) =>
        new(output, "ImportId");
}
