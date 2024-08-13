'use client'
import Layout from "@/components/Layout";
import { Divider, Typography } from "@mui/material";
import { HelpData, Field, HelpSection } from "./HelpData";
import React from "react";
import Typoheader from "@/components/Typoheader";

export default function Page() {
    function RenderField(field: Field) : any {
        return (
            <React.Fragment>
                <Typography variant="h6">{field.name}</Typography>
                <Typography variant="body1" marginBottom={1}>{field.description}</Typography>
                <Divider />
            </React.Fragment>
        )
    }

    function RenderSection(section: HelpSection) :any {
        return (
            <React.Fragment key={section.name}>
                {/* <Typography vriant="h3">{section.name}</Typography> */}
                <Typoheader variant="h5">{section.name}</Typoheader>
                <Typography variant="body1">{section.description}</Typography>
                {section.fields.map(f => RenderField(f))}
            </React.Fragment>
        );
    }

    return (
        <Layout title="Help">    
            {HelpData.map(d => RenderSection(d as HelpSection))}
        </Layout>
    );
}