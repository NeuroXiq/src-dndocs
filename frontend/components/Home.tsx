import "@fontsource/space-grotesk/400.css"; // Specify weight
import { Alert, Box, Button, Card, CardContent, CircularProgress, Divider, Grid, setRef, Stack, Step, StepLabel, Stepper, TextField, Typography } from "@mui/material";
import Image from 'next/image';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import PersonIcon from '@mui/icons-material/Person';
import '../css/home.css';
import SendIcon from '@mui/icons-material/Send';
import GitHubIcon from '@mui/icons-material/GitHub';
import LaunchIcon from '@mui/icons-material/Launch';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import Urls from "config/Urls";
import useGlobalAppContext from "hooks/useGlobalAppContext";
import CookieConsent from "./Shared/CookieConsent";
import InputText from "./Forms/InputText";
import AnnouncementIcon from '@mui/icons-material/Announcement';
import ArticleIcon from '@mui/icons-material/Article';
import CodeIcon from '@mui/icons-material/Code';
import { useParams, useSearchParams } from "next/navigation";
import BgJobViewModel from "@/services/ApiDTO/Other/BgJobViewModel";
import { useEffect, useState } from "react";
import UseApi from "@/services/Api";
import ReplayIcon from '@mui/icons-material/Replay';

function DotsProgress() {
    const [counter, setCounter] = useState<number>(0);
    useEffect(() => {
        const ival = setInterval(() => setCounter((x) => x + 1), 350);

        return () => clearInterval(ival);
    }, [setCounter]);

    const dots = counter % 4;
    return (
        <>
            <div style={{ display: 'inline-block', visibility: (dots > 0 ? 'visible' : 'hidden') }}>.</div>
            <div style={{ display: 'inline-block', visibility: (dots > 1 ? 'visible' : 'hidden') }}>.</div>
            <div style={{ display: 'inline-block', visibility: (dots > 2 ? 'visible' : 'hidden') }}>.</div>
        </>
    );
}

