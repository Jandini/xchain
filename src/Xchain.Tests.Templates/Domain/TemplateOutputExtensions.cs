namespace Xchain.Tests.Templates;

// Extension methods for ClientId<T>(), ProjectId<T>(), ImportId<T>() are generated
// by ChainOutputSchemaGenerator — see ChainOutputSchema_ITemplateOutputs.g.cs in obj/
[ChainOutputSchema]
public interface ITemplateOutputs
{
    Guid   ClientId   { get; }
    string ProjectId  { get; }
    string ImportId   { get; }
    string ImportName { get; }
}
