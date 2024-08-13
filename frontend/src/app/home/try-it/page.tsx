'use client';
import Form from "@/components/Forms/Form";
import InputNugetPackages, { NupkgStringToArray } from "@/components/Forms/InputNugetPackages";
import InputText from "@/components/Forms/InputText";
import Layout from "@/components/Layout";
import Typoheader from "@/components/Typoheader";
import UseApi from "@/services/Api";
import { Alert, Button, FormLabel, Grid, Typography } from "@mui/material";
import Urls from "config/Urls";
import { useRouter } from "next/navigation";
import { useState } from "react";


export default function Page() {
    const router = useRouter();
    const [form, setForm] = useState({
        pkgname: '',
        pkgver: '',
        nameErr: false,
        verErr: false
    });

    const onFormSubmit = function (e: any) {
        e.preventDefault();
        if (!form.pkgname) {
            setForm((x) => { return { ...x, nameErr: true } });
        }

        if (!form.pkgver) {
            setForm((x) => { return { ...x, verErr: true } });
        }

        if (!form.pkgver || !form.pkgname) { return; }
        
        router.push(Urls.i.nuget(form.pkgname.trim(), form.pkgver.trim()));
    }

    return (
        <Grid container >
            <Grid item xl={4} md={3}></Grid>
            <Grid item xl={4} md={6}>
                <Typoheader>Try It - Generate Docs</Typoheader>
                <Form onSubmit={onFormSubmit}>
                    <InputText
                        onChange={(e:any) => setForm(x => {return {...x, pkgname: e.newValue }})}
                        name="pkgname" value={form.pkgname} label="Nuget Package Name" errors={form.nameErr ? 'Nuget Package Name is required' : null} />
                    <InputText 
                    onChange={(e:any) => setForm(x => {return {...x, pkgver: e.newValue }})}
                    name="pkgver" value={form.pkgver} label="Nuget Package Version" errors={form.nameErr ? 'Nuget Package Version is required' : null}/>
                    <Button onClick={onFormSubmit} type="submit" variant="contained" color="success">Run</Button>
                    <Typography></Typography>
                </Form>
            </Grid>
            <Grid item xl={4} md={3}></Grid>
        </Grid>
    );
}


// export default function Page() {
//     const [form, setForm] = useState<any>({ nugetpackages: [] });
//     const [result, setResult] = useState<any>(null);
//     const api = UseApi();
//     const router = useRouter();

//     function onInputChange(e: any) {
//         setForm({ [e.name]: e.newValue });
//     }

//     function onSubmit(e: any) {
//         e.preventDefault();

//         let fdata = {
//             nugetPackages: NupkgStringToArray(form.nugetpackages)?.map(r => r.identityId)
//         }

//         api.Home_TryItCreateProject(fdata).then(r => {
//             setResult(r);
//             if (r.success) {
//                 router.push(Urls.account.bgjobWaiting.tryItCreateProject(r.result));
//             }
//         });
//     }

//     return (
//         <Layout title="Try it without login">
//             <Alert severity="info" sx={{ marginBottom: '1rem' }}>
//                 Following form will create temporary project from Nuget Packages Name.
//                 Project will be hosted for some period of time and then removed from the system.
//                 You can check how generated documentation will looks like for Your specific
//                 Nuget Package(s).
//             </Alert>
//             <Form autoApiResult={result}>
//                 <InputNugetPackages onChange={onInputChange} name="nugetpackages"/>
//                 <Button variant="contained" type="submit" disabled={false} onClick={onSubmit}>Submit</Button>
//             </Form>
//         </Layout>
//     );
// }