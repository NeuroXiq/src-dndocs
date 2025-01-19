'use client'
import { useContext, useEffect, useState } from 'react';
import ApiUrls from '../shared/ApiUrls';
import RequestProjectModel from './api-model-dn/RequestProjectModel';
import { GlobalAppContext, IGlobalAppContextValue } from '../shared/GlobalAppContext';

class Api {
    gctx: IGlobalAppContextValue;

    constructor(gctx: IGlobalAppContextValue) {
        this.gctx = gctx;
    }

    fetchGet(url: string, queryParams: any = null): any {
        const config = {
            method: 'GET',
            headers: { 'accept': 'application/json' }
        };

        let qurl = url;

        if (queryParams) {
            qurl += '?' + new URLSearchParams(queryParams);
        }

        let p = this.fetch(qurl, config);

        return {
            then: function (callback: any) {
                p.then(callback);
            },
            thenResult: function (callback: any) {
                p.then(r => callback(r.result));
            }
        }
    }

    fetchPost(url: string, config: any): Promise<any> {
        config = {
            ...config,
            method: 'POST',
            headers: {
                'content-type': 'application/json'
            }
        };

        if (config.isMultipartFormData) {
            delete config.headers['content-type'];
        }

        return this.fetch(url, config);
    }

    fetchDelete(url: string, config: any) {
        config = {
            ...config,
            method: 'DELETE',
            headers: {
                'content-type': 'application/json'
            }
        }

        return this.fetch(url, config);
    }

    fetch(url: any, config: any): Promise<any> {
        let fconfig = {
            method: config.method,
            body: config.body,
            headers: config.headers || {}
        };

        const jwt = localStorage.getItem('jwt');

        if (jwt) {
            fconfig.headers['Authorization'] = 'Bearer ' + jwt;
        }

        let resolve: any, reject: any;

        let promise = new Promise(function (rs, rj) {
            resolve = rs;
            reject = rj;
        });

        let cancel = false;
        let _this = this;

        let errorData = {
            fetchResult: null,
            fetchConfig: config,
            url: url,
            error: null,
            handlerResult: null
        };

        try {
            fetch(url, fconfig).then((result: any) => {
                if (cancel) {
                    return;
                }

                errorData.fetchResult = result;

                if (config.rawFetchResult) {
                    resolve(result);
                    return;
                }

                const showError =
                    (result.headers.get('content-type')?.indexOf('application/json') ?? -1) < 0 ||
                    (!result.ok && result.status !== 400);

                if (showError) {
                    _this.gctx.onApiFetchError(errorData);
                    reject(errorData);
                    return;
                }

                result.json().then((r: any) => {
                    if (r.error || r.fieldErrors) {
                        errorData.handlerResult = r;
                        _this.gctx.onApiFetchError(errorData);
                    }

                    resolve(r);
                });

            }).catch((err: any) => {
                errorData.error = err;
                _this.gctx.onApiFetchError(errorData);
                reject(errorData);
            });
        } catch (err: any) {
            errorData.error = err;
            _this.gctx.onApiFetchError(errorData);
            reject(errorData);
        } finally {

        }

        return promise;
    }

    public getAllUserProjects() {
        return this.fetchGet(ApiUrls.myAccount.getmyprojects);
    }

    public ProjectManage_GetProjectVersioningById(id: number) {
        return this.fetchGet(ApiUrls.projectmanage.GetProjectVersioningById, { id: id });
    }

    public ProjectManage_RequestProject(model: RequestProjectModel): Promise<any> {
        let formData = new FormData();

        for (var key in model) {
            formData.append(key, (model as any)[key]);
        }

        formData.delete('nugetPackages');
        formData.delete('projectfiles');

        model.nugetPackages.forEach((a: any) => formData.append('nugetpackages', a));
        (model.projectFiles || []).forEach((a: any) => formData.append('projectfiles', a, a.name));

        return this.fetchPost(ApiUrls.projectmanage.RequestProject, {
            body: formData,
            isMultipartFormData: true
        });
    }

    public adminLogin(form: any): Promise<any> {
        return this.fetchPost(ApiUrls.auth.adminLogin, {
            body: JSON.stringify(form)
        });
    }

    public MyAccountGetAccountDetails(): Promise<any> {
        return this.fetchGet(ApiUrls.myAccount.getAccountDetails);
    }

    public MyAccount_GetGithubRepositories(flushCache: boolean) {
        return this.fetchGet(ApiUrls.myAccount.GetGithubRepositories, { flushCache: flushCache });
    }

    public MyAccount_GetBgJob(projectId: number) {
        return this.fetchGet(ApiUrls.myAccount.GetBgJob, { projectId: projectId });
    }

    public MyAccount_GetSystemMessages(form: any) {
        return this.fetchGet(ApiUrls.myAccount.GetSystemMessages, form);
    }

    public Admin_GetDashboardInfo(): Promise<any> {
        return this.fetchGet(ApiUrls.admin.getDashboardInfo);
    }

