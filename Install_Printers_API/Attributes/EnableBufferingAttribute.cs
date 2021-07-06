using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Install_Printers_API.Attributes
{
    public class EnableBufferingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        { context.HttpContext.Request.EnableBuffering(); }
        public void OnResourceExecuted(ResourceExecutedContext context) { }
    }
}
