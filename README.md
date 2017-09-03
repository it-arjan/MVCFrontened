# MVCFrontend

This repository holds MVC frontend to shoot a message in WebEntrypoint and review the results. Authenticating using OAUTH against identityserver3. 

To see how MVCFrontend fits in the overall system, see https://messagequeuefrontend.azurewebsites.net/systemlayout

# Most interesting

### /Views/SendMessage.cshtml
- does ajax calls (get partial) to backend, as wel as CORS (message drop)
- Ajax code is a mix of js/JQuery)
- All 3 expiration counters in pure JS

### /overrides Folder
- CustomControllerFactory, injects logger and wrapper object for statics, for unit test. Controller not found is handled as 404, but custom error page is set using IIS httpErrors
- 2 Filters
- HandleErrorAttribute is currently not used, errors are mostly just logged & marked as handled and bubbled up. HttpStatus code is set manually when needed and Custom error page is served using IIIS httpErrors tag
- LogRequestsAttribute makes requests to a tagged controller being logged in data Api

### /Controllers
- MessageController most importantly registers the socket-access token
- MyBaseController holds the logger, and a basic onerror for all controllers
- ServiceSelectionController communicates with webentrypoint and serves a partial with the result
- PostbackDatasController, RequestLogEntriesController interact async with Data Api

### /Startup.cs
- Owin code dealing with OAUTH
- setting the different claims on auth success

### /Helpers/SettingsChecker
- Auto-checks on Application_Startup if all registered settings are present in Web.config, if not .. exception


