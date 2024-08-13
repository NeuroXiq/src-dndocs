'use client'
import Layout from "@/components/Layout";
import PageLoading from "@/components/PageLoading";
import Typoheader from "@/components/Typoheader";
import UseApi from "@/services/Api";
import UseUser from "@/services/user";
import { Button, Link, Typography } from "@mui/material";
import Urls from "config/Urls";
import { GlobalAppContext, IGlobalAppContextValue } from "hooks/globalAppContext";
import { useRouter } from "next/navigation";
import { useContext, useEffect, useState } from "react";

export default function Index() {
    const router = useRouter();
    const gc = useContext<IGlobalAppContextValue>(GlobalAppContext);

    const userCodeFromGithub = new URL(document.location.toString()).searchParams.get("code") || '';
    const api = UseApi();
    const user = gc.user;

    const [state, setState] = useState<any>({
        loading: false,
        title: '',
        message: '',
        authOk: false,
        validationOk: false
    });

    useEffect(() => {
        const newState: any = {};
        let valid = true;
        const debug_skip = false;

        if (user.isAuthenticated) {
            valid = false;
            newState.title = 'Authentication Failed';
            newState.message = 'User is authenticated';
            newState.authOk = false;
        } else if (!userCodeFromGithub) {
            valid = false;
            newState.title = 'Authentication Failed';
            newState.message = 'There is no github code from callback, aborting ';
            newState.authOk = false;
        }

        if (!valid) {
            newState.loading = false;
            setState(newState);
            return;
        }

        // const jwtcc = 'eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiTmV1cm9YaXEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJOZXVyb1hpcUB3cC5wbCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTRjN2Q2YzktZTFhMi00YzY2LWJhMDQtY2I1NTlhZTMyZmM0IiwiUm9iaW5pYV9Vc2VySWQiOiIzIiwiZXhwIjoxNjkyNzgyMTIwLCJpc3MiOiJyb2JpbmlhZG9jcy5jb20iLCJhdWQiOiJyb2JpbmlhZG9jcy5jb20ifQ.MgstvQmNMzdDIb6wiIk118ZdcGct5LJgw0L-NwsGDcc'

        setState({
            loading: true,
            title: 'Authentication in progress...',
            message: 'Authentication in progress, please wait'
        });

        api.Auth_CallbackGithubOAuth(userCodeFromGithub).then(r => {
            // const r = { success: true, result: jwtcc }
            const newState: any = {};
            newState.loading = false;
            newState.authOk = r.success;

            if (newState.authOk) {
                gc.loginUser(r.result);
                newState.title = 'Authentication Success';
                newState.message = 'Authentication success, wait for redirect';

                setTimeout(() => {
                    router.push(Urls.account.details);
                }, 1000);
            } else {
                newState.title = 'Authentication failed';
                newState.message = 'Authentication failed';
            }

            setState(newState);
        });
    }, []);

    let title = state.title;
    let message = state.message;
    let loading = state.loading;
    let mode = (state.authOk || state.loading) ? null : "error";
    let authOk = state.authOk;

    return (
        <Layout>
            <PageLoading open={loading}></PageLoading>
            <Typoheader mode={mode}>{title}</Typoheader>
            <Typoheader mode={mode} variant="h6">{message}</Typoheader>
            {authOk ? null : <Link href={Urls.home.index}><Button variant="outlined" fullWidth>Go to Home Page</Button></Link>}
        </Layout>
    );
}