import Config from "./Config";

const baseurl = Config?.backendUrl + 'api'

const getUrl = function (controller: string, action: string, routeParams: any = null): string {

    let url = `${baseurl}/${controller}/${action}`;

    if (routeParams) {
        url += '?' + new URLSearchParams(routeParams);
    }

    return url;
}

// const addUrl  = function (controller : string, action : string, args: any | undefined = null) {
//     let url = getUrl(controller, action, args);

//     if (!urls[controller]) {
//         urls[controller] = { };
//     }

//     urls[controller][action] = url;
// }
let myaccount = 'myAccount';
let projectmanage = 'projectmanage';
let integration = 'integration';
let admin = 'admin'
let auth = 'auth';
let home = 'home';


let urls = {
    home: {
        GetProjectDocsVersions: getUrl(home, 'GetProjectDocsVersions'),
        GetRecentProjects: getUrl(home, 'GetRecentProjects'),
        GetAllProjects: getUrl(home, 'GetAllProjects'),
        TryItCreateProject: getUrl(home, 'TryItCreateProject'),
        GetVersionInfo: getUrl(home, 'GetVersionInfo')
    },
    myAccount: {
        GetGithubRepositories: getUrl(myaccount, 'GetGithubRepositories'),
        getAccountDetails: getUrl(myaccount, 'getAccountDetails'),
        getmyprojects: getUrl(myaccount, 'getMyProjects'),
        GetSystemMessages: getUrl(myaccount, 'GetSystemMessages'),
        GetBgJob: getUrl(myaccount, 'GetBgJob')
    },
    projectmanage: {
        DeleteProjectVersioning: getUrl(projectmanage, 'DeleteProjectVersioning'), 
        CreateProjectVersionByGitTag: getUrl(projectmanage, 'CreateProjectVersionByGitTag'), 
        GetProjectsVersioningInfo: getUrl(projectmanage, 'GetProjectsVersioningInfo'), 
        GetVersioningGitTags: getUrl(projectmanage, 'GetVersioningGitTags'),
        GetProjectVersioningById: getUrl(projectmanage, 'GetProjectVersioningById'),
        GetAllProjectVersioning: getUrl(projectmanage, 'GetAllProjectVersioning'),
        CreateProjectVersion: getUrl(projectmanage, 'CreateProjectVersion'),
        CreateProjectVersioning: getUrl(projectmanage, 'CreateProjectVersioning'),
        RequestProject: getUrl(projectmanage, 'RequestProject'),
        DeleteProject: getUrl(projectmanage, 'DeleteProject'),
        GetProjectById: getUrl(projectmanage, 'GetProjectById'),
        RequestAutoupgrade: getUrl(projectmanage, 'RequestAutoupgrade'),
        DocfxGetDocfxProjectDetails: getUrl(projectmanage, 'DocfxGetDocfxProjectDetails'),
        DocfxCreateFile: getUrl(projectmanage, 'DocfxCreateFile'),
        GetDocfxContentItems: (projectid: number) => getUrl(projectmanage, 'GetDocfxContentItems', { projectid: projectid }),
        DocfxGetFileContent: (projectid: number, vpath: string) => getUrl(projectmanage, 'DocfxGetFileContent', { projectid: projectid, vpath: vpath }),
        DocfxUpdateFile: getUrl(projectmanage, 'DocfxUpdateFile'),
        DocfxMoveFile: getUrl(projectmanage, 'DocfxMoveFile'),
        DocfxCreateDirectory: getUrl(projectmanage, 'DocfxCreateDirectory'),
        DocfxMoveDirectory: getUrl(projectmanage, 'DocfxMoveDirectory'),
        DocfxUploadFile: getUrl(projectmanage, 'DocfxUploadFile'),
        DocfxRemoveFile: getUrl(projectmanage, 'DocfxRemoveFile'),
        DocfxRemoveDirectory: getUrl(projectmanage, 'DocfxRemoveDirectory'),
    },
    integration: {
        NuGetCreateProject: getUrl(integration, 'NuGetCreateProject'),
        NugetCreateProjectCheckStatus: getUrl(integration, 'NugetCreateProjectCheckStatus')
    },
    auth: {
        adminLogin: getUrl(auth, 'adminLogin'),
        CallbackGithubOAuth: getUrl(auth, 'CallbackGithubOAuth'),
        Logout: getUrl(auth, 'Logout')
    },
    admin: {
        // BackgroundWorkerRebuildAllProjects: getUrl(admin, 'BackgroundWorkerRebuildAllProjects'),
        DoBackgroundWorkNow: getUrl(admin, 'DoBackgroundWorkNow'),
        executeRawSql: getUrl(admin, 'ExecuteRawSql'),
        getDashboardInfo: getUrl(admin, 'GetDashboardInfo'),
        getProjectFiles: (id: number) => getUrl(admin, 'GetProjectFiles', { id: id }),
        getTableData: getUrl(admin, 'GetTableData'),
        HardRecreateProject: getUrl(admin, 'HardRecreateProject')
    }
}


// addUrl(myaccount, 'requestProject');
// addUrl(myaccount, 'getAccountDetails');
// addUrl(myaccount, 'getmyprojects');

// addUrl(projectmanage, 'docfxgetdocfxinfo')


// addUrl(auth, 'adminLogin');

export default urls;