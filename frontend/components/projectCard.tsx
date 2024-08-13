import { Box, Button, Card, CardActions, CardContent, Divider, Link, Paper, Typography } from "@mui/material";
import GitHubIcon from '@mui/icons-material/GitHub';
import LinkIcon from '@mui/icons-material/Link';
import Urls from "config/Urls";

export default function ProjectCard(props: { project: any }) {
    const project = props.project;
    const title = project.projectName;
    const description = project.description;
    const githubUrl = project.githubUrl;

    return (
        <Paper elevation={0} variant="outlined">
            <Typography variant="h6" bgcolor="secondary.main" color="primary.contrastText" padding={1}>
                {title}
            </Typography>
            <Box sx={{ p: 1 }}>
                <Typography variant="body1" margin={1} sx={{ wordBreak: "break-all" }} height="10em" overflow="auto">
                    {description}
                </Typography>
                <Divider/>
                <Box sx={{ marginTop: 1, display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                    <Link target="_blank" href={githubUrl}>
                        <Button startIcon={<GitHubIcon />} size="medium" variant="outlined">Github </Button>
                    </Link>
                    <Link target="_blank" href={Urls.robiniadocsUrl(project.robiniaUrlPrefix)}>
                        <Button startIcon={<LinkIcon />} size="medium" variant="contained">RobiniaDocs</Button>
                    </Link>
                </Box>
            </Box>
        </Paper>
    )
}