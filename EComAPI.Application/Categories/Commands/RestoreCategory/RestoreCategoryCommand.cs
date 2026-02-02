namespace EComAPI.Application.Categories.Commands.RestoreCategory
{
    public class RestoreCategoryCommand
    {
        public Guid Id { get; }

        public RestoreCategoryCommand(Guid id)
        {
            Id = id;
        }
    }
}