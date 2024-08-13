'use client';
import { Paper, Typography } from "@mui/material";

export default function Typoheader(props: any) {
    let variant = props.variant || 'h4';
    const mode = props.mode || 'primary';
    const margin = props.margin || '1rem 0';
    
    let bgcolor = "primary.main";
    let color = "primary.contrastText";
    let border = null;
    let borderColor = null;

    if (props.mode === 'error') {
        bgcolor = "error.main";
        color = "error.contrastText";
    }

    bgcolor = props.bgcolor || bgcolor;
    color = props.color || color;

    return (
        <Paper sx={{ margin: margin, backgroundColor: bgcolor, border: border, borderColor: borderColor }}>
            <Typography
                variant={variant}
                padding={1}
                align="center"
                color={color}>{props.children}</Typography>
        </Paper>
    );
}