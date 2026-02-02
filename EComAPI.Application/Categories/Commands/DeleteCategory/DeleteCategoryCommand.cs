namespace EComAPI.Application.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommand
    {
        public Guid Id { get; }

        public DeleteCategoryCommand(Guid id)
        {
            Id = id;
        }
    }
}