'use client'
import Layout from "@/components/Layout";
import PageLoading from "@/components/PageLoading";
import SimpleTable, { TableCfg } from "@/components/SimpleTable";
import UseApi, { useQueryEffect } from "@/services/Api";
import ProjectDocsVersionDto from "@/services/ApiDTO/Home/ProjectDocsVersionDto";
import { ResetTvOutlined } from "@mui/icons-material";
import { Link, Typography } from "@mui/material";
import Urls from "config/Urls";

import { useParams, useRouter } from "next/navigation";

export default function Page() {
    const params = useParams();
    const projectVersioningId = +(params?.projectversioningid || -1);
    const api = UseApi();

    const { result: versions, loading: vloading } = useQueryEffect<ProjectDocsVersionDto[]>(() => api.Home_GetProjectDocsVersions(projectVersioningId), [projectVersioningId]);

    const tableCfg : TableCfg<ProjectDocsVersionDto> = {
        data: versions || [],
        rowidval: v => v.gitTagName,
        cols: [
            { title: 'Version', val: v => v.gitTagName },
            { title: 'Docs URL', val: v => <Link href={Urls.robiniadocsUrl(v.projectUrlPrefix)}>{Urls.robiniadocsUrl(v.projectUrlPrefix)}</Link> },
        ]
    }

    const noVers = !vloading && (!versions || versions.length === 0);

    return (
        <Layout title="Project Versions">
            <PageLoading open={vloading} />
            <SimpleTable config={tableCfg} />
            {noVers && <Typography variant="h4" textAlign="center" color="warning">No versions for Versioning with ID: {projectVersioningId} </Typography>}
        </Layout>
    )
}