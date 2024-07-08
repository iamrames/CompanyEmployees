namespace Shared.DataTransferObjects;

[Serializable]
public record EmployeeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Position { get; set; }
}
