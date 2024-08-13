import { Box, Button, Typography } from "@mui/material";
import { useState } from "react";


export default function UploadFilesButton(props: any) {
    const multiple = props.multiple == null ? true : props.multiple;

    const [filesCount, setFilesCount] = useState<any>(0);

    const onChange = function (e: any) {
        setFilesCount(e.target.files.length);
    }

    let text = filesCount === 0 ? 'Upload files' : filesCount + ' File(s) selected';

    return (
        <>
            <Button onChange={onChange} variant="outlined" component="label" disabled={!!props.disabled}>
                {text}
                <input multiple={multiple} type="file" hidden onChange={props.onChange} name={props.name} />
            </Button>
            <Box>
                <Typography color="error" sx={{ fontSize: '0.75rem', ml: 2, mt: 0.5 }} component="p">
                    {props.errorText}
                </Typography>
            </Box>
        </>
    );
}