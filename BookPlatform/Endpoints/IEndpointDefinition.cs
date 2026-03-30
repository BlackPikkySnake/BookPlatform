namespace BookPlatform.Endpoints
{
    // Базовый интерфейс для всех групп конечных точек
    public interface IEndpointDefinition
    {
        void MapEndpoints(WebApplication app);
    }
}