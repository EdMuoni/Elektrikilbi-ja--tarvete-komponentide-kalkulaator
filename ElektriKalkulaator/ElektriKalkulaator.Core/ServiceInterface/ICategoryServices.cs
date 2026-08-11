using ElektriKalkulaator.Core.Domain;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface ICategoryServices
    {
        Task<IEnumerable<ProductCategory>> GetAll();
        Task<ProductCategory?> GetById(Guid id);
        Task<ProductCategory> Create(ProductCategory category);

        // Returns which of three things happened rather than throwing — see CategoryDeleteResult.
        Task<CategoryDeleteResult> Delete(Guid id);
    }
}