export default function Home() {
    function stepperInitialState() {
        return {
            currentStep: -1, steps: [
                { label: 'Initializing', failed: false, labelOptional: null },
                { label: 'Queue', failed: false, labelOptional: null },
                { label: 'Building', failed: false, labelOptional: null },
                { label: 'Done', failed: false, labelOptional: null }
            ]
        };
    }

    const gac = useGlobalAppContext();
    const urlParams = useSearchParams();
    const api = UseApi();
    const urlPackageName = urlParams?.get('packageName');
    const urlPackageVersion = urlParams?.get('packageVersion');
    const [iPackageName, setIPackageName] = useState<any>('');
    const [iPackageVersion, setIPackageVersion] = useState<any>('');
    const [errors, setErrors] = useState<any>({ name: false, version: false });
    const [state, setState] = useState<string>('init');
    const [stepper, setStepper] = useState<any>(stepperInitialState());

    console.log(urlPackageName, urlPackageVersion);

    const [jobStatus, setJobStatus] = useState<BgJobViewModel | null>(null);
    const [createResult, setCreateResult] = useState<any>(null);
    const [refreshJob, setRefreshJob] = useState<boolean>(false);
    const [refreshCounter, setRefreshCounter] = useState<number>(0);
    const [openDocsTimer, setOpenDocsTimer] = useState<number>(-1);

    function reset() {
        setJobStatus(null);
        setCreateResult(null);
        setState('init');
        setOpenDocsTimer(-1);
        setStepper(stepperInitialState());
    }

    // useEffect(() => {
    //     // reset();
    // }, [iPackageName, iPackageVersion]);

    function createProject(pkgname: string, pkgversion: string) {
        api.Integration_NuGetCreateProject({ packageName: pkgname, packageVersion: pkgversion })
            .then((cr) => {
                setCreateResult(cr);

                if (cr.success) {
                    setRefreshJob(true);
                } else {
                    setState('error');
                    let newsstate = stepperInitialState();
                    newsstate.steps[0].failed = true;
                    setStepper(newsstate);
                }
            });
    }

    useEffect(() => {
        if (urlPackageName && urlPackageVersion) {

            setIPackageName(urlPackageName);
            setIPackageVersion(urlPackageVersion);
            startCreateProject(urlPackageName, urlPackageVersion);
        }
    }, [urlPackageName, urlPackageVersion]);

    useEffect(() => {
        if (openDocsTimer === -1) { return; }

        if (openDocsTimer === 0 && jobStatus && jobStatus.state === 1) {
            window.location.href = jobStatus?.projectApiFolderUrl;
            return;
        }

        let timeout = setTimeout(() => setOpenDocsTimer((x) => x - 1), 1000);

        return () => clearTimeout(timeout);

    }, [openDocsTimer, jobStatus]);

    function onRefreshJobCompleted(jobStatus: BgJobViewModel) {
        const newState = stepperInitialState();

        if (jobStatus == null && createResult == null) {
            newState.currentStep = -1;
        } else if (createResult?.success === false) {
            newState.currentStep = 0;
            newState.steps[0].failed = true;
        } else if (jobStatus?.stateDetails === 2) {
            newState.currentStep = 1;
        } else if (jobStatus?.stateDetails === 3) {
            newState.currentStep = 2;
        } else if (jobStatus?.stateDetails !== 6) {
            // failed
            newState.currentStep = 3;
            newState.steps[3].failed = true;
            setState('error');
        } else if (jobStatus?.state === 1) {
            newState.currentStep = 4;
            setOpenDocsTimer(5);
            setState('success');
        }

        (newState.steps[1] as any).labelOptional = 'No.' + jobStatus?.estimateOtherJobsBeforeThis + ' (~' + jobStatus?.estimateStartIn + 's)';
        (newState.steps[2] as any).labelOptional = '~' + jobStatus?.estimateBuildTime + 's';

        setJobStatus(jobStatus);
        setStepper(newState);

        if (jobStatus?.stateDetails !== 2 && jobStatus?.stateDetails !== 3) { setRefreshJob(false); }
    }

    useEffect(() => {
        let abort = false;

        if (!refreshJob || !iPackageName || !iPackageVersion) { return; }

        api.Integration_NugetCreateProjectCheckStatus(iPackageName as string, iPackageVersion as string)
            .then((r: any) => {
                setTimeout(() => setRefreshCounter((x) => x + 1), 2000);
                if (abort) { return; }

                onRefreshJobCompleted(r.result);
            });

        return () => { abort = true; };
    }, [iPackageName, iPackageVersion, refreshJob, refreshCounter]);

    function startCreateProject(pkgName: string, pkgVer: string) {
        let newErrors = {
            name: !pkgName,
            version: !pkgVer
        }

        reset();
        setErrors(newErrors);

        if (newErrors.name || newErrors.version) { return; }

        setStepper((x: any) => {
            return {
                currentStep: 0,
                steps: x.steps
            };
        });

        setState('loading');

        api.Integration_NugetCreateProjectCheckStatus(pkgName, pkgVer)
            .then((r: any) => {
                if (r.result == null) {
                    createProject(pkgName, pkgVer);
                } else {
                    onRefreshJobCompleted(r.result);
                }
            });
    }

    return (
        <div className="home">
            <header className="header">
                <h1>DNDocs</h1>
                <Button href={Urls.account.details}><PersonIcon sx={{ color: 'white' }} fontSize="large" /></Button>
                <div className="logo">
                    <div className="nletter">.N</div>
                </div>
            </header>
            <main className="main">
                <Grid container>
                    <Grid item lg={3}></Grid>
                    <Grid item lg={6}>
                        <Typography
                            variant="h2"
                            fontWeight="medium"
                            align="center"
                            marginBottom={4}>Explore API Docs...</Typography>

                        <Box marginBottom={4} style={{ display: 'flex', alignItems: 'stretch', gap: "0.25rem" }}>
                            <TextField
                                error={errors.name}
                                onChange={(e) => { setIPackageName(e.target.value); reset(); }}
                                value={iPackageName}
                                disabled={state === 'loading'}
                                sx={{ flex: '3 1 1px' }}
                                label="Nuget Package Name"
                                variant="outlined"
                                type="text"
                                size="medium"
                                fullWidth
                            />

                            <TextField
                                error={errors.version}
                                disabled={state === 'loading'}
                                onChange={(e) => { setIPackageVersion(e.target.value); reset(); }}
                                value={iPackageVersion}
                                sx={{ flex: '3 1 1px' }}
                                fullWidth
                                label="Nuget Package Version"
                                variant="outlined"
                                type="text"
                                size="medium"
                            />

                            {(state === 'init' || state === 'loading') &&
                                <Button
                                    sx={{ flex: '1 1 1px' }}
                                    variant="contained"
                                    onClick={() => startCreateProject(iPackageName, iPackageVersion)}
                                    color="info"
                                    disabled={state === 'loading'}
                                    startIcon={<SendIcon />}>
                                    Explore
                                </Button>
                            }

                            {(state === 'success') &&
                                <Button
                                    sx={{ flex: '1 1 1px' }}
                                    startIcon={<LaunchIcon />}
                                    href={jobStatus?.projectApiFolderUrl}
                                    variant="contained" color="success">
                                    Open ({openDocsTimer})
                                </Button>
                            }

                            {(state === 'error') &&
                                <Button
                                    sx={{ flex: '1 1 1px' }}
                                    variant="contained"
                                    color="error"
                                    onClick={() => startCreateProject(iPackageName, iPackageVersion)}
                                    startIcon={<ReplayIcon />}>
                                    retry
                                </Button>}
                        </Box>
                        <Box marginBottom={4} sx={{ width: '100%' }} display="flex">
                            <Stepper sx={{ flex: "1 0 1px" }} activeStep={stepper.currentStep}>
                                {stepper.steps.map((step: any, index: any) => {
                                    const labelProps: {
                                        optional?: React.ReactNode;
                                        error?: boolean;
                                    } = {};

                                    if (stepper.steps[index].failed === true) {
                                        labelProps.optional = (
                                            <Typography variant="caption" color="error">
                                                Step failed
                                            </Typography>
                                        );
                                        labelProps.error = true;
                                    } else if (stepper.steps[index].labelOptional) {
                                        labelProps.optional = stepper.steps[index].labelOptional && (
                                            <Typography variant="caption" >
                                                {stepper.steps[index].labelOptional}
                                            </Typography>
                                        );
                                    }

                                    return (
                                        <Step key={step.label}>
                                            <StepLabel {...labelProps} color="success">
                                                {step.label}
                                                {(index === stepper.currentStep && stepper.currentStep !== 3) ? <DotsProgress /> : null}
                                            </StepLabel>
                                        </Step>
                                    );
                                })}
                            </Stepper>
                        </Box>
                        <Box marginBottom={3} style={{ display: 'flex', gap: "0.25rem", flexWrap: "wrap", justifyContent: "center" }}>
                            <Button href={Urls.other.DNDocsGithub} variant="contained" size="small" color="success" startIcon={<GitHubIcon />}>DNDocs Github</Button>
                            <Button href={Urls.other.DDocsAllProjects} variant="outlined" size="small" color="primary" startIcon={<ArticleIcon />}>All projects</Button>
                            <Button href={Urls.other.DNDocsSourceGithub} variant="outlined" size="small" color="primary" startIcon={<CodeIcon />}>Source code</Button>
                            <Button href={Urls.other.DNDocsReportIssue} color="error" size="small" variant="outlined" startIcon={<AnnouncementIcon />}>Report Issue</Button>
                        </Box>
                        {(createResult?.success === false) && <Alert severity="error">
                            <pre>
                                {JSON.stringify(createResult, null, 4)}
                            </pre>
                        </Alert>}
                        {(jobStatus?.stateDetails === 4) && <Alert severity="error">Build failed. Project Id: {jobStatus.projectId}</Alert>}
                    </Grid>
                    <Grid item lg={3}></Grid>
                </Grid>

            </main>
        </div>
    );
}

