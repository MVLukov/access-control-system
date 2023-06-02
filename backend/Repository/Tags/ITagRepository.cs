using ACS.Models;

namespace ACS.Repository
{
    public interface ITagRepository
    {
        ResultStatus AuthorizeTag(string tagId, string deviceId);
    }
}