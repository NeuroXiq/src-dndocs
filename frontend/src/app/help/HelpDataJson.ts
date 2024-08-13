const helpDataJson = [{
    id: "projectversioning",
    name: "Project Versioning",
    description: "Project Versioning",
    fields: [
        {
            id: "id",
            name: "Id",
            description: "Identifier"
        },
        {
            id: "autoupgrade",
            name: "Autoupgrade",
            description: "If true system from time to time will scan for new git tags and if needed automatically generate project documentation for new tags"
        },
        {
            id: "gitrepourl",
            name: "Git Repository Url",
            description: "Git Repository URL that contains folder with .MD docs"
        },
        {
            id: 'urlprefix',
            name: "Url Prefix",
            description: "Url prefix that will be used when creating new project."
        },
        {
            id: "gitbranchname",
            name: "Git Branch Name",
            description: "Git Branch name to checkout when preparing .MD docs for build (in most cases it will be main branch like 'master' or'main')"
        },
        {
            id: "gitdocspath",
            name: "Git Docs Path",
            description: "Relative path (relative in git repository) of directory where .MD docs are stored. Empty if there is no .MD docs"
        },
        {
            id: "gitreadmepath",
            name: "Git Readme Path",
            description: "Relative path (relative in git repositroy) of README.md file. Empty if there is no relative path"
        },
        {
            id: "projectname",
            name: "Projetc Name",
            description: "Name of the project"
        },
        {
            id: "projectwebsiteurl",
            name: "Project Website Url",
            description: "Project Website"
        },
        {
            id: "nugetpackages",
            name: "Nuget Packages",
            description: "Nuget Packages used to create new versions. When creating new version, system will try to find nuget packages matching current git tag"
        },
        {
            id: "alwaysNewestDocsUrl",
            name: "Always-Newest Docs URL",
            description: "This link never changes and always points to the newest docs of all versions. Can be used as link for documentation (e.g. in README.md)"
        },
    ]
    }, {
    id: "howToHost",
    name: "Host API Explorer on DNDocs in 2 minutes",
    description: "Instruction how host API Explorer on DNDocs",
    fields: [{
        id: "",
        name: "1. Login to DNDocs",
        description: "Open account menu (top right on the page) and click Login, then proceed with Your github account"
    }, {
        id: "",
        name: "2. Go to My Account",
        description: "Open account menu (top right on the page) and click My Account"
    }, {
        id: "",
        name: "3. Click Create Project",
        description: "Click Create Project button"
    }, {
        id: "",
        name: "4. Create project",
        description: "Fill all form fields and click Submit"
    }, {
        id: "",
        name: "5. Go back to My Account. Your project will be visible on the list",
        description: "Table shows two URLS, one is for ready-to-use DNDocs API Explorer and second to shows project details"
    }]
},{
    id: "myAccount",
    name: "My Account",
    description: "Account details",
    fields: [{
        id: "id",
        name: "Id",
        description: "Unique User Identifier in the system"
    }, {
        id: "email",
        name: "Email",
        description: "User Email"
    }, {
        id: "login",
        name: "Login",
        description: "Unique user login in the system"
    }
    ]
}, {
    id: "createProjectForm",
    name: "Create Project Form",
    description: "Form used to create new project. You can create a new project using *.dll and *.xml " +
        "from the disk or by Nuget Packages" + 
        "Creating a project can took 2 minutes",
    fields: []
}, {
    id: "project",
    name: "Project",
    description: "Manage Project",
    fields: [{
        id: "id",
        name: "Id",
        description: "Project identifier in the system "
    }, {
        id: "description",
        name: "Description",
        description: "Project description"
    }, {
        id: "name",
        name: "Name",
        description: "Project unique name"
    }, {
        id: "githubUrl",
        name: "Github Url",
        description: "Project repository URL"
    }, {
        id: "shieldsBadgeMd",
        name: "Shields.io Badge markdown",
        description: "Example generated shields.io badge ready to " +
            "copy & paste to README.md file. It is not " +
            "required to use exactly same badge as presented " +
            "and can be customized on shields.io if needed."
    }, {
        id: "nugetPackages",
        name: "Current Nuget Packages",
        description: "Current packages deployed and hosted."
    }, {
        id: "lastDocfxBuildTime",
        name: "Last Build Time",
        description: "Indicates time when project was build last."
    }, {
        id: "bgHealthCheckHttpGetDateTime",
        name: "Background Health Date",
        description: "System form time to time invokes HTTP GET " +
            "request to check if projects is online and can be browsed. " +
            "This date indicates when this action occured"
    }, {
        id: "bgHealthCheckHttpGetStatus",
        name: "Background Health Status",
        description: "If HTTP GET returns ok then value is OK. " +
            "Undefined means that system did not invoked HTTP GET " +
            "in background yet. You can check if project is available " +
            "just by opening Your project by url"
    }, {
        id: "urlPrefix",
        name: "Url Prefix",
        description: "This is unique part of project URL " +
            "used to serve project files by system"
    }, {
        id: "status",
        name: "Status",
        description: "Project status."
    }, {
        id: "docsUrl",
        name: "URL",
        description: "Ready-to-use API explorer that " +
            "can be shared with other people e.g. in README file " +
            "on github"
    }, {
        id: "autoRebuildLatestNupkgEnabled",
        name: "Auto rebuild nuget packages",
        description: "Indicates if system from time to time " + 
            "should scan for latest nuget packages assigned to project " +
            "and if there are newest fetch them and rebuild"
    }, {
        id: "autoRebuildLatestNupkgRebuildDatetime",
        name: "Auto Rebuild Laste NuPkg Reubuild Date Time",
        description: "Indicates last update and rebuild of nuget packages " + 
        "if newer that project references were found on nuget.org." + 
        "Empty value if this process did not occur yet or " + 
        "if auto-rebuild nuget packages is disabled",
    }]
}, {
    id: "systemMesssages",
    name: "System Messages",
    description: "System generates messages about occured actions, statuses, information with projects etc.",
    fields: [{
        id: "title",
        name: "Title",
        description: "Short title of the message"
    }, {
        id: "dateTime",
        name: "Date time",
        description: "Date when message was generated"
    }, {
        id: "message",
        name: "Message",
        description: "Long message description"
    }, {
        id: "level",
        name: "Level",
        description: "Message level. Levels: Information, Success, Warning, Error"
    }]
}];

export { helpDataJson };