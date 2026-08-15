using FluentValidation;

namespace VsaTemplate.Common.Pagination;

#if (IncludeComments)
/// <summary>
/// Reglas comunes de un request paginado (PageNumber >= 1, PageSize dentro de
/// un rango razonable). El Validator de cada caso de uso hereda de esta clase
/// y solo agrega las reglas propias de su Request.
/// </summary>
#endif
public abstract class PagedRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IPagedRequest
{
    protected PagedRequestValidator(int maxPageSize = 100)
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, maxPageSize);
    }
}
