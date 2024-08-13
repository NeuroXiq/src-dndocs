import { helpDataJson } from "./HelpDataJson";

interface HelpSection {
    id: string,
    name: string,
    description: string,
    fields: Field[]
}

interface Field {
    id: string,
    name: string,
    description: string
}

function getField(fieldId: any) {
    if (!fieldId) {
        return null;
    }

    const result = helpDataJson
        .find(d => d.id === fieldId.sectionId)
        ?.fields.find(d => d.id === fieldId.fieldId);

    return result;
}

const FID = {
    projectVersioning: {
        id: { sectionId: 'projectversioning', fieldId: 'id' },
        autoupgrade: { sectionId: 'projectversioning', fieldId: 'autoupgrade' },
        gitrepourl: { sectionId: 'projectversioning', fieldId: 'gitrepourl' },
        urlprefix: { sectionId: 'projectversioning', fieldId: 'urlprefix' },
        gitbranchname: { sectionId: 'projectversioning', fieldId: 'gitbranchname' },
        gitdocspath: { sectionId: 'projectversioning', fieldId: 'gitdocspath' },
        gitreadmepath: { sectionId: 'projectversioning', fieldId: 'gitreadmepath' },
        projectname: { sectionId: 'projectversioning', fieldId: 'projectname' },
        projectwebsiteurl: { sectionId: 'projectversioning', fieldId: 'projectwebsiteurl' },
        nugetpackages: { sectionId: 'projectversioning', fieldId: 'nugetpackages' },
        alwaysNewestDocsUrl: { sectionId: 'projectversioning', fieldId: 'alwaysNewestDocsUrl' },
    },
    project: {
        id: { sectionId: 'project', fieldId: 'id' },
        description: { sectionId: 'project', fieldId: 'description' },
        name: { sectionId: 'project', fieldId: 'name' },
        githubUrl: { sectionId: 'project', fieldId: 'githubUrl' },
        nugetPackages: { sectionId: 'project', fieldId: 'nugetPackages' },
        shieldsBadgeMd: { sectionId: 'project', fieldId: 'shieldsBadgeMd' },
        lastDocfxBuildTime: { sectionId: 'project', fieldId: 'lastDocfxBuildTime' },
        bgHealthCheckHttpGetDateTime: { sectionId: 'project', fieldId: 'bgHealthCheckHttpGetDateTime' },
        bgHealthCheckHttpGetStatus: { sectionId: 'project', fieldId: 'bgHealthCheckHttpGetStatus' },
        urlPrefix: { sectionId: 'project', fieldId: 'urlPrefix' },
        status: { sectionId: 'project', fieldId: 'status' },
        docsUrl: { sectionId: 'project', fieldId: 'docsUrl' },
        autoRebuildLatestNupkgEnabled: { sectionId: 'project', fieldId: 'autoRebuildLatestNupkgEnabled' },
        autoRebuildLatestNupkgRebuildDatetime: { sectionId: 'project', fieldId: 'autoRebuildLatestNupkgRebuildDatetime' },
    }
}

export type { Field };
export type { HelpSection };

export { helpDataJson as HelpData, getField, FID };