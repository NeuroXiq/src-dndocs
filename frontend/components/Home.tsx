import "@fontsource/space-grotesk/400.css"; // Specify weight
import { Button, Card, CardContent, Divider, Grid, Stack, Typography } from "@mui/material";
import Image from 'next/image';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import PersonIcon from '@mui/icons-material/Person';
import '../css/home.css';
import SendIcon from '@mui/icons-material/Send';
import GitHubIcon from '@mui/icons-material/GitHub';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import Urls from "config/Urls";
import useGlobalAppContext from "hooks/useGlobalAppContext";
import CookieConsent from "./Shared/CookieConsent";

export default function Home() {
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
                        href={Urls.other.appGithubProjectRepository}
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