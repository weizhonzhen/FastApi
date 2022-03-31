namespace Fast.Api.Blazor.Service
{
    public interface IMapService
    {
        Task<Dictionary<string, object>> XmlSaveAsyn(object name, object xml);

        Task<Dictionary<string, object>> XmlDelAsyn(object name);
    }
}
