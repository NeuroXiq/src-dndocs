'use client'
import { Box, Grid } from "@mui/material";
import Typoheader from "./Typoheader";

export default function Layout(props: any) {
    props = props || {};
    
    if (props.type === '') {
    }

    return (
        //sx={{ minHeight: '100vh' }}
        <Grid container >
            <Grid item xl={2} lg={2} md={0}></Grid>
            <Grid item xl={8} lg={8} md={12}>
                {props.title && <Typoheader>{props.title}</Typoheader>}
                <Box>{props.children}</Box>
            </Grid>
            <Grid item xl={2} lg={2} md={0}></Grid>
        </Grid>
    );
}