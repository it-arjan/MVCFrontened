# MVCFrontened
This repository holds MVC frontend to shoot a message in WebEntrypoint and review the results. Authenticating against identityserver3. 
Uses websockets to display live feedback from system componenets like queue manager and data api.
The solution file also needs the data project from the sharedprojects repostitory, clone that in a folder "shared", next to mvcfrontend folder. See mvcfrontend solution.
Other componenets are two windows services: auth server (self-hosted identity server3) and webentrypoint (queue manager + self-hosted web api to drop the message into the message queue)
Follow this link for a complete system diagram. https://messagequeuefrontend.azurewebsites.net/systemlayout
