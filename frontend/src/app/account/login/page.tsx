'use client'
import { Box, Card, CardActionArea, CardContent, CardMedia, Paper, Stack, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import GitHubIcon from '@mui/icons-material/GitHub';
import Layout from '@/components/Layout';
import Link from '@mui/material/Link';
import Urls from 'config/Urls';
import useGlobalAppContext from 'hooks/useGlobalAppContext';
import Config from 'config/Config';

export default function Index() {
  const githubUrl = Config?.githubOAuthLoginUrl; //process.env.NEXT_PUBLIC_GITHUB_OAUTH_URL;
  const user = useGlobalAppContext().user;

  const isloggedin = user.isAuthenticated;

  if (isloggedin) {
    return (<h1>Aleardy loged in</h1>)
  }

  return (
    <Layout title="Login">
      <Paper sx={{padding: 2}}>
        <Stack spacing={2}>
          <Link href={githubUrl}>
            <Button size="large" fullWidth variant="outlined" startIcon={<GitHubIcon fontSize="large"/>}>Login with Github</Button>
          </Link>
          <Typography textAlign="center" variant="body2">
            By Login You aggree with our <Link href={Urls.home.termsOfService}>Terms of service</Link>
          </Typography>
        </Stack>
      </Paper>
    </Layout>
  );
}

// <a href="https://www.github.com/" style={{ textDecoration: 'none', display: 'block', width: '30%', margin: '4rem auto' }}>
    //   <Card variant="outlined" sx={{ display: 'flex',  }}>
    //     <CardActionArea>
    //       <CardMedia sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
    //         <GitHubIcon fontSize='large' sx={{ paddingTop: '0.5rem' }} />
    //       </CardMedia>

    //       <CardContent>
    //         <Typography variant="h6" align='center'>Login with Github</Typography>
    //       </CardContent>
    //     </CardActionArea>
    //   </Card>
    // </a>