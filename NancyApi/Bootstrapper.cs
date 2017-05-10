using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;

namespace NancyApi
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        //check https://github.com/NancyFx/Nancy/wiki/The-Application-Before,-After-and-OnError-pipelines for a better to do this
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            StaticConfiguration.DisableErrorTraces = false;
        }
    }
}