using MediatR;
using mvmclean.backend.Domain.Aggregates.SeoPage;

namespace mvmclean.backend.Application.Features.SeoPage.Queries;

public class GetAllSeoPagesRequest : IRequest<GetAllSeoPagesResponse>
{
}

public class GetAllSeoPagesResponse
{
    public List<SeoPageDto> Pages { get; set; } = new();
}

public class SeoPageDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; }
    public string City { get; set; }
    public string? Area { get; set; }
    public string? ServiceType { get; set; }
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public int ContentBlocksCount { get; set; }
    public int FAQsCount { get; set; }
    public int KeywordsCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string DisplayName => !string.IsNullOrEmpty(Area)
        ? $"{ServiceType ?? "Services"} in {Area}, {City}"
        : $"{ServiceType ?? "Services"} in {City}";
}

public class GetAllSeoPagesHandler : IRequestHandler<GetAllSeoPagesRequest, GetAllSeoPagesResponse>
{
    private readonly ISeoPageRepository _seoPageRepository;

    public GetAllSeoPagesHandler(ISeoPageRepository seoPageRepository)
    {
        _seoPageRepository = seoPageRepository;
    }

    public async Task<GetAllSeoPagesResponse> Handle(GetAllSeoPagesRequest request, CancellationToken cancellationToken)
    {
        var pages = await _seoPageRepository.GetList(
            predicate: p => true,
            noTracking: true,
            orderBy: q => q.OrderByDescending(p => p.CreatedAt),
            p => p.Areas,
            p => p.Services,
            p => p.Keywords
        );

        var pageDtos = pages
            .Select(p => new SeoPageDto
            {
                Id = p.Id,
                Slug = p.Slug,
                City = p.City,
                Area = p.GetFirstAreaName(),
                ServiceType = p.GetFirstServiceName(),
                MetaTitle = p.MetaTitle,
                MetaDescription = p.MetaDescription,
                ContentBlocksCount = 0,
                FAQsCount = 0,
                KeywordsCount = p.Keywords.Count,
                CreatedAt = p.CreatedAt,
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return new GetAllSeoPagesResponse { Pages = pageDtos };
    }
}
