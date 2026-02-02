namespace EComAPI.Application.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand
    {
        public string Name { get; }
        public string Slug { get; }
        public Guid? ParentId { get; }

        public CreateCategoryCommand(
            string name,
            string slug,
            Guid? parentId)
        {
            Name = name;
            Slug = slug;
            ParentId = parentId;
        }
    }
}