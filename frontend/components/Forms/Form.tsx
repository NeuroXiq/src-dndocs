'use client';
import { Alert, Box, Button, Grid, Paper, Stack, Typography } from "@mui/material";
import { getApiErrorsForm } from "./FormHelpers";

export default function Form(props: any) {
    let formErrors = getApiErrorsForm(props.autoApiResult);

    function onSubmit(e: any) {
        e.preventDefault();
        props.submitButton?.onSubmit(e);
    }

    function topErrors() {
        if (formErrors.errors.length === 0 && formErrors.fieldErrors.length === 0) {
            return null;
        }

        return (
            <Alert severity="error" sx={{ mb: 2 }}>
                Errors occured: <br />
                {formErrors.errors.map((e: any) => { return (<div key={e}>{e}</div>) })}
                {formErrors.fieldErrors.map((e: any) => { return (<div key={e.fieldName}>{e.fieldName}: {e.errors.join(', ')}</div>) })}
            </Alert>
        );
    }

    return (
        <Paper>
            {props.title && <Typography bgcolor="primary.main" color="white" variant="h4" align="center" padding={'0.5rem'}>{props.title}</Typography>}
            <Box padding="1rem">
                <form>
                    {topErrors()}
                    <Stack gap={0.5}>
                        {props.children}
                        {props.submitButton && <Button type="submit" onClick={onSubmit} variant="contained">Submit</Button>}
                    </Stack>
                </form>
            </Box>
        </Paper>
    );
}