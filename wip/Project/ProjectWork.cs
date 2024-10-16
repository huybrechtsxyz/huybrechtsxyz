namespace Huybrechts.Core.Project

public record ProjectWork: Entity, IEntity
{
    public string Title { get; set; }

    public string Description { get; set; }


}