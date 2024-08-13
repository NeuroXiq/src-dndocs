'use client';
import UseApi from "@/services/Api";
import { Typography } from "@mui/material";
import Config from "config/Config";
import { useEffect, useState } from "react";


export default function Page() {
    const api = UseApi();
    const [backendVersion, setBackendVersion] = useState<any>(null);

    useEffect(() => {
        api.Home_GetVersionInfo().then((r: any) => {
            setBackendVersion(r.result);
        });
    }, []);

    return (
        <pre>
            Frontend Version Info: {Config.versionInfo} <br />
            Backend Version Info: <br />
            {backendVersion && JSON.stringify(backendVersion, null, 2)}
        </pre>
    )
}