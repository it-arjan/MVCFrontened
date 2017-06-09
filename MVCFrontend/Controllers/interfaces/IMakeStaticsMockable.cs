using System.Web;

namespace MVCFrontend.Controllers
{
    public interface IMakeStaticsMockable
    {
        void SignOut(HttpRequestBase request);
    }
}