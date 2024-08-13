import { BgProjectHealthCheckStatus } from "./BgProjectHealthCheckStatus"
import { ProjectStatus } from "./ProjectStatus"

const ProjectStatusUINames = {
    [ProjectStatus.New]: 'New',
    [ProjectStatus.Active]: 'Active',
    [ProjectStatus.Blocked]: 'Blocked',
    [ProjectStatus.Requested]: 'Requested',
    [ProjectStatus.DeployStarted]: 'Deploy Started',
    [ProjectStatus.DeployFailed]: 'Deploy Failed',
    [ProjectStatus.Building]: 'Building',
    [ProjectStatus.BuildFailed]: 'Build Failed',
}

const BgProjectHealthCheckStatusUINames = {
    [BgProjectHealthCheckStatus.HttpGetOk]: 'HTTP GET OK',
    [BgProjectHealthCheckStatus.HttpGetFail]: 'HTTP GET FAIL',
    [BgProjectHealthCheckStatus.SystemFailedToInvokeGet]: 'System Failed To Invoke Get',
}

const einfo: any = {
    ProjectStatus: {
        UIName: ProjectStatusUINames
    },
    BgProjectHealthCheckStatus: {
        UIName: BgProjectHealthCheckStatusUINames
    }
}

const EID = {
    ProjectStatus: 'ProjectStatus',
    BgProjectHealthCheckStatus: 'BgProjectHealthCheckStatus'
}

const UIName = function (eid: any, value: any) {
    let error = '';

    if (!eid) {
        error = 'eid is null or empty';
    } else if (value === null || value === undefined) {
        error = 'value is null or empty';
        return '-';
    } else if (!einfo[eid]) {
        error = 'Enum with id: "${eid}" not found';
    }

    if (error) {
        throw new Error(error);
    }

    return einfo[eid].UIName[value] || value;
}

enum TableDataRequestOrderDir {
    Asc = 1,
    Desc = 2
}

enum ContentType {
    Folder = 1,
    Yml = 2,
    Md = 3
}

export {
    EID,
    UIName,
    ContentType,
    ProjectStatus,
    TableDataRequestOrderDir
}