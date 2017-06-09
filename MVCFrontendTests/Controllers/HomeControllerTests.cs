using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLogWrapper;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers.Tests
{
    [TestClass()]
    public class HomeControllerTests
    {
        [TestMethod()]
        public void IndexAuthenticatedTest()
        {
            Mock<ClaimsPrincipal> principalMock;
            Mock<MakeStaticsMockable> staticsMock;
            HomeController hc = MockHomeController(out principalMock, out staticsMock);

            principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true);

            ViewResult result = hc.Index() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewBag.Message == "You are logged on.");
        }

        [TestMethod()]
        public void IndexAnonymousTest()
        {
            Mock<ClaimsPrincipal> principalMock;
            Mock<MakeStaticsMockable> staticsMock;
            HomeController hc = MockHomeController(out principalMock, out staticsMock);

            principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(false);

            ViewResult result = hc.Index() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewBag.Message == "You are not logged on.");
        }
 

        [TestMethod()]
        public void DoNotSignoutWhenNotAuthenticatedTest()
        {
            Mock<ClaimsPrincipal> principalMock;
            Mock<MakeStaticsMockable> staticsMock;
            HomeController hc = MockHomeController(out principalMock, out staticsMock);

            principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(false);

            var result = hc.Logout() as RedirectResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Url == "/");
        }

        [TestMethod()]
        public void LogOutWhenAuthenticatedTest()
        {
            // find a way to mock request.getOwinContext().Authentication.Signout()
            // getOwinContext is extention: static
            // http://adventuresdotnet.blogspot.nl/2011/03/mocking-static-methods-for-unit-testing.html

            Mock<ClaimsPrincipal> principalMock;
            Mock<MakeStaticsMockable> staticsMock;
            HomeController hc = MockHomeController(out principalMock, out staticsMock);
            principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true);

            var result = hc.Logout() as RedirectResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Url == "/");
            // expect that mocked signout gets called once
            staticsMock.Verify(sm => sm.SignOut(It.IsAny<HttpRequestBase>()), Times.Exactly(1));
        }

        [TestMethod()]
        public void AboutTest()
        {
            Mock<ClaimsPrincipal> cpMock;
            Mock<MakeStaticsMockable> staticsMock;
            HomeController hc = MockHomeController(out cpMock, out staticsMock);

            cpMock.Setup(p => p.Identity.IsAuthenticated).Returns(true);
            var claimList = new List<Claim> {
                new Claim ("auth_time","1496914632"),
                new Claim ("access_token","eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IjFqNV9UQnJEMmxpR002VGVES0ZWTlhqenpWcyIsImtpZCI6IjFqNV9UQnJEMmxpR002VGVES0ZWTlhqenpWcyJ9.eyJpc3MiOiJodHRwczovL2xvY2FsLmlkZW50aXR5c2VydmVyOjUwMDAiLCJhdWQiOiJodHRwczovL2xvY2FsLmlkZW50aXR5c2VydmVyOjUwMDAvcmVzb3VyY2VzIiwiZXhwIjoxNDk2OTE0NjMyLCJuYmYiOjE0OTY5MTQ2MjIsImNsaWVudF9pZCI6ImRldi1odW1hbiIsInNjb3BlIjpbIm9wZW5pZCIsInJvbGVzIiwibXZjLWZyb250ZW5kLWh1bWFuIl0sInN1YiI6ImFkbWluQG1lc3NhZ2VxdWV1ZWZyb250ZW5kLmNvbSIsImF1dGhfdGltZSI6MTQ5NjkxNDYyMiwiaWRwIjoiaWRzcnYiLCJhbXIiOlsicGFzc3dvcmQiXX0.k-FIoNDix3SX4O0-vNP7aDrYz1MI-zaCL_BgZBp1lBORx50y6s0uE0i8b44NBo2OX0cLQExP9lM3L0D_SKx1-iqKfxJuZeg6PjyK0upAwEvJ7jtBpE6_OdhJLW09ckS0cJWw9wGhC8496mbh9t8MLG-HjZAZHgrwmPr1k7BKKmr6N4sy6H8CYHsS4avbielq465beUiMdZrtmZZ2CrOcjiAHpAMWTuoBm9RPtz-rUq-MN0vVeCTq5GxpCYsST-tZHfKu0D7kklg66wnHEItw9EpgZKK0zWYsdT_okpEYnWQV_e2MhltC-Sz1BbgcdFxhQltxm-tt-0xUFVq_QZM-3Q,"),
                };
            cpMock.SetupGet(p => p.Claims).Returns(claimList);

            ViewResult result = hc.About() as ViewResult;
            Assert.IsNotNull(result);

            var abouModel = result.Model as Models.AboutModel;
            Assert.AreSame(abouModel.Claims, claimList);
            // Claim Setup: auth_time == exp from access_token
            Assert.IsTrue(abouModel.TokenSessionStart == abouModel.TokenSessionEnd);
        }

        private static HomeController MockHomeController( out Mock<ClaimsPrincipal> principalMock, out Mock<MakeStaticsMockable> staticsMock)
        {

            var loggerMock = new Mock<ILogger>();
            var requestMock = new Mock<HttpRequestBase>();
            var SmOutvar = new Mock<MakeStaticsMockable>();
            SmOutvar.Setup(sm => sm.SignOut(requestMock.Object));

            var homeController = new HomeController(loggerMock.Object, SmOutvar.Object);


            var CpOutvar = new Mock<ClaimsPrincipal>();
            var contextBaseMock = new Mock<HttpContextBase>();
            contextBaseMock.Setup(cttx => cttx.User).Returns(CpOutvar.Object);
            var sessionMock = new Mock<HttpSessionStateBase>();
            contextBaseMock.Setup(cttx => cttx.Session).Returns(sessionMock.Object);

            // Cannot mock request.getOwinContext like this, it is static extention
            //var requestMock = new Mock<HttpRequestBase>();
            //requestMock.Setup(rm=>rm.get)
            //contextBaseMock.Setup(cttx => cttx.Request).Returns(requestMock.Object);

            var controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Setup(con => con.HttpContext).Returns(contextBaseMock.Object);

            homeController.ControllerContext = controllerContextMock.Object;

            principalMock = CpOutvar;
            staticsMock = SmOutvar;

            return homeController;
        }
    }
}