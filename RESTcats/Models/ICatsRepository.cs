
namespace RESTcats.Models
{
    public interface ICatsRepository
    {
        Cat AddCat(Cat cat);
        IEnumerable<Cat> GetAllCats(int? minimumweight =null, int? maximumweight = null, string? nameFilter = null);
        Cat? GetCatById(int id);
        Cat? RemoveCat(int id);
        Cat? UpdateCat(int id, Cat updatedCat);
    }
}