function Home_old() {
    const gac = useGlobalAppContext();
    function stepImg(imgurl: string): any {
        return (<img alt="step-1" src={imgurl} />)
    }

    function stepVid(): any {
        return (
            <video autoPlay loop muted>
                <source src="/step3.mp4" type="video/mp4" />
            </video>
        );
    }

    function howItWorks(id: number): any {
        let title = 'test test test';
        let img = '/howtouse-1.png';
        let desc = 'test test test';

        if (id === 1) {
            title = "Create a project"
            desc = "Fill nuget package name(s), project name and github url in create project form";
            img = stepImg('/step1.png');

        } else if (id === 2) {
            title = "Wait for Job"
            desc = "Wait 30 - 60s until system job build Your project. You will see " +
                'progress in job logs'

            img = stepImg('/step2.png');
        } else if (id === 3) {
            title = 'API Docs are online!'
            desc = 'Your documentation is hosted online and ready-to-use.' +
                '';

            img = stepVid();
        } else { throw new Error('asdf'); }

        return (
            <div className="step">
                <h3>{title}</h3>
                <div className="img">
                    {img}
                </div>
                <div className="footer">{desc}</div>
            </div>
        );
    }

    function main1(): any {
        return (
            <div className="main__howit">
                <h1>How it works</h1>
                <Grid container alignItems="stretch" spacing={1}>
                    <Grid item xs={12} md={4}>{howItWorks(1)}</Grid>
                    <Grid item xs={12} md={4}>{howItWorks(2)}</Grid>
                    <Grid item xs={12} md={4}>{howItWorks(3)}</Grid>
                </Grid>
            </div>);
    }

    function main2(): any {
        return (
            <div className="tryit">
                <div className="tryit__buttons">
                    <h1>Try it</h1>
                    <Button variant="contained" size="large" href={Urls.home.tryit}
                        color="info" startIcon={<SendIcon />}>Try it without login</Button>
                    <Button variant="outlined"
                        href={Urls.account.login(Urls.account.details)}
                        size="large" color="secondary"
                        startIcon={<PersonIcon />}>Login with github</Button>
                    <Button
                        href={Urls.home.projects}
                        variant="outlined" size="large"
                        color="secondary"
                        startIcon={<AccountTreeIcon />}>See all projects</Button>
                    <Button
                        href={Urls.other.DNDocsGithub}
                        variant="outlined"
                        size="large"
                        color="secondary" startIcon={<GitHubIcon />}>Github</Button>
                </div>
                <div className="features">
                    <h3>Features</h3>
                    <ol>
                        <li>API Explorer using DOCFX</li>
                        <li>README.md as main page</li>
                        <li>MD docs from github in articles section</li>
                        <li>Versioning support</li>
                        <li>Multiple templates (discordfx, docfx-minimal-main, material, singulinkfx)</li>
                        <li>Auto-generate latest nuget packages</li>
                        <li>Auto-generate MD docs from github</li>
                        <li>Auto-generate project versions</li>
                    </ol>
                </div>
            </div>
        );
    }

    function main3(): any {
        return (
            <Grid container spacing={1} alignItems="stretch" className="notes">
                <Grid item lg={12} md={0}><Divider /></Grid>
                <Grid item md={12} lg={4} className="note">
                    <h3 className="title">
                        Motivation
                    </h3>
                    <p className="text">
                        Looking on github C# repositories lots
                        of them did not have any documentation
                        online. Moreover there is no simple
                        way to host .NET API explorer
                        in the most ease way - from nuget
                        packages.
                        DNDocs allows to host API Explorer
                        in 1 minute without future maintenance
                    </p>
                </Grid>
                <Grid item md={12} lg={4} className="note">
                    <h3 className="title">Solution</h3>
                    <p className="text">
                        DNDocs allows to host .NET Core
                        API explorer online in 1 minute.
                        No need for future maintenance.
                        System automatically downloads
                        latest Nuget packages, docs from github and
                        rebuild documentation online
                    </p>
                </Grid>
                <Grid item md={12} lg={4} className="note">
                    <h3 className="title">
                        DNDocs
                    </h3>
                    <p className="text">
                        DNDocs is a free online documentation hosting platform
                        for .NET Core projects. DNDocs allows to host
                        *.dll and *.xml API Explorer using Docfx documents generator.
                        Additionally includes markdown docs from github repository
                        in &apos;Articles&apos; section in generated documentation.
                    </p>
                </Grid>
            </Grid>
        );
    }

    return (
        <div className="home">
            <header className="header">
                <h1>DNDocs</h1>
                <Button href={Urls.account.details}><PersonIcon sx={{ color: 'white' }} fontSize="large" /></Button>
                <div className="logo">
                    <div className="nletter">.N</div>
                </div>
            </header>
            <main className="main">
                <Grid container spacing={1}>
                    <Grid item xs={12} lg={9}>
                        {main1()}
                    </Grid>
                    <Grid item xs={12} lg={3}>{main2()}</Grid>
                    <Grid item xs={12} lg={12}>{main3()}</Grid>
                </Grid>
            </main>
        </div>
    );
}