    public Admin_GetProjectFiles(projectId: number): Promise<any> {
        return this.fetch(ApiUrls.admin.getProjectFiles(projectId), {
            method: 'GET',
            rawFetchResult: true
        })
            .then(r => r.blob())
            .then(r => {
                let fileUrl = window.URL.createObjectURL(r);
                return fileUrl;
            })
    }

    public Admin_ExecuteRawSql(form: any) {
        return this.fetchPost(ApiUrls.admin.executeRawSql, {
            body: JSON.stringify(form)
        });
    }

    public Admin_GetTableData(form: any) {
        return this.fetchPost(ApiUrls.admin.getTableData, {
            body: JSON.stringify(form)
        });
    }

    public CreateProjectVersionByGitTag(versioningId: any, gitTagName: any) {
        return this.fetchPost(ApiUrls.projectmanage.CreateProjectVersionByGitTag, {
            body: JSON.stringify({ projectVersioningId: versioningId, gitTagName: gitTagName })
        })
    }

    public ProjectManage_GetProjectsVersioningInfo(versioningid: number, pageNo: number) {
        return this.fetchGet(ApiUrls.projectmanage.GetProjectsVersioningInfo, { projectVersioningId: versioningid, pageNo: pageNo });
    }

    public ProjectManage_GetVersioningGitTags(versioningid: number) {
        return this.fetchGet(ApiUrls.projectmanage.GetVersioningGitTags, { projectVersioningId: versioningid });
    }

    public ProjectManage_RequestAutoupgrade(projectid: number) {
        return this.fetchPost(ApiUrls.projectmanage.RequestAutoupgrade, {
            body: JSON.stringify(projectid)
        });
    }

    public Admin_HardRecreateProject(projectId: number) {
        return this.fetchPost(ApiUrls.admin.HardRecreateProject, { body: JSON.stringify(projectId) });
    }

    public Admin_DoBackgroundWorkNow(form: any) {
        return this.fetchPost(ApiUrls.admin.DoBackgroundWorkNow, {
            body: JSON.stringify(form)
        });
    }

    public ProjectManage_GetProjectById(projectid: number) {
        return this.fetchPost(ApiUrls.projectmanage.GetProjectById, {
            body: JSON.stringify(projectid)
        })
    }

    public ProjectManage_DeleteProject(projectid: number) {
        return this.fetchDelete(ApiUrls.projectmanage.DeleteProject, {
            body: JSON.stringify(projectid)
        });
    }

    public Auth_CallbackGithubOAuth(githubCode: string) {
        return this.fetchPost(ApiUrls.auth.CallbackGithubOAuth, {
            body: JSON.stringify(githubCode)
        })
    }

    public Auth_Logout() {
        return this.fetchPost(ApiUrls.auth.Logout, {});
    }

    public Home_TryItCreateProject(form: any) {
        return this.fetchPost(ApiUrls.home.TryItCreateProject, { body: JSON.stringify(form) });
    }

    public Home_GetRecentProjects() {
        return this.fetchGet(ApiUrls.home.GetRecentProjects);
    }

    public Home_GetAllProjects() {
        return this.fetchGet(ApiUrls.home.GetAllProjects);
    }

    public Home_GetVersionInfo() {
        return this.fetchGet(ApiUrls.home.GetVersionInfo);
    }

    public ProjectManage_DeleteProjectVersioning(projectVersioningId: number) {
        return this.fetchDelete(ApiUrls.projectmanage.DeleteProjectVersioning, {
            body: JSON.stringify(projectVersioningId)
        });
    }
    public ProjectManage_DocfxRemoveFile(form: any) {
        return this.fetchPost(ApiUrls.projectmanage.DocfxRemoveFile, {
            body: JSON.stringify(form)
        });
    }

    public ProjectManage_DocfxRemoveDirectory(form: any) {
        return this.fetchPost(ApiUrls.projectmanage.DocfxRemoveDirectory, {
            body: JSON.stringify(form)
        });
    }

    public Project_GetAllProjectVersioning() {
        return this.fetchGet(ApiUrls.projectmanage.GetAllProjectVersioning);
    }

    public Integration_NuGetCreateProject(form: any) {
        return this.fetchPost(ApiUrls.integration.NuGetCreateProject, {
            body: JSON.stringify(form)
        });
    }

    public Integration_NugetCreateProjectCheckStatus(packageName: string, packageVersion: string) {
        return this.fetchGet(ApiUrls.integration.NugetCreateProjectCheckStatus, { packageName: packageName, packageVersion: packageVersion });
    }
}

export default function UseApi() {
    const globalContext = useContext<IGlobalAppContextValue>(GlobalAppContext);
    return new Api(globalContext);
}

export function useQueryEffect<TResult = any>(action: any, useEffectParam: any = []) {
    const [loading, setLoading] = useState<boolean>(true);
    const [result, setResult] = useState<TResult>(null as TResult);
    const [response, setResponse] = useState<any>(null);

    useEffect(() => {
        action().then((r: any) => {
            setLoading(false);
            setResult(r.result);
            setResponse(r);
        });
    }, useEffectParam);

    return { loading, result, response }
}
