'use client'
import Layout from "@/components/Layout";
import PageLoading from "@/components/PageLoading";
import UseUser from "@/services/user";
import { Button, Link, Typography } from "@mui/material";
import { GlobalAppContext, IGlobalAppContextValue } from "hooks/globalAppContext";
import { useContext, useEffect, useState } from "react";
import LogoutIcon from '@mui/icons-material/Logout';
import UseApi from "@/services/Api";
import { useRouter } from "next/navigation";
import Urls from "config/Urls";

export default function Page() {
    const gac = useContext<IGlobalAppContextValue>(GlobalAppContext);
    const [loading, setLoading] = useState<boolean>(false);
    const [logoutCompleted, setLogoutCompleted] = useState<boolean>(false);
    const router = useRouter();
    const user = gac.user;
    const api = UseApi();

    function onLogoutClick() {
        setLoading(true);
        gac.logoutUser();
        gac.showSuccessMessage("Signout success");
        
        // todo: now do not care about errors
        // (what will happen on this page when token invalid/expired? - need to check this)
        api.Auth_Logout().then(r => {
            setLoading(false);
            setLogoutCompleted(r.success);
            
            setTimeout(() => {
                router.push(Urls.home.index);
            }, 2000);
        });
    }

    const title = logoutCompleted ? 'Logout Completed' : 'Logout';

    return (
        <Layout title={title}>
            {<PageLoading open={loading} />}
            {logoutCompleted && <Typography margin={1} color="primary" variant="h5" textAlign="center">Please wait for redirect...</Typography>}
            {
                user.isAuthenticated ?
                    <Button
                        disabled={logoutCompleted}
                        onClick={onLogoutClick}
                        color="warning"
                        startIcon={<LogoutIcon />} fullWidth variant="contained">Click here to Logout</Button> :
                    <Typography variant="h6">Your are not logged in and You cannot logout</Typography>
            }
        </Layout>
    );
}