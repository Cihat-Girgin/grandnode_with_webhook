﻿using Grand.Infrastructure;
using Grand.Web.Common.Components;
using Grand.Web.Features.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components;

public class CategoryFeaturedProductsViewComponent : BaseViewComponent
{
    #region Constructors

    public CategoryFeaturedProductsViewComponent(
        IMediator mediator,
        IWorkContext workContext)
    {
        _mediator = mediator;
        _workContext = workContext;
    }

    #endregion

    #region Invoker

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = await _mediator.Send(new GetCategoryFeaturedProducts {
            Customer = _workContext.CurrentCustomer,
            Language = _workContext.WorkingLanguage,
            Store = _workContext.CurrentStore
        });

        return !model.Any() ? Content("") : View(model);
    }

    #endregion

    #region Fields

    private readonly IMediator _mediator;
    private readonly IWorkContext _workContext;

    #endregion
}