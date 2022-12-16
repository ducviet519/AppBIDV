using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using System;

namespace AppBIDV.Authorizations
{
    public class ControllerModelConvention : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel model)
        {
            foreach (var actionModel in model.Actions)
                actionModel.Filters.Add(new AuthorizeFilter($"Permission.{actionModel.Controller}.{actionModel.ActionName}"));
        }
    }
}
