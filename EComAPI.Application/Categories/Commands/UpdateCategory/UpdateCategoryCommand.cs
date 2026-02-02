namespace EComAPI.Application.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommand
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Slug { get; }
        public Guid? ParentId { get; }

        public UpdateCategoryCommand(
            Guid id,
            string name,
            string slug,
            Guid? parentId)
        {
            Id = id;
            Name = name;
            Slug = slug;
            ParentId = parentId;
        }
    }
}