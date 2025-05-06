using Mango.Services.OrderAPI.Models.Dtos;

namespace Mango.Services.OrderAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
