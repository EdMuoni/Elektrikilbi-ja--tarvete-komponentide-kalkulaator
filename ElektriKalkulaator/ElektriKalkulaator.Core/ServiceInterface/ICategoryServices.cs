using ElektriKalkulaator.Core.Domain;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface ICategoryServices
    {
        Task<IEnumerable<ProductCategory>> GetAll();
        Task<ProductCategory> GetById(Guid id);
        Task<ProductCategory> Create(ProductCategory category);
        Task<ProductCategory> Delete(Guid id);
    }
}
