import Config from "./Config";

const account = 'account';
const project = 'project';
const home = 'home';

let robiniadocsDocfx = Config?.backendUrl + 'd';

function bgWaitingUrl(type: number, bgjobid: any): string {
    return `/${account}/bgjob-waiting?type=${type}&bgjobid=${bgjobid}`
}

const Urls = {
    i: {
        nuget: (packageName: string, packageVersion: string) => `/i/nuget/${packageName}/${packageVersion}`
    },
    other: {
        help: '/help',
        appGithubProjectRepository: 'https://github.com/NeuroXiq/DNDocs',
        cookiesConsentHtml: `${robiniadocsDocfx}/cookies-consent.html`
    },
    robiniadocsUrl: function (urlPrefix: any) { return `${robiniadocsDocfx}/${urlPrefix}/` },
    account: {
        login: (returnUrl : string = '/') => `/${account}/login?return=${returnUrl}`,
        details: `/${account}/details`,
        bgjobWaiting: (projectId: number) => `/${account}/bgjob-waiting?projectid=${projectId}`
    },
    project: {
        createProject: `/${project}/create-project`,
        details: (id : number) => `/${project}/${id}/details`,
        docfxFilesExplorer: (id : number) => `/${project}/${id}/docfx-files-explorer`,
        docfxEditFile: (id : number) => `/${project}/${id}/docfx-edit-file`,
        delete: (id: number) => `/${project}/${id}/delete`,
        createVersioning: `/${project}/create-versioning`,
        createProjectVersion: (versioningId: number) => `/${project}/create-project-version/${versioningId}`,
        versioningDetails: (versioningId: number) => `/${project}/versioning-details/${versioningId}`
    },
    home: {
        index: `/${home}/`,
        howToUse: `/${home}/how-to-use`,
        termsOfService: `/${home}/terms-of-service`,
        projects: `/projects`,
        tryit: `${home}/try-it`
    }
};

export default Urls;